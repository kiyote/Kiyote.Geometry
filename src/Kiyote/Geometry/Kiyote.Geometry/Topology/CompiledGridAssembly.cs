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

	public IGridLayer<T> CreateLayer<T>(
		int halo
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );

		GridLayer<T> layer = new GridLayer<T>( _space, halo );
		AddLayer( layer );
		return layer;
	}

	public bool RemoveLayer(
		IGridLayer layer
	) {
		if( !_layers.Remove( layer ) ) {
			return false;
		}
		_storage.Remove( (IGridLayerStorage)layer );
		if( layer is IBoundGridLayer<TCell> bound ) {
			_bound.Remove( bound );
		}
		return true;
	}

	public void Swap<T>(
		IGridLayer<T> a,
		IGridLayer<T> b
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );

		if( a is not GridLayer<T> left || !_layers.Contains( a ) ) {
			throw new ArgumentException( "Layer does not belong to this assembly.", nameof( a ) );
		}
		if( b is not GridLayer<T> right || !_layers.Contains( b ) ) {
			throw new ArgumentException( "Layer does not belong to this assembly.", nameof( b ) );
		}
		if( left.Halo != right.Halo ) {
			throw new ArgumentException( "Layers must have the same halo.", nameof( b ) );
		}
		left.SwapStorage( right );
	}

	public IGridLayer<Direction> CreateVacuumLayer(
		int halo
	) {
		ObjectDisposedException.ThrowIf( _disposed, this );

		GridLayer<Direction> result = new GridLayer<Direction>( _space, halo );
		int length = GridLayer<byte>.GetLength( _space, 1 );
		byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent( length );
		try {
			GridLayer<byte> occupied = new GridLayer<byte>( _space, 1, buffer );
			int size = _space.ChunkSize;
			int shift = _space.ChunkShift;
			int cellCount = size * size;
			for( int slot = 0; slot < _space.SlotCount; slot++ ) {
				ReadOnlySpan<ulong> mask = _space.GetValidityMask( slot );
				for( int localIndex = 0; localIndex < cellCount; localIndex++ ) {
					if( ( mask[localIndex >> 6] & ( 1UL << localIndex ) ) != 0 ) {
						buffer[occupied.IndexOf( slot, localIndex )] = 1;
					}
				}
			}
			occupied.ExchangeHalos();

			int stride = occupied.Stride;
			Span<Direction> cells = result.Cells;
			for( int slot = 0; slot < _space.SlotCount; slot++ ) {
				for( int row = 0; row < size; row++ ) {
					int centre = occupied.IndexOf( slot, row << shift );
					int target = result.IndexOf( slot, row << shift );
					for( int column = 0; column < size; column++ ) {
						int i = centre + column;
						if( buffer[i] == 0 ) {
							continue;
						}
						int vacuum = buffer[i - stride] ^ 1;
						vacuum |= ( buffer[i - stride + 1] ^ 1 ) << 1;
						vacuum |= ( buffer[i + 1] ^ 1 ) << 2;
						vacuum |= ( buffer[i + stride + 1] ^ 1 ) << 3;
						vacuum |= ( buffer[i + stride] ^ 1 ) << 4;
						vacuum |= ( buffer[i + stride - 1] ^ 1 ) << 5;
						vacuum |= ( buffer[i - 1] ^ 1 ) << 6;
						vacuum |= ( buffer[i - stride - 1] ^ 1 ) << 7;
						cells[target + column] = (Direction)vacuum;
					}
				}
			}
		} finally {
			System.Buffers.ArrayPool<byte>.Shared.Return( buffer );
		}
		result.ExchangeHalos();

		AddLayer( result );
		return result;
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
		AddRun( run );

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
			if( before > 0 ) {
				_runs[i] = run with { Length = before };
				if( after > 0 ) {
					_runs.Add( run with {
						SourceColumn = run.SourceColumn + before + 1,
						LocalIndex = localIndex + 1,
						Length = after
					} );
				}
			} else if( after > 0 ) {
				_runs[i] = run with {
					SourceColumn = run.SourceColumn + 1,
					LocalIndex = localIndex + 1,
					Length = after
				};
			} else {
				RemoveRunAt( i );
			}
			break;
		}

		Span<ChunkSeamLink> links = System.Runtime.InteropServices.CollectionsMarshal.AsSpan( _seamLinks );
		int kept = 0;
		for( int i = 0; i < links.Length; i++ ) {
			ref ChunkSeamLink link = ref links[i];
			if( ( link.Slot == slot && link.LocalIndex == localIndex )
				|| ( link.NeighbourSlot == slot && link.NeighbourLocalIndex == localIndex )
			) {
				continue;
			}
			if( kept != i ) {
				links[kept] = link;
			}
			kept++;
		}
		if( kept != links.Length ) {
			_seamLinks.RemoveRange( kept, links.Length - kept );
		}
	}

	/// <summary>
	/// Adds a run, merging it with any run it directly continues or precedes
	/// so that repeated removal and re-adding of cells does not fragment
	/// <see cref="SourceRuns"/>.
	/// </summary>
	private void AddRun(
		SourceRun run
	) {
		int shift = _space.ChunkShift;
		int localRow = run.LocalIndex >> shift;
		int previous = -1;
		int next = -1;
		for( int i = 0; i < _runs.Count; i++ ) {
			SourceRun other = _runs[i];
			if( other.Slot != run.Slot
				|| other.Placement != run.Placement
				|| other.SourceRow != run.SourceRow
				|| ( other.LocalIndex >> shift ) != localRow
			) {
				continue;
			}
			if( other.LocalIndex + other.Length == run.LocalIndex
				&& other.SourceColumn + other.Length == run.SourceColumn
			) {
				previous = i;
			} else if( run.LocalIndex + run.Length == other.LocalIndex
				&& run.SourceColumn + run.Length == other.SourceColumn
			) {
				next = i;
			}
			if( previous >= 0 && next >= 0 ) {
				break;
			}
		}

		if( previous >= 0 && next >= 0 ) {
			SourceRun first = _runs[previous];
			_runs[previous] = first with { Length = first.Length + run.Length + _runs[next].Length };
			RemoveRunAt( next );
		} else if( previous >= 0 ) {
			SourceRun first = _runs[previous];
			_runs[previous] = first with { Length = first.Length + run.Length };
		} else if( next >= 0 ) {
			SourceRun last = _runs[next];
			_runs[next] = run with { Length = run.Length + last.Length };
		} else {
			_runs.Add( run );
		}
	}

	private void RemoveRunAt(
		int index
	) {
		int last = _runs.Count - 1;
		_runs[index] = _runs[last];
		_runs.RemoveAt( last );
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
