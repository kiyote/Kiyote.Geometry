namespace Kiyote.Geometry;

public readonly struct Point : IEquatable<Point> {
	public readonly static Point None = new Point( int.MinValue, int.MinValue );

	public Point(
		int x,
		int y
	) {
		X = x;
		Y = y;
	}

	public Point()
		: this( int.MinValue, int.MinValue ) { }

	public int X { get; }
	public int Y { get; }

	public bool Equals(
		Point other
	) {
		return ( other.X == X && other.Y == Y );
	}

	public override bool Equals( object? obj ) {
		return obj is Point point && Equals( point );
	}

	public override int GetHashCode() {
		return (X << 16) ^ Y;
	}

	public static bool operator ==( Point left, Point right ) {
		return left.Equals( right );
	}

	public static bool operator !=( Point left, Point right ) {
		return !( left == right );
	}
}
