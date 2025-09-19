namespace Kiyote.Geometry;

public readonly struct Point : IEquatable<Point>, ISize {

	public static readonly Point None = new Point( int.MinValue, int.MaxValue );

	private readonly int _hashCode;

	public Point(
		int x,
		int y
	) {
		X = x;
		Y = y;
		_hashCode = HashCode.Combine( x, y );
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

	public override readonly int GetHashCode() {
		return _hashCode;
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
