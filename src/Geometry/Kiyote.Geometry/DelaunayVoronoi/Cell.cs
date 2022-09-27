namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed record Cell(
	IPoint Center,
	IReadOnlyList<Point> Points,
	bool IsOpen,
	IRect BoundingBox
);
