namespace Kiyote.Geometry;

public readonly struct Edge : IEquatable<Edge> {

	public static readonly Edge None = new Edge( Point.None, Point.None );

	public Edge(
		Point a,
		Point b
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

		if( result ) {
			intersection = new Point( intersectionX, intersectionY );
			return true;
		}
		intersection = Point.None;
		return false;
	}

	public bool IsEquivalentTo(
		Edge other
	) {
		return ( ( other.A == A && other.B == B )
			|| ( other.A == B && other.B == A ) );
	}

	public static Rect GetBoundingBox(
		Point p1,
		Point p2
	) {
		int minX = Math.Min( p1.X, p2.X );
		int minY = Math.Min( p1.Y, p2.Y );
		int maxX = Math.Max( p1.X, p2.X );
		int maxY = Math.Max( p1.Y, p2.Y );
		return new Rect(
			minX,
			minY,
			maxX - minX + 1,
			maxY - minY + 1
		);

	}

	public Rect GetBoundingBox() {
		return GetBoundingBox( A, B );
	}

	public static Point GetMidpoint(
		Point p1,
		Point p2,
		float distance
	) {
		int mx = Math.Min( p1.X, p2.X );
		int dx = Math.Max( p1.X, p2.X ) - mx;
		int my = Math.Min( p1.Y, p2.Y );
		int dy = Math.Max( p1.Y, p2.Y ) - my;
		int x = mx + (int)Math.Floor( dx * distance );
		int y = my + (int)Math.Floor( dy * distance );

		return new Point( x, y );
	}

	public Point GetMidpoint(
		float distance
	) {
		return GetMidpoint( A, B, distance );
	}

	public static float Magnitude(
		Point p1,
		Point p2
	) {
		Point norm = Normalize( p1, p2 );
		return (float)Math.Sqrt( ( norm.X * norm.X ) + ( norm.Y * norm.Y ) );
	}

	public float Magnitude() {
		return Magnitude( A, B );
	}

	public static Point Normalize(
		Point p1,
		Point p2
	) {
		Rect bb = GetBoundingBox( p1, p2 );
		return new Point( bb.X2 - bb.X1, bb.Y2 - bb.Y1 );
	}

	public Point Normalize() {
		Rect bb = GetBoundingBox( A, B );
		return new Point( bb.X2 - bb.X1, bb.Y2 - bb.Y1 );
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Edge e
			&& A.X == e.A.X
			&& A.Y == e.A.Y
			&& B.X == e.B.X
			&& B.Y == e.B.Y;
	}

	public override int GetHashCode() {
		return HashCode.Combine( A, B );
	}

	bool IEquatable<Edge>.Equals(
		Edge other
	) {
		return A.X == other.A.X
			&& A.Y == other.A.Y
			&& B.X == other.B.X
			&& B.Y == other.B.Y;
	}

	public static bool operator ==(
		Edge left,
		Edge right
	) {
		return left.A.X == right.A.X
			&& left.A.Y == right.A.Y
			&& left.B.X == right.B.X
			&& left.B.Y == right.B.Y;
	}

	public static bool operator !=(
		Edge left,
		Edge right
	) {
		return !( left == right );
	}

	public override string ToString() {
		return $"({A},{B})";
	}
}
