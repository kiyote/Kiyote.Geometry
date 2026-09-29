namespace Kiyote.Geometry.Topology;

/// <summary>
/// Chunked storage laid out over a <see cref="GridChunkLayout"/>.  Each slot holds
/// <c>Stride * Stride</c> elements in row-major order, with interior cells
/// offset by <see cref="Halo"/>.
/// </summary>
internal class GridLayer<T> : IGridLayer<T>, IGridLayerStorage {

	private readonly GridChunkLayout _space;
	private T[] _cells;
	private bool[] _dirty;

	public GridLayer(
		GridChunkLayout space,
		int halo
	) {
		ArgumentOutOfRangeException.ThrowIfLessThan( halo, 0 );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( halo, 1 );

		_space = space;
		Halo = halo;
		Stride = space.ChunkSize + ( 2 * halo );
		ChunkLength = Stride * Stride;
		_cells = new T[space.SlotCount * ChunkLength];
		_dirty = new bool[space.SlotCount];
	}

	public IGridChunkLayout Space => _space;

	public int Halo { get; }

	public int Stride { get; }

	/// <summary>
	/// The number of elements occupied by each slot, including halo cells.
	/// </summary>
	public int ChunkLength { get; }

	public Span<T> Cells => _cells;

	public bool IsDirty(
		int slot
	) {
		return _dirty[slot];
	}

	public void MarkDirty(
		int slot
	) {
		_dirty[slot] = true;
	}

	public void ClearDirty() {
		Array.Clear( _dirty );
	}

	public Span<T> GetChunk(
		int slot
	) {
		return _cells.AsSpan( slot * ChunkLength, ChunkLength );
	}

	public Span<T> GetRow(
		int slot,
		int row
	) {
		return _cells.AsSpan( IndexOf( slot, row << _space.ChunkShift ), _space.ChunkSize );
	}

	public ref T this[int column, int row] {
		get {
			if( !_space.TryGetCell( column, row, out int slot, out int localIndex ) ) {
				throw new ArgumentOutOfRangeException( nameof( column ), $"Cell ( {column}, {row} ) is not occupied." );
			}
			return ref _cells[IndexOf( slot, localIndex )];
		}
	}

	public void ExchangeHalos() {
		if( Halo == 0 ) {
			return;
		}
		int size = _space.ChunkSize;
		int mask = size - 1;
		for( int slot = 0; slot < _space.SlotCount; slot++ ) {
			int baseIndex = slot * ChunkLength;
			for( int hy = -1; hy <= size; hy++ ) {
				int dy = hy < 0 ? -1 : hy >= size ? 1 : 0;
				for( int hx = -1; hx <= size; hx++ ) {
					int dx = hx < 0 ? -1 : hx >= size ? 1 : 0;
					if( dx == 0 && dy == 0 ) {
						hx = size - 1;
						continue;
					}
					int target = baseIndex + ( ( hy + 1 ) * Stride ) + hx + 1;
					int neighbour = _space.GetNeighbour( slot, DirectionExtensions.FromOffset( new Point( dx, dy ) ) );
					_cells[target] = neighbour < 0
						? default!
						: _cells[IndexOf( neighbour, ( ( hy & mask ) << _space.ChunkShift ) + ( hx & mask ) )];
				}
			}
		}
	}

	/// <summary>
	/// Returns the index into <see cref="Cells"/> of an interior cell.
	/// </summary>
	internal int IndexOf(
		int slot,
		int localIndex
	) {
		int shift = _space.ChunkShift;
		int localRow = localIndex >> shift;
		int localColumn = localIndex & ( _space.ChunkSize - 1 );
		return ( slot * ChunkLength ) + ( ( localRow + Halo ) * Stride ) + localColumn + Halo;
	}

	/// <summary>
	/// Grows storage to match the current slot count of the space.
	/// </summary>
	public void EnsureSlots() {
		int slots = _space.SlotCount;
		if( _dirty.Length >= slots ) {
			return;
		}
		Array.Resize( ref _cells, slots * ChunkLength );
		Array.Resize( ref _dirty, slots );
	}

	public void Reset(
		int slot,
		int localIndex
	) {
		_cells[IndexOf( slot, localIndex )] = default!;
	}
}
