namespace Kiyote.Geometry;

public sealed class Point : IEquatable<Point>, ISize {

	public static readonly Point None = new Point( int.MinValue, int.MaxValue );

	private readonly int _hashCode;

	public Point(
		int x,
		int y
	) {
		X = x;
		Y = y;
		_hashCode = ( X << 16 ) ^ Y;
	}

	public int X { get; }
	public int Y { get; }

	int ISize.Width => X;

	int ISize.Height => Y;

	public Point Subtract( Point other ) {
		return Subtract( other.X, other.Y );
	}

	public Point Subtract(
		int x,
		int y
	) {
		return new Point( X - x, Y - y );
	}

	public Point Add( Point other ) {
		return Add( other.X, other.Y );
	}

	public Point Add(
		int x,
		int y
	) {
		return new Point( X + x, Y + y );
	}

	public override int GetHashCode() {
		return _hashCode;
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Point p
			&& p.X == X
			&& p.Y == Y;		
	}

	bool IEquatable<ISize>.Equals(
		ISize? other
	) {
		return other is not null
			&& other.Width == X
			&& other.Height == Y;
	}

	bool IEquatable<Point>.Equals(
		Point? other
	) {
		return other is not null
			&& other.X == X
			&& other.Y == Y;
	}
}
