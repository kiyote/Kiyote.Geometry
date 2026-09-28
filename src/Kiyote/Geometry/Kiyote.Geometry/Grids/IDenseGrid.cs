namespace Kiyote.Geometry.Grids;

/// <summary>
/// A leaf <see cref="IMutableGrid{T}"/> whose cells are stored contiguously in
/// row-major order, allowing hot loops to operate on the storage directly
/// rather than through the indexer.
/// </summary>
public interface IDenseGrid<T> : IMutableGrid<T> {

	/// <summary>
	/// The backing cells in row-major order.  The cell at local
	/// (<c>column - Column</c>, <c>row - Row</c>) is found at
	/// <c>(localRow * Stride) + localColumn</c>.
	/// </summary>
	Span<T?> Cells { get; }

	/// <summary>
	/// The number of elements between the start of consecutive rows.
	/// </summary>
	int Stride { get; }
}
