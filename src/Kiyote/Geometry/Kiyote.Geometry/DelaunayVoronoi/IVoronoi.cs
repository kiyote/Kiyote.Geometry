namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IVoronoi {

	IReadOnlyList<Cell> Cells { get; }

	IReadOnlyList<Point> Points { get; }

	/// <summary>Returns indices into <see cref="Cells"/> for the given cell's ordered Delaunay neighbours, without allocating.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The index is outside the cell collection.</exception>
	ReadOnlySpan<int> GetNeighbourIndices( int cellIndex );

	IReadOnlyList<Edge> Edges { get; }

}
