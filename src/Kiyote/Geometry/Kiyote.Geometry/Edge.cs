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

	/// <summary>
	/// Returns whether bounded segments share any point, including endpoints and
	/// collinear overlap. A zero-length segment is a point.
	/// </summary>
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

	/// <summary>
	/// Returns a unique intersection, rounded to nearest with exact halves to even.
	/// Collinear shared lengths return false and <see cref="Point.None"/>.
	/// Coordinates must satisfy <see cref="GeometryLimits"/>.
	/// </summary>
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

	/// <summary>
	/// Returns inclusive bounds across the shared coordinate domain.
	/// Throws <see cref="OverflowException"/> if a dimension is unrepresentable.
	/// </summary>
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
			checked( (int)( (long)maxX - minX + 1 ) ),
			checked( (int)( (long)maxY - minY + 1 ) )
		);

	}

	public Rect GetBoundingBox() {
		return GetBoundingBox( A, B );
	}

	/// <summary>
	/// Interpolates from <paramref name="p1"/> to <paramref name="p2"/>, rounding
	/// each coordinate to nearest, with exact halves to even. Positions must satisfy
	/// <see cref="GeometryLimits"/>; amount must be finite and in [0, 1].
	/// </summary>
	public static Point Interpolate(
		Point p1,
		Point p2,
		float amount
	) {
		GeometryLimits.Validate( p1, nameof( p1 ) );
		GeometryLimits.Validate( p2, nameof( p2 ) );
		if( !( amount >= 0f && amount <= 1f ) ) {
			throw new ArgumentOutOfRangeException( nameof( amount ), amount, "Interpolation amount must be finite and in [0, 1]." );
		}
		int x = (int)Math.Round( p1.X + ( (double)p2.X - p1.X ) * amount );
		int y = (int)Math.Round( p1.Y + ( (double)p2.Y - p1.Y ) * amount );
		return new Point( x, y );
	}

	/// <summary>
	/// Interpolates from A to B; see <see cref="Interpolate(Point, Point, float)"/>.
	/// </summary>
	public Point Interpolate(
		float amount
	) {
		return Interpolate( A, B, amount );
	}

	/// <summary>Returns the Euclidean length between bounded geometry positions.</summary>
	public static float Magnitude(
		Point p1,
		Point p2
	) {
		return (float)Math.Sqrt( p1.DistanceSquared( p2 ) );
	}

	public float Magnitude() {
		return Magnitude( A, B );
	}

	/// <summary>
	/// Returns the absolute per-axis displacement, not a unit vector.
	/// Supports shared coordinates; unrepresentable displacements throw
	/// <see cref="OverflowException"/>.
	/// </summary>
	public static Point Normalize(
		Point p1,
		Point p2
	) {
		return new Point(
			checked( (int)Math.Abs( (long)p2.X - p1.X ) ),
			checked( (int)Math.Abs( (long)p2.Y - p1.Y ) )
		);
	}

	public Point Normalize() {
		return Normalize( A, B );
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
