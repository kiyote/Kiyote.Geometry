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
			minX,
			minY,
			maxX - minX + 1,
			maxY - minY + 1
		);
	}

	public bool IsEquivalentTo(
		Edge other
	) {
		return ( ( other.A.Equals( A ) && other.B.Equals( B ) )
			|| ( other.A.Equals( B ) && other.B.Equals( A ) ) );
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
		return !( left  ==  right );
	}
}
