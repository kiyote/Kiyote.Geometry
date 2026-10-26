namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IVoronoi {

	IReadOnlyList<Cell> Cells { get; }

	IReadOnlyList<Point> Points { get; }

	/// <summary>Returns indices into <see cref="Cells"/> for the given cell's ordered Delaunay neighbours, without allocating.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The index is outside the cell collection.</exception>
	ReadOnlySpan<int> GetNeighbourIndices( int cellIndex );

	/// <summary>Gets distinct directed sides of the final quantized polygons; reversed endpoints are distinct edges.</summary>
	IReadOnlyList<Edge> Edges { get; }

}
