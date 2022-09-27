namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IVoronoi {

	IReadOnlyList<Cell> Cells { get; }

	IReadOnlyList<IEdge> Edges { get; }

	IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> Neighbours { get; }

	IReadOnlyList<Cell> OpenCells { get; }

	IReadOnlyList<Cell> ClosedCells { get; }
}
