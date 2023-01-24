namespace Kiyote.Geometry;

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
