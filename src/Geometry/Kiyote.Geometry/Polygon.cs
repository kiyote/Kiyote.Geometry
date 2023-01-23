namespace Kiyote.Geometry;

internal sealed record Polygon( IReadOnlyList<IPoint> Points ) : IPolygon {

	IReadOnlyList<IPoint> IPolygon.Intersections(
		IReadOnlyList<IPoint> polygon
	) {
		throw new InvalidOperationException();
	}

	bool IPolygon.Contains(
		IPoint target
	) {
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		for( int i = 0; i < Points.Count; i++ ) {
			IPoint point = Points[i];
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
		if( target.X < minX || target.X > maxX || target.Y < minY || target.Y > maxY ) {
			return false;
		}

		// https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
		bool inside = false;
		for( int i = 0, j = Points.Count - 1; i < Points.Count; j = i++ ) {
			if(
				( Points[i].Y > target.Y ) != ( Points[j].Y > target.Y )
				&& target.X < ( ( Points[j].X - Points[i].X ) * ( target.Y - Points[i].Y ) / ( Points[j].Y - Points[i].Y ) ) + Points[i].X
			) {
				inside = !inside;
			}
		}

		return inside;

	}
}
