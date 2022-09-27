namespace Kiyote.Geometry;

public sealed class Triangle : ITriangle {

	public Triangle(
		IPoint p1,
		IPoint p2,
		IPoint p3
	) {
		P1 = p1;
		P2 = p2;
		P3 = p3;
	}

	public IPoint P1 { get; }

	public IPoint P2 { get; }

	public IPoint P3 { get; }
}
