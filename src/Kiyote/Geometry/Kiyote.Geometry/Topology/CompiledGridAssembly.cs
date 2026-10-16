using System.Numerics;
using System.Runtime.InteropServices;
using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="ICompiledGridAssembly{TCell}"/>.  When compiled from a
/// <see cref="GridAssembly{TCell}"/>, per-cell edits are applied to the chunk
/// layout and to every layer as they happen.  Not thread-safe.
/// </summary>
internal sealed class CompiledGridAssembly<TCell> : ICompiledGridAssembly<TCell>, IGridAssemblyObserver {

	private readonly ChunkedGridLayout _layout;
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
		if( chunkSize < GridCompiler.MinimumChunkSize
			|| chunkSize > ChunkedGridLayout.MaximumChunkSize
			|| !BitOperations.IsPow2( chunkSize )
		) {
			throw new ArgumentOutOfRangeException(
				nameof( chunkSize ),
				chunkSize,
				$"Chunk size must be a power of two between {GridCompiler.MinimumChunkSize} and {ChunkedGridLayout.MaximumChunkSize}."
			);
		}

		Assembly = assembly;
		Version = assembly.Version;
		_layout = new ChunkedGridLayout( chunkSize );
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

	public ChunkedGridLayout ChunkLayout => _layout;

	public ReadOnlySpan<SourceRun> SourceRuns => CollectionsMarshal.AsSpan( _runs );

	public ReadOnlySpan<ChunkSeamLink> SeamLinks => CollectionsMarshal.AsSpan( _seamLinks );

	public IReadOnlyList<IGridLayer> Layers => _layers;

	public IGridLayer<T> Bind<T, TBinding>(
		TBinding binding
	) where T : unmanaged
		where TBinding : struct, IGridLayerBinding<TCell, T> {
		ObjectDisposedException.ThrowIf( _disposed, this );

		BoundGridLayer<TCell, T, TBinding> layer = new BoundGridLayer<TCell, T, TBinding>( _layout, binding );
		foreach( SourceRun run in _runs ) {
			layer.Load( _sources[run.Placement], run );
		}

		_layers.Add( layer );
		_storage.Add( layer );
		_bound.Add( layer );
		return layer;
	}

	public IGridLayer<T> CreateLayer<T>() where T : unmanaged {
		ObjectDisposedException.ThrowIf( _disposed, this );

		GridLayer<T> layer = new GridLayer<T>( _layout );
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
	) where T : unmanaged {
		ObjectDisposedException.ThrowIf( _disposed, this );

		if( a is not GridLayer<T> left || !_layers.Contains( a ) ) {
			throw new ArgumentException( "Layer does not belong to this assembly.", nameof( a ) );
		}
		if( b is not GridLayer<T> right || !_layers.Contains( b ) ) {
			throw new ArgumentException( "Layer does not belong to this assembly.", nameof( b ) );
		}
		if( ReferenceEquals( left, right ) ) {
			throw new ArgumentException( "A layer cannot be swapped with itself.", nameof( b ) );
		}
		left.SwapStorage( right );
	}

	public IGridLayer<Direction> CreateVacuumLayer() {
		ObjectDisposedException.ThrowIf( _disposed, this );

		GridLayer<Direction> result = new GridLayer<Direction>( _layout );
		ChunkedGridLayer<Direction> values = result.Values;
		int size = _layout.ChunkSize;
		int shift = _layout.ChunkShift;
		foreach( int chunkIndex in _layout.EnumerateChunks() ) {
			Span<Direction> cells = values.GetValues( chunkIndex );
			for( int row = 0; row < size; row++ ) {
				for( int column = 0; column < size; column += 64 ) {
					int count = Math.Min( 64, size - column );
					ulong occupied = _layout.ReadPresenceBits( chunkIndex, column, row, count );
					if( occupied == 0 ) {
						continue;
					}
					// Each mask has bit i set when the neighbour of cell column + i is unoccupied.
					ulong north = ~_layout.ReadPresenceBits( chunkIndex, column, row - 1, count );
					ulong northEast = ~_layout.ReadPresenceBits( chunkIndex, column + 1, row - 1, count );
					ulong east = ~_layout.ReadPresenceBits( chunkIndex, column + 1, row, count );
					ulong southEast = ~_layout.ReadPresenceBits( chunkIndex, column + 1, row + 1, count );
					ulong south = ~_layout.ReadPresenceBits( chunkIndex, column, row + 1, count );
					ulong southWest = ~_layout.ReadPresenceBits( chunkIndex, column - 1, row + 1, count );
					ulong west = ~_layout.ReadPresenceBits( chunkIndex, column - 1, row, count );
					ulong northWest = ~_layout.ReadPresenceBits( chunkIndex, column - 1, row - 1, count );
					int start = ( row << shift ) + column;
					while( occupied != 0 ) {
						int i = BitOperations.TrailingZeroCount( occupied );
						occupied &= occupied - 1;
						ulong vacuum = ( ( north >> i ) & 1UL )
							| ( ( ( northEast >> i ) & 1UL ) << 1 )
							| ( ( ( east >> i ) & 1UL ) << 2 )
							| ( ( ( southEast >> i ) & 1UL ) << 3 )
							| ( ( ( south >> i ) & 1UL ) << 4 )
							| ( ( ( southWest >> i ) & 1UL ) << 5 )
							| ( ( ( west >> i ) & 1UL ) << 6 )
							| ( ( ( northWest >> i ) & 1UL ) << 7 );
						cells[start + i] = (Direction)vacuum;
					}
				}
			}
		}

		AddLayer( result );
		return result;
	}

