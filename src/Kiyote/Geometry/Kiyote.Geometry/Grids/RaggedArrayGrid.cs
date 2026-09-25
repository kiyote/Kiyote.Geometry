namespace Kiyote.Geometry.Grids;

/// <summary>
/// A leaf <see cref="IMutableGrid{T}"/> backed by a ragged array of cells.
/// </summary>
public sealed class RaggedArrayGrid<T> : IMutableGrid<T> {

	private readonly T?[][] _cells;

	public RaggedArrayGrid(
		int width,
		int height
	) : this( 0, 0, width, height ) {
	}

	public RaggedArrayGrid(
		int column,
		int row,
		int width,
		int height
	) {
		ArgumentOutOfRangeException.ThrowIfNegative( width );
		ArgumentOutOfRangeException.ThrowIfNegative( height );

		Column = column;
		Row = row;
		Width = width;
		Height = height;

		_cells = new T?[height][];
		for( int r = 0; r < height; r++ ) {
			_cells[r] = new T?[width];
		}
	}

	T? IGrid<T>.this[int column, int row] => ( (IMutableGrid<T>)this )[column, row];

	T? IMutableGrid<T>.this[int column, int row] {
		get {
			if( !TryGetIndices( column, row, out int c, out int r ) ) {
				return default;
			}
			return _cells[r][c];
		}
		set {
			if( !TryGetIndices( column, row, out int c, out int r ) ) {
				throw new InvalidOperationException( "Attempt to access ArrayGrid out of bounds." );
			}
			_cells[r][c] = value;
		}
	}

	int IGrid<T>.Column => Column;

	int IGrid<T>.Row => Row;

	int IGrid<T>.Width => Width;

	int IGrid<T>.Height => Height;

	private int Column { get; }

	private int Row { get; }

	private int Width { get; }

	private int Height { get; }

	bool IGrid<T>.TryAttach(
		IGrid<T> grid,
		int column,
		int row
	) {
		ArgumentNullException.ThrowIfNull( grid );
		return false;
	}

	bool IGrid<T>.TryDetach(
		IGrid<T> grid
	) {
		ArgumentNullException.ThrowIfNull( grid );
		return false;
	}

	IGrid<T>? IGrid<T>.GetGrid(
		int column,
		int row
	) {
		return null;
	}

	IGrid<T>? IGrid<T>.GetGrid(
		int column,
		int row,
		bool recursive
	) {
		return null;
	}

	void IGrid<T>.VisitGrids(
		int column,
		int row,
		Action<IGrid<T>, int, int> visitor
	) {
		ArgumentNullException.ThrowIfNull( visitor );
	}

	private bool TryGetIndices(
		int column,
		int row,
		out int columnIndex,
		out int rowIndex
	) {
		columnIndex = column - Column;
		rowIndex = row - Row;
		if( columnIndex < 0
			|| columnIndex >= Width
			|| rowIndex < 0
			|| rowIndex >= Height
		) {
			columnIndex = 0;
			rowIndex = 0;
			return false;
		}
		return true;
	}
}
