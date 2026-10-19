namespace Kiyote.Geometry.DelaunayVoronoi;

/// <summary>Creates bounded Voronoi diagrams with stable, read-only result storage.</summary>
/// <remarks>
/// Sites must be distinct, noncollinear, and within both GeometryLimits and the clipping rectangle.
/// Clipping bounds must enclose positive geometric area (at least two occupied cells on each axis).
/// Circumcentres and clipping calculations remain unrounded; final vertices are floored to the integer grid.
/// The convex hull of those vertices is the output approximation, not an exact partition of continuous space.
/// </remarks>
public interface IVoronoiFactory {

	/// <summary>Maximum vertices emitted by cell traversal, before projection and clipping.</summary>
	const int MaximumCellVertices = 64;

	/// <summary>Creates a diagram without sanitizing invalid sites.</summary>
	IVoronoi Create(
		Rect bounds,
		IReadOnlyList<Point> points
	);

	/// <summary>Optionally deduplicates sites and discards sites outside the rectangle, preserving first occurrence order.</summary>
	/// <remarks>Sanitization never permits out-of-domain coordinates; the remaining sites must still be nondegenerate.</remarks>
	/// <exception cref="ArgumentException">Sites are duplicate, insufficient, collinear, or outside the rectangle without sanitization.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Coordinates, bounds, or required array capacities are unsupported.</exception>
	/// <exception cref="InvalidOperationException">A cell exceeds the traversal/polygon limit or collapses after quantization.</exception>
	IVoronoi Create(
		Rect bounds,
		IReadOnlyList<Point> points,
		bool sanitizePoints
	);

}
