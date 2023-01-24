namespace Kiyote.Geometry;

/*
/// <summary>
/// Provides an interface for a integer-based coordinate point
/// </summary>
public sealed record Point(
	int X,
	int Y
) : IPoint {
	public readonly static IPoint None = new Point( int.MinValue, int.MinValue );

	public Point() :this(None.X, None.Y) { }

	IPoint IPoint.None => None;

	int IPoint.X => X;

	int IPoint.Y => Y;

	bool IEquatable<IPoint>.Equals(
		IPoint? other
	) {
		if( other is null ) {
			return false;
		}

		return X == other.X
			&& Y == other.Y;
	}
}
*/

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

	public readonly int X;
	public readonly int Y;

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
