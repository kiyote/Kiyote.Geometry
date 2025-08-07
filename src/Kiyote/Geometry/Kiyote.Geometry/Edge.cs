using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Kiyote.Geometry;

public sealed record Edge {

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
		[NotNullWhen(true)]
		out Point? intersection
	) {

		return Intersect.TryFindIntersection(
			A.X,
			A.Y,
			B.X,
			B.Y,
			other.A.X,
			other.A.Y,
			other.B.X,
			other.B.Y,
			out intersection
		);
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
}
