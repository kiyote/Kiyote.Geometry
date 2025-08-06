namespace Kiyote.Geometry;

public sealed record Triangle {

	public Triangle(
		Point p1,
		Point p2,
		Point p3
	) {
		P1 = p1;
		P2 = p2;
		P3 = p3;
	}

	public Point P1 { get; }

	public Point P2 { get; }

	public Point P3 { get; }
}
