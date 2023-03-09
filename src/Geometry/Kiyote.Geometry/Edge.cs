namespace Kiyote.Geometry;

public readonly struct Edge: IEquatable<Edge> {

	public Edge(
		Point a,
		Point b
	) {
		A = a;
		B = b;
	}

	public Point A { get; }

	public Point B { get; }

	public bool TryFindIntersection(
		Edge other,
		out Point intersection
	) {
		return TryFindIntersection(
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

	public bool TryFindIntersection(
		Point a,
		Point b,
		out Point intersection
	) {
		return TryFindIntersection(
			A.X,
			A.Y,
			B.X,
			B.Y,
			a.X,
			a.Y,
			b.X,
			b.Y,
			out intersection
		);
	}

	public static bool TryFindIntersection(
		int aX1,
		int aY1,
		int bX1,
		int bY1,
		int aX2,
		int aY2,
		int bX2,
		int bY2,
		out Point intersection
	) {
		// Make sure none of the lines are zero length
		if( ( aX1 == bX1 && aY1 == bY1 )
			|| ( aX2 == bX2 && aY2 == bY2 )
		) {
			intersection = Point.None;
			return false;
		}

		double denominator = ( ( ( bY2 - aY2 ) * ( bX1 - aX1 ) )
			- ( ( bX2 - aX2 ) * ( bY1 - aY1 ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			intersection = Point.None;
			return false;
		}

		double ua = ( ( ( bX2 - aX2 ) * ( aY1 - aY2 ) )
			- ( ( bY2 - aY2 ) * ( aX1 - aX2 ) ) ) / denominator;

		double ub = ( ( ( bX1 - aX1 ) * ( aY1 - aY2 ) )
			- ( ( bY1 - aY1 ) * ( aX1 - aX2 ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			intersection = Point.None;
			return false;
		}

		double x = aX1 + ( ua * ( bX1 - aX1 ) );
		double y = aY1 + ( ua * ( bY1 - aY1 ) );

		intersection = new Point( (int)Math.Round( x ), (int)Math.Round( y ) );
		return true;
	}

	public bool Equals(
		Edge other
	) {
		return Equals( other.A, other.B );
	}

	public bool Equals(
		Point a,
		Point b
	) {
		return (A.Equals( a ) && B.Equals( b ))
			|| (A.Equals( b ) && B.Equals( a ));
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Edge edge && Equals( edge );
	}

	public override int GetHashCode() {
		return HashCode.Combine( A, B );
	}

	public static bool operator ==( Edge left, Edge right ) {
		return left.Equals( right );
	}

	public static bool operator !=( Edge left, Edge right ) {
		return !( left == right );
	}
}
