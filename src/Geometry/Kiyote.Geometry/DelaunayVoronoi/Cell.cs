namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed record Cell(
	Point Center,
	IPolygon Polygon,
	bool IsOpen,
	IRect BoundingBox
);
