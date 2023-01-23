namespace Kiyote.Geometry;

/// <summary>
/// Provides an interface for a integer-based coordinate point
/// </summary>
public sealed record Point(
	int X,
	int Y
) : IPoint {

	bool IEquatable<IPoint>.Equals(
		IPoint? other
	) {
		if( other is null ) {
			return false;
		}

		return Equals( other );
	}
}
