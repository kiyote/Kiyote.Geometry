namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed record Cell(
	Point Center,
	Polygon Polygon,
	bool IsOpen,
	Rect BoundingBox
);
