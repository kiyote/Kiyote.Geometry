namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed record Cell(
	IPoint Center,
	IPolygon Polygon,
	bool IsOpen,
	IRect BoundingBox
);
