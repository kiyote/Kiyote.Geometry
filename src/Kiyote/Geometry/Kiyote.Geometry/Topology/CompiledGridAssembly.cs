namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="ICompiledGridAssembly{TCell}"/>.  When compiled from a
/// <see cref="GridAssembly{TCell}"/>, per-cell edits are applied to the chunk
/// layout and to every layer as they happen.  Not thread-safe.
/// </summary>
internal sealed class CompiledGridAssembly<TCell> : ICompiledGridAssembly<TCell>, IGridAssemblyObserver {

	private readonly GridChunkLayout _space;
	private readonly GridAssembly<TCell>? _observed;
	private readonly List<SourceRun> _runs;
	private readonly List<ChunkSeamLink> _seamLinks;
	private readonly List<IGridLayer> _layers;
	private readonly List<IGridLayerStorage> _storage;
	private readonly List<IBoundGridLayer<TCell>> _bound;
	private readonly Dictionary<PlacementId, IGridSource<TCell>> _sources;
	private bool _disposed;

	public CompiledGridAssembly(
		IGridAssembly<TCell> assembly,
		int chunkSize
	) {
		Assembly = assembly;
		Version = assembly.Version;
		_space = new GridChunkLayout( chunkSize );
		_runs = [];
		_seamLinks = [];
		_layers = [];
		_storage = [];
		_bound = [];
		_sources = [];

		foreach( IGridPlacement<TCell> placement in assembly.Placements ) {
			_sources[placement.Id] = placement.Source;
			IGridSource<TCell> source = placement.Source;
			for( int row = 0; row < source.Height; row++ ) {
				foreach( CellRun run in source.GetOccupiedRuns( row ) ) {
					AddRuns( placement, run );
				}
			}
		}

		foreach( GridSeam seam in assembly.Seams ) {
			foreach( GridContact contact in seam.Contacts ) {
				AddSeamLink( contact );
			}
		}

		_observed = assembly as GridAssembly<TCell>;
		_observed?.AddObserver( this );
	}

	public IGridAssembly<TCell> Assembly { get; }

	public int Version { get; }

	public bool IsStale => Assembly.Version != Version;

	public IGridChunkLayout ChunkLayout => _space;

	public ReadOnlySpan<SourceRun> SourceRuns => System.Runtime.InteropServices.CollectionsMarshal.AsSpan( _runs );

	public ReadOnlySpan<ChunkSeamLink> SeamLinks => System.Runtime.InteropServices.CollectionsMarshal.AsSpan( _seamLinks );

	public IReadOnlyList<IGridLayer> Layers => _layers;

	internal GridChunkLayout GridChunkLayout => _space;

	public IGridLayer<T> Bind<T, TBinding>(
		TBinding binding,
		int halo
	) where TBinding : struct, IGridLayerBinding<TCell, T> {
		ObjectDisposedException.ThrowIf( _disposed, this );

		BoundGridLayer<TCell, T, TBinding> layer = new BoundGridLayer<TCell, T, TBinding>( _space, binding, halo );
		foreach( SourceRun run in _runs ) {
			layer.Load( _sources[run.Placement], run );
		}
		layer.ExchangeHalos();

		_layers.Add( layer );
		_storage.Add( layer );
		_bound.Add( layer );
		return layer;
	}

	public void Commit() {
		ObjectDisposedException.ThrowIf( _disposed, this );

		foreach( IBoundGridLayer<TCell> layer in _bound ) {
			foreach( SourceRun run in _runs ) {
				if( layer.IsDirty( run.Slot ) ) {
					layer.Store( _sources[run.Placement], run );
				}
			}
			layer.ClearDirty();
		}
	}

	public void Dispose() {
		if( _disposed ) {
			return;
		}
		_observed?.RemoveObserver( this );
		_disposed = true;
	}

	/// <summary>
	/// Registers a layer that is not bound to the sources, such as a derived
	/// connectivity layer, so that it is resized by cell edits.
	/// </summary>
	internal void AddLayer<T>(
		GridLayer<T> layer
	) {
		_layers.Add( layer );
		_storage.Add( layer );
	}

