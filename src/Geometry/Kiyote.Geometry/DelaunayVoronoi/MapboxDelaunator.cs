namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed record MapboxDelaunator(
	int[] Hull,
	int[] Triangles,
	int[] HalfEdges
);
