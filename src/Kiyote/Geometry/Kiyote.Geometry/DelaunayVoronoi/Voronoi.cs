namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class Voronoi : IVoronoi {

	public Voronoi(
		IReadOnlyList<Point> points,
		IReadOnlyList<Cell> cells,
		IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> neighbours,
		IReadOnlyList<Edge> edges
	) {
		Points = points;
		Cells = cells;
		Neighbours = neighbours;
		Edges = edges;
	}

	public IReadOnlyList<Point> Points { get; }

	public IReadOnlyList<Cell> Cells { get; }

	public IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> Neighbours { get; }

	public IReadOnlyList<Edge> Edges { get; }
}
