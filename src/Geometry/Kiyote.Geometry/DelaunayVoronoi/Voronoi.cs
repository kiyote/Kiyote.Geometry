namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class Voronoi : IVoronoi {

	public Voronoi(
		IReadOnlyList<Edge> edges,
		IReadOnlyList<Cell> cells,
		IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> neighbours
	) {
		Edges = edges;
		Cells = cells;
		Neighbours = neighbours;
		ClosedCells = cells.Where( c => !c.IsOpen ).ToList();
		OpenCells = cells.Where( c => c.IsOpen ).ToList();
	}

	public IReadOnlyList<Edge> Edges { get; }

	public IReadOnlyList<Cell> Cells { get; }

	public IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> Neighbours { get; }

	public IReadOnlyList<Cell> ClosedCells { get; }

	public IReadOnlyList<Cell> OpenCells { get; }
}

