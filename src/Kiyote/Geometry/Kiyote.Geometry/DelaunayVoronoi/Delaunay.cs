namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed record Delaunay(
	IReadOnlyList<Point> Points,
	IReadOnlyList<Triangle> Triangles,
	IReadOnlyList<Point> Hull
) : IDelaunay;
