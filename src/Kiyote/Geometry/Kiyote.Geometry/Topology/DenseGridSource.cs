namespace Kiyote.Geometry.Topology;

/// <summary>
/// A fixed-size, row-major source that tracks occupancy with a bitmask.
/// </summary>
public sealed class DenseGridSource<TCell> : IGridSource<TCell> {

	private readonly TCell[] _cells;
	private readonly ulong[] _occupied;
	private readonly CellRun[][] _rowRuns;
	private readonly int[] _rowRunCounts;
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
		_rowRunCounts = new int[height];
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
			BuildRuns( row );
			_rowDirty[row] = false;
		}
		return _rowRuns[row].AsSpan( 0, _rowRunCounts[row] );
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

	/// <summary>
	/// Rebuilds the runs of a row in place.  The row's array only grows, so
	/// steady-state edits do not allocate.
	/// </summary>
	private void BuildRuns(
		int row
	) {
		CellRun[] runs = _rowRuns[row];
		int count = 0;
		int start = -1;
		int index = row * Width;
		for( int column = 0; column < Width; column++, index++ ) {
			if( ( _occupied[index >> 6] & ( 1UL << index ) ) != 0 ) {
				if( start < 0 ) {
					start = column;
				}
			} else if( start >= 0 ) {
				Append( ref runs, ref count, new CellRun( start, row, column - start ) );
				start = -1;
			}
		}
		if( start >= 0 ) {
			Append( ref runs, ref count, new CellRun( start, row, Width - start ) );
		}
		_rowRuns[row] = runs;
		_rowRunCounts[row] = count;
	}

	private void Append(
		ref CellRun[] runs,
		ref int count,
		CellRun run
	) {
		if( count == runs.Length ) {
			// A row holds at most ceil( Width / 2 ) runs.
			int capacity = Math.Min( Math.Max( 4, runs.Length * 2 ), ( Width + 1 ) / 2 );
			Array.Resize( ref runs, capacity );
		}
		runs[count++] = run;
	}
}
