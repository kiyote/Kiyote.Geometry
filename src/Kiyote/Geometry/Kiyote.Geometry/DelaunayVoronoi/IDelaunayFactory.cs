namespace Kiyote.Geometry.DelaunayVoronoi;

/// <summary>Creates Delaunay triangulations from bounded integer sites, owning stable, read-only result storage.</summary>
public interface IDelaunayFactory {

	/// <summary>Requires at least three distinct, noncollinear sites within GeometryLimits.</summary>
	IDelaunay Create(
		IReadOnlyList<Point> points
	);

	/// <summary>Optionally deduplicates sites in first occurrence order, then requires a nondegenerate site set.</summary>
	/// <remarks>Sanitization never permits out-of-domain coordinates.</remarks>
	/// <exception cref="ArgumentException">Sites are insufficient, collinear, or duplicate without sanitization.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Coordinates or required array capacities are unsupported.</exception>
	IDelaunay Create(
		IReadOnlyList<Point> points,
		bool sanitizePoints
	);

}
