namespace Kiyote.Geometry;

public sealed record Polygon(
	IReadOnlyList<Point> Points
) {
	private readonly static Point NoPoint = new Point( int.MinValue, int.MinValue );
	public readonly static Polygon None = new Polygon( Array.Empty<Point>() );

	public IReadOnlyList<Point> Intersections(
		IReadOnlyList<Point> polygon
	) {
		List<Point> intersections = new List<Point>();
		for( int i = 0; i < Points.Count; i++ ) {
			Point p1 = Points[i];
			Point p2 = Points[( i + 1 ) % Points.Count];

			for( int j = 0; j < polygon.Count; j++ ) {
				Point p3 = polygon[j];
				Point p4 = polygon[( j + 1 ) % polygon.Count];

				if( TryFindIntersection(
					p1.X,
					p1.Y,
					p2.X,
					p2.Y,
					p3.X,
					p3.Y,
					p4.X,
					p4.Y,
					out Point intersection
				) ) {
					intersections.Add( intersection );
				}
			}
		}

		return intersections;
	}

	public bool Contains(
		Point target
	) {
		return Contains( target.X, target.Y );
	}

	public bool Contains(
		int x,
		int y
	) {
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		for( int i = 0; i < Points.Count; i++ ) {
			Point point = Points[i];
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
		if( x < minX || x > maxX || y < minY || y > maxY ) {
			return false;
		}

		// https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
		bool inside = false;
		for( int i = 0, j = Points.Count - 1; i < Points.Count; j = i++ ) {
			if(
				( Points[i].Y > y ) != ( Points[j].Y > y )
				&& x < ( ( Points[j].X - Points[i].X ) * ( y - Points[i].Y ) / ( Points[j].Y - Points[i].Y ) ) + Points[i].X
			) {
				inside = !inside;
			}
		}

		return inside;
	}

	public bool TryFindIntersection(
		Polygon polygon,
		out Polygon clipped
	) {
		// https://gorillasun.de/blog/an-algorithm-for-polygon-intersections
		IReadOnlyList<Point> intersections = Intersections( polygon.Points );
		if( !intersections.Any() ) {
			clipped = None;
			return false;
		}
		IEnumerable<Point> otherPointsWithinThis = polygon.Points.Where( p => Contains( p ) );
		IEnumerable<Point> thisPointsWithinOther = Points.Where( p => polygon.Contains( p ) );

		// This may need a .Distinct() just before .ToList()?
		List<Point> allPoints = intersections.Union( otherPointsWithinThis ).Union( thisPointsWithinOther ).ToList();

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

	private static bool TryFindIntersection(
		int aX1,
		int aY1,
		int bX1,
		int bY1,
		int aX2,
		int aY2,
		int bX2,
		int bY2,
		out Point intersection
	) {
		// Make sure none of the lines are zero length
		if( ( aX1 == bX1 && aY1 == bY1 )
			|| ( aX2 == bX2 && aY2 == bY2 )
		) {
			intersection = NoPoint;
			return false;
		}

		double denominator = ( ( ( bY2 - aY2 ) * ( bX1 - aX1 ) )
			- ( ( bX2 - aX2 ) * ( bY1 - aY1 ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			intersection = NoPoint;
			return false;
		}

		double ua = ( ( ( bX2 - aX2 ) * ( aY1 - aY2 ) )
			- ( ( bY2 - aY2 ) * ( aX1 - aX2 ) ) ) / denominator;

		double ub = ( ( ( bX1 - aX1 ) * ( aY1 - aY2 ) )
			- ( ( bY1 - aY1 ) * ( aX1 - aX2 ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			intersection = NoPoint;
			return false;
		}

		double x = aX1 + ( ua * ( bX1 - aX1 ) );
		double y = aY1 + ( ua * ( bY1 - aY1 ) );

		intersection = new Point( (int)Math.Round( x ), (int)Math.Round( y ) );
		return true;
	}
}
