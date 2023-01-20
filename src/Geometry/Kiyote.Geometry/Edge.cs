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
		int x1,
		int y1,
		int x2,
		int y2
	) {
		A = new Point( x1, y1 );
		B = new Point( x2, y2 );
	}

	public IPoint A { get; }

	public IPoint B { get; }

	public IPoint? Intersect(
		IEdge other
	) {
		// Make sure none of the lines are zero length
		if( ( A.X == B.X && A.Y == B.Y )
			|| ( other.A.X == other.B.X && other.A.Y == other.B.Y )
		) {
			return null;
		}

		double denominator = ( ( ( other.B.Y - other.A.Y ) * ( B.X - A.X ) )
			- ( ( other.B.X - other.A.X ) * ( B.Y - A.Y ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			return null;
		}

		double ua = ( ( ( other.B.X - other.A.X ) * ( A.Y - other.A.Y ) )
			- ( ( other.B.Y - other.A.Y ) * ( A.X - other.A.X ) ) ) / denominator;

		double ub = ( ( ( B.X - A.X ) * ( A.Y - other.A.Y ) )
			- ( ( B.Y - A.Y ) * ( A.X - other.A.X ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			return null;
		}

		double x = A.X + ( ua * ( B.X - A.X ) );
		double y = A.Y + ( ua * ( B.Y - A.Y ) );

		return new Point( (int)Math.Round( x ), (int)Math.Round( y ) );
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
