namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed record Voronoi(
	IReadOnlyList<Point> Points,
	IReadOnlyList<Cell> Cells,
	IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> Neighbours,
	IReadOnlyList<Edge> Edges
) : IVoronoi;
