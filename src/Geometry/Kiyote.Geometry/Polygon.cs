namespace Kiyote.Geometry;

internal sealed record Polygon(
	IReadOnlyList<IPoint> Points
) : IPolygon {

	IReadOnlyList<IPoint> IPolygon.Intersections(
		IReadOnlyList<IPoint> polygon
	) {
		List<IPoint> intersections = new List<IPoint>();
		for( int i = 0; i < Points.Count; i++ ) {
			IPoint p1 = Points[i];
			IPoint p2 = Points[( i + 1 ) % Points.Count];

			for( int j = 0; j < polygon.Count; j++ ) {
				IPoint p3 = polygon[j];
				IPoint p4 = polygon[( j + 1 ) % polygon.Count];

				IPoint? intersection = Edge.Intersect(
					p1.X,
					p1.Y,
					p2.X,
					p2.Y,
					p3.X,
					p3.Y,
					p4.X,
					p4.Y
				);
				if( intersection is not null ) {
					intersections.Add( intersection );
				}
			}
		}

		return intersections;
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
