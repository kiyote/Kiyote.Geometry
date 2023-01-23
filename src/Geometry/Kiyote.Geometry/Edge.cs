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

	public IPoint? Intersection(
		IEdge other
	) {
		return Intersect(
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

	public IPoint? Intersection(
		IPoint a,
		IPoint b
	) {
		return Intersect(
			A.X,
			A.Y,
			B.X,
			B.Y,
			a.X,
			a.Y,
			b.X,
			b.Y
		);
	}

	public static IPoint? Intersect(
		int aX1,
		int aY1,
		int bX1,
		int bY1,
		int aX2,
		int aY2,
		int bX2,
		int bY2
	) {
		// Make sure none of the lines are zero length
		if( ( aX1 == bX1 && aY1 == bY1 )
			|| ( aX2 == bX2 && aY2 == bY2 )
		) {
			return null;
		}

		double denominator = ( ( ( bY2 - aY2 ) * ( bX1 - aX1 ) )
			- ( ( bX2 - aX2 ) * ( bY1 - aY1 ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			return null;
		}

		double ua = ( ( ( bX2 - aX2 ) * ( aY1 - aY2 ) )
			- ( ( bY2 - aY2 ) * ( aX1 - aX2 ) ) ) / denominator;

		double ub = ( ( ( bX1 - aX1 ) * ( aY1 - aY2 ) )
			- ( ( bY1 - aY1 ) * ( aX1 - aX2 ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			return null;
		}

		double x = aX1 + ( ua * ( bX1 - aX1 ) );
		double y = aY1 + ( ua * ( bY1 - aY1 ) );

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
