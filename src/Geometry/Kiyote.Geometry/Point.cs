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

	bool IPoint.Inside(
		IReadOnlyList<IPoint> polygon
	) {
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		for( int i = 0; i < polygon.Count; i++ ) {
			IPoint point = polygon[i];
			if( point.X < minX ) {
				minX = point.X;
			}
			if( point.X > maxX ) {
				maxX = point.X;
			}

			if( point.Y < minY ) {
				minY = point.Y;
			}
			if( point.Y > maxY ) {
				maxY = point.Y;
			}
		}
		if( X < minX || X > maxX || Y < minY || Y > maxY ) {
			return false;
		}

		// https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
		bool inside = false;
		for( int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++ ) {
			if(
				( polygon[i].Y > Y ) != ( polygon[j].Y > Y )
				&& X < ( ( polygon[j].X - polygon[i].X ) * ( Y - polygon[i].Y ) / ( polygon[j].Y - polygon[i].Y ) ) + polygon[i].X
			) {
				inside = !inside;
			}
		}

		return inside;
	}
}
