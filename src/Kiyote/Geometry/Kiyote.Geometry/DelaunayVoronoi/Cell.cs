namespace Kiyote.Geometry.DelaunayVoronoi;

/// <summary>Immutable cell metadata. The containing diagram owns its site, vertices and neighbours.</summary>
public readonly record struct Cell {

	internal Cell(
		int vertexOffset,
		int vertexCount,
		bool isOpen,
		Rect boundingBox
	) {
		VertexOffset = vertexOffset;
		VertexCount = vertexCount;
		IsOpen = isOpen;
		BoundingBox = boundingBox;
	}

	internal int VertexOffset { get; }

	public int VertexCount { get; }
	/// <summary>Whether projection/clipping marked this cell as open at the diagram boundary.</summary>
	public bool IsOpen { get; }

	public Rect BoundingBox { get; }
}
