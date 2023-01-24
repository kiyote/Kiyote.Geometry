namespace Kiyote.Geometry;

public sealed record Triangle(
	Point P1,
	Point P2,
	Point P3
) : ITriangle;
