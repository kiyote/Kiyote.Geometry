namespace Kiyote.Geometry;

public sealed class Polygon {
	public readonly static Polygon None = new Polygon( [] );

	public Polygon(
		IReadOnlyList<Point> points
	) {
		Points = points;

		List<Edge> edges = [];
		for( int i = 0; i < points.Count; i++ ) {
			int ind = ( i + 1 ) % points.Count;
			Edge edge = new Edge( points[i], points[ind] );
			edges.Add( edge );
		}
		Edges = edges;
	}

	public IReadOnlyList<Point> Points { get; }

	public IReadOnlyList<Edge> Edges { get; }

	public IReadOnlyList<Point> Intersections(
		IReadOnlyList<Point> polygon
	) {
		List<Point> intersections = [];
		for( int i = 0; i < Points.Count; i++ ) {
			Point p1 = Points[i];
			Point p2 = Points[( i + 1 ) % Points.Count];

			for( int j = 0; j < polygon.Count; j++ ) {
				Point p3 = polygon[j];
				Point p4 = polygon[( j + 1 ) % polygon.Count];

				if( Geometry.Intersect.TryFindIntersection(
					p1.X,
					p1.Y,
					p2.X,
					p2.Y,
					p3.X,
					p3.Y,
					p4.X,
					p4.Y,
					out Point? intersection
				) ) {
					intersections.Add( intersection );
				}
			}
		}

		return intersections;
	}

	public bool HasIntersection(
		Polygon polygon
	) {
		return HasIntersection( polygon.Edges );
	}

	public bool HasIntersection(
		IReadOnlyList<Edge> edges
	) {
		foreach (Edge edge in edges) {
			if (HasIntersection( edge)) {
				return true;
			}
		}

		return false;
	}

	public bool HasIntersection(
		Edge edge
	) {
		for( int i = 0; i < Points.Count; i++ ) {
			Point p1 = Points[i];
			Point p2 = Points[( i + 1 ) % Points.Count];

			if( Geometry.Intersect.HasIntersection(
				edge.A.X,
				edge.A.Y,
				edge.B.X,
				edge.B.Y,
				p1.X,
				p1.Y,
				p2.X,
				p2.Y
			) ) {
				return true;
			}
		}

		return false;
	}

	public bool TryFindIntersections(
		Edge edge,
		out IReadOnlyList<Point> intersections
	) {
		List<Point> result = [];
		for( int i = 0; i < Points.Count; i++ ) {
			Point p1 = Points[i];
			Point p2 = Points[( i + 1 ) % Points.Count];

			if( Geometry.Intersect.TryFindIntersection(
				edge.A.X,
				edge.A.Y,
				edge.B.X,
				edge.B.Y,
				p1.X,
				p1.Y,
				p2.X,
				p2.Y,
				out Point? intersection
			) ) {
				result.Add( intersection );
			}
		}
		intersections = result;
		return intersections.Count != 0;
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

	public bool Intersect(
		Polygon polygon,
		out Polygon intersectedPolygon
	) {
		// https://gorillasun.de/blog/an-algorithm-for-polygon-intersections
		IReadOnlyList<Point> intersections = Intersections( polygon.Points );
		if( !intersections.Any() ) {
			intersectedPolygon = None;
			return false;
		}
		IEnumerable<Point> otherPointsWithinThis = polygon.Points.Where( Contains );
		IEnumerable<Point> thisPointsWithinOther = Points.Where( polygon.Contains );

		// This may need a .Distinct() just beforehand?
		List<Point> allPoints = [.. intersections.Union( otherPointsWithinThis ).Union( thisPointsWithinOther )];

		int centerX = allPoints[0].X;
		int centerY = allPoints[0].Y;
		for( int i = 1; i < allPoints.Count; i++ ) {
			centerX += allPoints[i].X;
			centerY += allPoints[i].Y;
		}
		centerX /= allPoints.Count;
		centerY /= allPoints.Count;

		var sortedPoints = allPoints.OrderBy( p => Math.Atan2( centerY - p.Y, centerX - p.X ) ).ToList();
		intersectedPolygon = new Polygon( sortedPoints );
		return true;
	}

	public bool IsEquivalentTo(
		Polygon other
	) {
		return
			Points.Count != other.Points.Count
			&& Points.OrderBy( p => p ).SequenceEqual( other.Points.OrderBy( p => p ) );
	}
}
