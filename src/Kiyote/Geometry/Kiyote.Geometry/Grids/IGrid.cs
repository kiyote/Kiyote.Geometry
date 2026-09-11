namespace Kiyote.Geometry.Grids;

public interface IGrid<T> {

	bool TryAttach(
		IGrid<T> grid,
		int column,
		int row
	);

	bool TryDetach(
		IGrid<T> grid
	);

	T? this[int column, int row] { get; }

	int Column { get; }

	int Row { get; }

	int Width { get; }

	int Height { get; }

	IGrid<T>? GetGrid(
		int column,
		int row
	);

	IGrid<T>? GetGrid(
		int column,
		int row,
		bool recursive
	);

	/// <summary>
	/// Invokes <paramref name="visitor"/> for each grid found at the supplied location,
	/// descending from the grid directly attached to this one through to the child-most
	/// grid at that location.
	/// </summary>
	/// <param name="column">The column, in this grid's coordinate space.</param>
	/// <param name="row">The row, in this grid's coordinate space.</param>
	/// <param name="visitor">
	/// Invoked with each grid encountered along with the column and row of the location
	/// expressed in that grid's own coordinate space.  Not invoked at all when no grid
	/// occupies the location.
	/// </param>
	void VisitGrids(
		int column,
		int row,
		Action<IGrid<T>, int, int> visitor
	);
}
