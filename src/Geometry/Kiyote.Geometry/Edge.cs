namespace Kiyote.Geometry;

public sealed class Edge : IEdge {

	public Edge(
		IPoint a,
		IPoint b
	) {
		A = a;
		B = b;
	}

	public Edge(
		int aX,
		int aY,
		int bX,
		int bY
	) {
		A = new Point( aX, aY );
		B = new Point( bX, bY );
	}

	public IPoint A { get; }

	public IPoint B { get; }

	public IPoint? Intersect(
		IPoint a,
		IPoint b
	) {
		// Make sure none of the lines are zero length
		if( ( A.X == B.X && A.Y == B.Y )
			|| ( a.X == b.X && a.Y == b.Y )
		) {
			return null;
		}

		double denominator = ( ( ( b.Y - a.Y ) * ( B.X - A.X ) )
			- ( ( b.X - a.X ) * ( B.Y - A.Y ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			return null;
		}

		double ua = ( ( ( b.X - a.X ) * ( A.Y - a.Y ) )
			- ( ( b.Y - a.Y ) * ( A.X - a.X ) ) ) / denominator;

		double ub = ( ( ( B.X - A.X ) * ( A.Y - a.Y ) )
			- ( ( B.Y - A.Y ) * ( A.X - a.X ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			return null;
		}

		double x = A.X + ( ua * ( B.X - A.X ) );
		double y = A.Y + ( ua * ( B.Y - A.Y ) );

		return new Point( (int)Math.Round( x ), (int)Math.Round( y ) );
	}

	public IPoint? Intersect(
		IEdge other
	) {
		return Intersect( other.A, other.B );
	}

	public bool Equals(
		IPoint a,
		IPoint b
	) {
		return ( A.Equals( a ) && B.Equals( b ) )
			|| ( A.Equals( b ) && B.Equals( a ) );
	}

	public bool Equals(
		IEdge? other
	) {
		if( other is null ) {
			return false;
		}

		return Equals( other.A, other.B );
	}

	public override bool Equals(
		object? obj
	) {
		return Equals( obj as Edge );
	}

	public override int GetHashCode() {
		return HashCode.Combine( A, B );
	}
}