	void IGridAssemblyObserver.OnCellAdded(
		PlacementId placement,
		int column,
		int row,
		ReadOnlySpan<GridContact> contacts
	) {
		if( !_sources.TryGetValue( placement, out IGridSource<TCell>? source ) ) {
			if( !Assembly.TryGetPlacement( placement, out IGridPlacement<TCell>? found ) ) {
				return;
			}
			source = found.Source;
			_sources[placement] = source;
		}
		Assembly.TryGetPlacement( placement, out IGridPlacement<TCell>? owner );

		int slotCount = _space.SlotCount;
		int slot = _space.GetOrAllocateSlot( column, row );
		int localIndex = _space.GetLocalIndex( column, row );
		_space.SetOccupied( slot, localIndex, true );
		if( _space.SlotCount != slotCount ) {
			foreach( IGridLayerStorage layer in _storage ) {
				layer.EnsureSlots();
			}
		}

		SourceRun run = new SourceRun(
			placement,
			column - owner!.Column,
			row - owner.Row,
			slot,
			localIndex,
			1
		);
		_runs.Add( run );

		foreach( GridContact contact in contacts ) {
			AddSeamLink( contact );
		}

		foreach( IBoundGridLayer<TCell> layer in _bound ) {
			layer.Load( source, run );
		}
	}

	void IGridAssemblyObserver.OnCellRemoved(
		PlacementId placement,
		int column,
		int row
	) {
		if( !_space.TryGetCell( column, row, out int slot, out int localIndex ) ) {
			return;
		}

		foreach( IGridLayerStorage layer in _storage ) {
			layer.Reset( slot, localIndex );
		}
		_space.SetOccupied( slot, localIndex, false );

		for( int i = 0; i < _runs.Count; i++ ) {
			SourceRun run = _runs[i];
			if( run.Slot != slot
				|| localIndex < run.LocalIndex
				|| localIndex >= run.LocalIndex + run.Length
			) {
				continue;
			}

			int before = localIndex - run.LocalIndex;
			int after = run.Length - before - 1;
			_runs.RemoveAt( i );
			if( before > 0 ) {
				_runs.Add( run with { Length = before } );
			}
			if( after > 0 ) {
				_runs.Add( run with {
					SourceColumn = run.SourceColumn + before + 1,
					LocalIndex = localIndex + 1,
					Length = after
				} );
			}
			break;
		}

		_seamLinks.RemoveAll( link =>
			( link.Slot == slot && link.LocalIndex == localIndex )
			|| ( link.NeighbourSlot == slot && link.NeighbourLocalIndex == localIndex )
		);
	}

	/// <summary>
	/// Adds the source runs
	/// chunk boundaries.
	/// </summary>
	private void AddRuns(
		IGridPlacement<TCell> placement,
		CellRun sourceRun
	) {
		int row = sourceRun.Row + placement.Row;
		int column = sourceRun.Column + placement.Column;
		int remaining = sourceRun.Length;
		int sourceColumn = sourceRun.Column;
		int size = _space.ChunkSize;

		while( remaining > 0 ) {
			int inChunk = size - ( column & ( size - 1 ) );
			int length = Math.Min( inChunk, remaining );
			int slot = _space.GetOrAllocateSlot( column, row );
			int localIndex = _space.GetLocalIndex( column, row );
			for( int i = 0; i < length; i++ ) {
				_space.SetOccupied( slot, localIndex + i, true );
			}
			_runs.Add( new SourceRun( placement.Id, sourceColumn, sourceRun.Row, slot, localIndex, length ) );

			column += length;
			sourceColumn += length;
			remaining -= length;
		}
	}

	private void AddSeamLink(
		GridContact contact
	) {
		if( _space.TryGetCell( contact.Column, contact.Row, out int slot, out int localIndex )
			&& _space.TryGetCell( contact.NeighbourColumn, contact.NeighbourRow, out int neighbourSlot, out int neighbourLocalIndex )
		) {
			_seamLinks.Add( new ChunkSeamLink( slot, localIndex, neighbourSlot, neighbourLocalIndex, contact.Direction ) );
		}
	}
}
