namespace Kiyote.Geometry.DelaunayVoronoi;

public record Voronoi(
	IReadOnlyList<Point> Points,
	IReadOnlyList<Cell> Cells,
	IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> Neighbours
) : IVoronoi;
