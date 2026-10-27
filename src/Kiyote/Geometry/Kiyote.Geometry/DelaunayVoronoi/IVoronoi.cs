namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IVoronoi {

	/// <summary>Gets immutable metadata, indexed by cell ID. Use ref-readonly iteration to avoid copying metadata.</summary>
	ReadOnlySpan<Cell> Cells { get; }

	/// <summary>Gets owned site positions in cell-ID order.</summary>
	IReadOnlyList<Point> Points { get; }

	/// <summary>Returns the canonical positive-winding vertices of a cell without allocating.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The ID is outside the cell collection.</exception>
	ReadOnlySpan<Point> GetCellVertices( int cellIndex );

	/// <summary>Tests inclusive containment of a bounded position without constructing a polygon.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The ID or target position is outside its supported domain.</exception>
	bool Contains( int cellIndex, Point point );

	/// <summary>Explicitly allocates an independently owned polygon, including its edge array.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The ID is outside the cell collection.</exception>
	Polygon ExportPolygon( int cellIndex );

	/// <summary>Returns indices into <see cref="Cells"/> for the given cell's ordered Delaunay neighbours, without allocating.</summary>
	/// <exception cref="ArgumentOutOfRangeException">The index is outside the cell collection.</exception>
	ReadOnlySpan<int> GetNeighbourIndices( int cellIndex );

	/// <summary>Gets distinct directed sides of the final quantized polygons; reversed endpoints are distinct edges.</summary>
	IReadOnlyList<Edge> Edges { get; }

}
