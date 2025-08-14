namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed class Cell {

	public Cell(
		Point center,
		Polygon polygon,
		bool isOpen,
		Rect boundingBox
	) {
		Center = center;
		Polygon = polygon;
		IsOpen = isOpen;
		BoundingBox = boundingBox;
	}

	public Point Center { get; }

	public Polygon Polygon { get; }

	public bool IsOpen { get; }

	public Rect BoundingBox { get; }
}
