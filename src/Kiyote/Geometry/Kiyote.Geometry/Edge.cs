using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry;

public sealed class Edge : IEquatable<Edge> {

	public static readonly Edge None = new Edge( Point.None, Point.None );

	private readonly int _hashCode;

	public Edge(
		Point a,
		Point b
	) {
		A = a;
		B = b;

		int x1 = Math.Min( A.X, B.X );
		int x2 = Math.Max( A.X, B.X );
		int y1 = Math.Min( A.Y, B.Y );
		int y2 = Math.Max( A.Y, B.Y );
		_hashCode = HashCode.Combine( x1, y1, x2, y2 );
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

	public override int GetHashCode() {
		return _hashCode;
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Edge e
			&& EdgeEquals( e );
	}

	bool IEquatable<Edge>.Equals(
		Edge? other
	) {
		return other is not null
			&& EdgeEquals( other );
	}

	private bool EdgeEquals(
		Edge other
	) {
		return ( ( other.A.Equals( A ) && other.B.Equals( B ) )
			|| ( other.A.Equals( B ) && other.B.Equals( A ) ) );
	}
}
