namespace Kiyote.Geometry;

public readonly struct Point : IEquatable<Point>, ISize {

	public static readonly Point None = new Point( int.MinValue, int.MaxValue );

	public Point(
		int x,
		int y
	) {
		X = x;
		Y = y;
	}

	public int X { get; }

	public int Y { get; }

	readonly int ISize.Width => X;

	readonly int ISize.Height => Y;

	public readonly Point Subtract(
		Point other
	) {
		return Subtract( other.X, other.Y );
	}

	public readonly Point Subtract(
		int x,
		int y
	) {
		return new Point( X - x, Y - y );
	}

	public readonly Point Add(
		Point other
	) {
		return Add( other.X, other.Y );
	}

	public readonly Point Add(
		int x,
		int y
	) {
		return new Point( X + x, Y + y );
	}

	public readonly void Deconstruct(
		out int x,
		out int y
	) {
		x = X;
		y = Y;
	}

	public readonly Point Negate() {
		return new Point( -X, -Y );
	}

	/// <summary>
	/// Returns a point made of the smaller of each coordinate.
	/// </summary>
	public static Point Min(
		Point a,
		Point b
	) {
		return new Point( Math.Min( a.X, b.X ), Math.Min( a.Y, b.Y ) );
	}

	/// <summary>
	/// Returns a point made of the larger of each coordinate.
	/// </summary>
	public static Point Max(
		Point a,
		Point b
	) {
		return new Point( Math.Max( a.X, b.X ), Math.Max( a.Y, b.Y ) );
	}

	/// <summary>
	/// The number of orthogonal steps between the two points.
	/// </summary>
	public readonly int ManhattanDistance(
		Point other
	) {
		return Math.Abs( X - other.X ) + Math.Abs( Y - other.Y );
	}

	/// <summary>
	/// The number of steps between the two points when diagonal steps are allowed.
	/// </summary>
	public readonly int ChebyshevDistance(
		Point other
	) {
		return Math.Max( Math.Abs( X - other.X ), Math.Abs( Y - other.Y ) );
	}

	/// <summary>
	/// The squared Euclidean distance between the two points.
	/// </summary>
	public readonly long DistanceSquared(
		Point other
	) {
		long dx = (long)X - other.X;
		long dy = (long)Y - other.Y;
		return ( dx * dx ) + ( dy * dy );
	}

	/// <summary>
	/// Returns true when <paramref name="other"/> is a direct neighbour of this
	/// point.  A point is not adjacent to itself.
	/// </summary>
	public readonly bool IsAdjacentTo(
		Point other,
		bool includeDiagonals
	) {
		return includeDiagonals
			? ChebyshevDistance( other ) == 1
			: ManhattanDistance( other ) == 1;
	}

	/// <summary>
	/// Arithmetically shifts both coordinates right.  Unlike division this
	/// rounds towards negative infinity, so it maps cells to chunk coordinates
	/// correctly for negative positions.
	/// </summary>
	public readonly Point ShiftRight(
		int shift
	) {
		return new Point( X >> shift, Y >> shift );
	}

	/// <summary>
	/// Applies a bitwise AND to both coordinates.  With a mask of
	/// <c>chunkSize - 1</c> this yields the position within a chunk, including
	/// for negative positions.
	/// </summary>
	public readonly Point And(
		int mask
	) {
		return new Point( X & mask, Y & mask );
	}

	public static Point operator +( Point left, Point right ) => left.Add( right );

	public static Point operator -( Point left, Point right ) => left.Subtract( right );

	public static Point operator -( Point value ) => value.Negate();

	public override readonly int GetHashCode() {
		return HashCode.Combine( X, Y );
	}

	public override readonly bool Equals(
		object? obj
	) {
		return obj is Point p
			&& p.X == X
			&& p.Y == Y;
	}

	public static bool operator ==( Point left, Point right ) => left.X == right.X && left.Y == right.Y;

	public static bool operator !=( Point left, Point right ) => !( left == right );

	readonly bool IEquatable<Point>.Equals(
		Point other
	) {
		return other.X == X
			&& other.Y == Y;
	}

	public override string ToString() {
		return $"( {X}, {Y} )";
	}
}
