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

	/// <summary>
	/// Creates a layer over caller-supplied storage, such as a pooled array.
	/// The first <c>SlotCount * ChunkLength</c> elements are cleared.
	/// </summary>
	internal GridLayer(
		GridChunkLayout space,
		int halo,
		T[] cells
	) {
		ArgumentOutOfRangeException.ThrowIfLessThan( halo, 0 );
		ArgumentOutOfRangeException.ThrowIfGreaterThan( halo, 1 );

		_space = space;
		Halo = halo;
		Stride = space.ChunkSize + ( 2 * halo );
		ChunkLength = Stride * Stride;
		ArgumentOutOfRangeException.ThrowIfLessThan( cells.Length, space.SlotCount * ChunkLength );
		Array.Clear( cells, 0, space.SlotCount * ChunkLength );
		_cells = cells;
		_dirty = [];
	}

	/// <summary>
	/// The number of elements needed to back a layer with the given halo.
	/// </summary>
	internal static int GetLength(
		GridChunkLayout space,
		int halo
	) {
		int stride = space.ChunkSize + ( 2 * halo );
		return space.SlotCount * stride * stride;
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
		for( int slot = 0; slot < _space.SlotCount; slot++ ) {
			ExchangeHalo( slot );
		}
	}

	/// <summary>
	/// Refreshes the halo of a single slot from the edge cells of its
	/// neighbours.  No-op when <see cref="Halo"/> is 0.
	/// </summary>
	internal void ExchangeHalo(
		int slot
	) {
		if( Halo == 0 ) {
			return;
		}
		const int north = 0;
		const int northEast = 1;
		const int east = 2;
		const int southEast = 3;
		const int south = 4;
		const int southWest = 5;
		const int west = 6;
		const int northWest = 7;

		int size = _space.ChunkSize;
		int last = size - 1;
		int top = slot * ChunkLength;
		int bottom = top + ( ( size + 1 ) * Stride );
		ReadOnlySpan<int> neighbours = _space.GetNeighbours( slot );
		Span<T> cells = _cells;

		CopyRow( cells, neighbours[north], last, top + 1, size );
		CopyRow( cells, neighbours[south], 0, bottom + 1, size );
		CopyColumn( cells, neighbours[west], last, top + Stride, size );
		CopyColumn( cells, neighbours[east], 0, top + Stride + size + 1, size );
		CopyCell( cells, neighbours[northWest], last, last, top );
		CopyCell( cells, neighbours[northEast], 0, last, top + size + 1 );
		CopyCell( cells, neighbours[southWest], last, 0, bottom );
		CopyCell( cells, neighbours[southEast], 0, 0, bottom + size + 1 );
	}

	private void CopyRow(
		Span<T> cells,
		int source,
		int sourceRow,
		int target,
		int length
	) {
		Span<T> destination = cells.Slice( target, length );
		if( source < 0 ) {
			destination.Clear();
			return;
		}
		cells.Slice( IndexOf( source, sourceRow << _space.ChunkShift ), length ).CopyTo( destination );
	}

	private void CopyColumn(
		Span<T> cells,
		int source,
		int sourceColumn,
		int target,
		int length
	) {
		if( source < 0 ) {
			for( int i = 0; i < length; i++ ) {
				cells[target + ( i * Stride )] = default!;
			}
			return;
		}
		int from = IndexOf( source, sourceColumn );
		for( int i = 0; i < length; i++ ) {
			cells[target + ( i * Stride )] = cells[from + ( i * Stride )];
		}
	}

	private void CopyCell(
		Span<T> cells,
		int source,
		int sourceColumn,
		int sourceRow,
		int target
	) {
		cells[target] = source < 0
			? default!
			: cells[IndexOf( source, ( sourceRow << _space.ChunkShift ) + sourceColumn )];
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
