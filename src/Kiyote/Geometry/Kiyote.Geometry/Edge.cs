using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry;

public sealed class Edge {

	public static readonly Edge None = new Edge( Point.None, Point.None );

	public Edge(
		Point a,
		Point b
	) {
		A = a;
		B = b;
	}

	public Point A { get; }

	public Point B { get; }

	public bool HasIntersection(
		Edge other
	) {
		return Intersect.HasIntersection(
			A.X,
			A.Y,
			B.X,
			B.Y,
			other.A.X,
			other.A.Y,
			other.B.X,
			other.B.Y
		);
	}

	public bool TryFindIntersection(
		Edge other,
		out Point intersection
	) {

		bool result = Intersect.TryFindIntersection(
			A.X,
			A.Y,
			B.X,
			B.Y,
			other.A.X,
			other.A.Y,
			other.B.X,
			other.B.Y,
			out int intersectionX,
			out int intersectionY
		);

		if (result) {
			intersection = new Point( intersectionX, intersectionY );
			return true;
		}
		intersection = Point.None;
		return false;
	}

	public Rect GetBoundingBox() {
		int minX = Math.Min( A.X, B.X );
		int minY = Math.Min( A.Y, B.Y );
		int maxX = Math.Max( A.X, B.X );
		int maxY = Math.Max( A.Y, B.Y );
		return new Rect(
			new Point( minX, minY ),
			new Point( maxX, maxY )
		);
	}

	public bool IsEquivalentTo(
		Edge other
	) {
		return ( ( other.A.Equals( A ) && other.B.Equals( B ) )
			|| ( other.A.Equals( B ) && other.B.Equals( A ) ) );
	}
}
