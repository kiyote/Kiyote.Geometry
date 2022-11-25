namespace Kiyote.Geometry;

public sealed record Triangle(
	IPoint P1,
	IPoint P2,
	IPoint P3
) : ITriangle;
