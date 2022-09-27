namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed record Delaunay(
	IReadOnlyList<IPoint> Points,
	IReadOnlyList<Edge> Edges,
	IReadOnlyList<Triangle> Triangles
);
