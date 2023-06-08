namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IVoronoi {

	IReadOnlyList<Cell> Cells { get; }

	IReadOnlyList<Point> Points { get; }

	IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> Neighbours { get; }

	IReadOnlyList<Edge> Edges { get; }

}