	public void Commit() {
		ObjectDisposedException.ThrowIf( _disposed, this );

		foreach( IBoundGridLayer<TCell> layer in _bound ) {
			foreach( SourceRun run in _runs ) {
				if( layer.IsDirty( run.ChunkIndex ) ) {
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
	/// connectivity layer, so that removed cells are reset in it.
	/// </summary>
	internal void AddLayer<T>(
		GridLayer<T> layer
	) where T : unmanaged {
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

		// Layers allocate storage for new chunks themselves, zeroed, when next used.
		_ = _layout.SetPopulated( column, row );
		_ = _layout.TryLocate( column, row, out int chunkIndex, out int localIndex );

		SourceRun run = new SourceRun(
			placement,
			column - owner!.Column,
			row - owner.Row,
			chunkIndex,
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
		if( !_layout.TryLocate( column, row, out int chunkIndex, out int localIndex ) ) {
			return;
		}

		foreach( IGridLayerStorage layer in _storage ) {
			layer.Reset( chunkIndex, localIndex );
		}
		_ = _layout.ClearPopulated( column, row );

		for( int i = 0; i < _runs.Count; i++ ) {
			SourceRun run = _runs[i];
			if( run.ChunkIndex != chunkIndex
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

		Span<ChunkSeamLink> links = CollectionsMarshal.AsSpan( _seamLinks );
		int kept = 0;
		for( int i = 0; i < links.Length; i++ ) {
			ref ChunkSeamLink link = ref links[i];
			if( ( link.ChunkIndex == chunkIndex && link.LocalIndex == localIndex )
				|| ( link.NeighbourChunkIndex == chunkIndex && link.NeighbourLocalIndex == localIndex )
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
		int shift = _layout.ChunkShift;
		int localRow = run.LocalIndex >> shift;
		int previous = -1;
		int next = -1;
		for( int i = 0; i < _runs.Count; i++ ) {
			SourceRun other = _runs[i];
			if( other.ChunkIndex != run.ChunkIndex
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
	/// Marks a source run occupied in the layout and records it as one
	/// <see cref="SourceRun"/> per chunk it crosses.
	/// </summary>
	private void AddRuns(
		IGridPlacement<TCell> placement,
		CellRun sourceRun
	) {
		int row = sourceRun.Row + placement.Row;
		int column = sourceRun.Column + placement.Column;
		int remaining = sourceRun.Length;
		int sourceColumn = sourceRun.Column;
		int size = _layout.ChunkSize;
		int shift = _layout.ChunkShift;
		int mask = _layout.ChunkMask;
		int localRowStart = ( row & mask ) << shift;

		while( remaining > 0 ) {
			int localColumn = column & mask;
			int length = Math.Min( size - localColumn, remaining );
			int chunkIndex = _layout.GetOrCreateChunk( column >> shift, row >> shift );
			_ = _layout.SetPopulatedRun( column, row, length );
			_runs.Add( new SourceRun( placement.Id, sourceColumn, sourceRun.Row, chunkIndex, localRowStart + localColumn, length ) );

			column += length;
			sourceColumn += length;
			remaining -= length;
		}
	}

	private void AddSeamLink(
		GridContact contact
	) {
		if( _layout.TryLocate( contact.Column, contact.Row, out int chunkIndex, out int localIndex )
			&& _layout.TryLocate( contact.NeighbourColumn, contact.NeighbourRow, out int neighbourChunkIndex, out int neighbourLocalIndex )
		) {
			_seamLinks.Add( new ChunkSeamLink( chunkIndex, localIndex, neighbourChunkIndex, neighbourLocalIndex, contact.Direction ) );
		}
	}
}
