namespace Kiyote.Geometry;

internal sealed record Polygon(
	IReadOnlyList<IPoint> Points
) : IPolygon {

	public static readonly Polygon None = new Polygon( Array.Empty<Point>() );

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

				if (Edge.TryFindIntersection(
					p1.X,
					p1.Y,
					p2.X,
					p2.Y,
					p3.X,
					p3.Y,
					p4.X,
					p4.Y,
					out IPoint intersection
				) ) {
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

	bool IPolygon.TryFindIntersection(
		IPolygon polygon,
		out IPolygon clipped
	) {
		// https://gorillasun.de/blog/an-algorithm-for-polygon-intersections
		IReadOnlyList<IPoint> intersections = ( this as IPolygon ).Intersections( polygon.Points );
		if (!intersections.Any()) {
			clipped = None;
			return false;
		}
		IEnumerable<IPoint> otherPointsWithinThis = polygon.Points.Where( p => ( this as IPolygon ).Contains( p ) );
		IEnumerable<IPoint> thisPointsWithinOther = Points.Where( p => polygon.Contains( p ) );

		// This may need a .Distinct() just before .ToList()?
		List<IPoint> allPoints = intersections.Union( otherPointsWithinThis ).Union( thisPointsWithinOther ).ToList();

		int centerX = allPoints[0].X;
		int centerY = allPoints[0].Y;
		for( int i = 1; i < allPoints.Count; i++ ) {
			centerX += allPoints[i].X;
			centerY += allPoints[i].Y;
		}
		centerX /= allPoints.Count;
		centerY /= allPoints.Count;

		var sortedPoints = allPoints.OrderBy( p => Math.Atan2( centerY - p.Y, centerX - p.X ) ).ToList();
		clipped = new Polygon( sortedPoints );
		return true;
	}
}
