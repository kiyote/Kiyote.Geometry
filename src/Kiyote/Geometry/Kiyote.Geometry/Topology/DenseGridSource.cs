namespace Kiyote.Geometry.Topology;

/// <summary>
/// A fixed-size, row-major source that tracks occupancy with a bitmask.
/// </summary>
public sealed class DenseGridSource<TCell> : IGridSource<TCell> {

	private readonly TCell[] _cells;
	private readonly ulong[] _occupied;
	private readonly CellRun[][] _rowRuns;
	private readonly bool[] _rowDirty;

	public DenseGridSource(
		int width,
		int height
	) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( width );
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero( height );

		Width = width;
		Height = height;
		_cells = new TCell[width * height];
		_occupied = new ulong[( ( width * height ) + 63 ) >> 6];
		_rowRuns = new CellRun[height][];
		_rowDirty = new bool[height];
		for( int i = 0; i < height; i++ ) {
			_rowRuns[i] = [];
		}
	}

	public int Width { get; }

	public int Height { get; }

	public ReadOnlySpan<CellRun> GetOccupiedRuns(
		int row
	) {
		if( (uint)row >= (uint)Height ) {
			return [];
		}
		if( _rowDirty[row] ) {
			_rowRuns[row] = BuildRuns( row );
			_rowDirty[row] = false;
		}
		return _rowRuns[row];
	}

	public bool IsOccupied(
		int column,
		int row
	) {
		if( (uint)column >= (uint)Width
			|| (uint)row >= (uint)Height
		) {
			return false;
		}
		int index = ( row * Width ) + column;
		return ( _occupied[index >> 6] & ( 1UL << index ) ) != 0;
	}

	public ref TCell GetCell(
		int column,
		int row
	) {
		return ref _cells[GetIndex( column, row )];
	}

	public bool TryGetRow(
		int row,
		out Span<TCell> cells
	) {
		if( (uint)row >= (uint)Height ) {
			cells = [];
			return false;
		}
		cells = _cells.AsSpan( row * Width, Width );
		return true;
	}

	public bool TrySetCell(
		int column,
		int row,
		in TCell cell
	) {
		if( (uint)column >= (uint)Width
			|| (uint)row >= (uint)Height
		) {
			return false;
		}
		int index = GetIndex( column, row );
		_cells[index] = cell;
		_occupied[index >> 6] |= 1UL << index;
		_rowDirty[row] = true;
		return true;
	}

	public bool TryClearCell(
		int column,
		int row
	) {
		if( (uint)column >= (uint)Width
			|| (uint)row >= (uint)Height
		) {
			return false;
		}
		int index = GetIndex( column, row );
		_cells[index] = default!;
		_occupied[index >> 6] &= ~( 1UL << index );
		_rowDirty[row] = true;
		return true;
	}

	private int GetIndex(
		int column,
		int row
	) {
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( (uint)column, (uint)Width, nameof( column ) );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( (uint)row, (uint)Height, nameof( row ) );
		return ( row * Width ) + column;
	}

	private CellRun[] BuildRuns(
		int row
	) {
		List<CellRun> runs = [];
		int start = -1;
		for( int column = 0; column < Width; column++ ) {
			if( IsOccupied( column, row ) ) {
				if( start < 0 ) {
					start = column;
				}
			} else if( start >= 0 ) {
				runs.Add( new CellRun( start, row, column - start ) );
				start = -1;
			}
		}
		if( start >= 0 ) {
			runs.Add( new CellRun( start, row, Width - start ) );
		}
		return [.. runs];
	}
}
