using System.Runtime.CompilerServices;

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
		foreach( Edge edge in edges ) {
			if( HasIntersection( edge ) ) {
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
		// Refined from https://web.archive.org/web/20130126163405/http://geomalgorithms.com/a03-_inclusion.html
		int pc = Points.Count;
		int wn = 0;
		for( int i = 0; i < pc; i++ ) {
			int j = ( i + 1 ) % pc;

			int ix = Points[i].X;
			int iy = Points[i].Y;
			int jx = Points[j].X;
			int jy = Points[j].Y;

			int isLeft = IsLeft( ix, iy, jx, jy, x, y );

			if( IsOnEdge( isLeft, ix, iy, jx, jy, x, y ) ) {
				return true;
			}

			if( iy <= y ) {
				if( jy > y ) {
					if( isLeft > 0 ) {
						wn++;
					}
				}
			} else {
				if( jy <= y ) {
					if( isLeft < 0 ) {
						wn--;
					}
				}
			}
		}

		return wn != 0;

	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static int IsLeft(
		int p0x,
		int p0y,
		int p1x,
		int p1y,
		int p2x,
		int p2y
	) {
		return ( ( ( p1x - p0x ) * ( p2y - p0y ) ) - ( ( p2x - p0x ) * ( p1y - p0y ) ) );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static bool IsOnEdge(
		int isLeft,
		int p0x,
		int p0y,
		int p1x,
		int p1y,
		int p2x,
		int p2y
	) {
		// Check for collinearity (cross-product is zero)
		if( isLeft != 0 ) {
			return false;
		}

		// Check if the point is within the bounding box of the edge
		return
			p2x >= Math.Min( p0x, p1x )
			&& p2x <= Math.Max( p0x, p1x )
			&& p2y >= Math.Min( p0y, p1y )
			&& p2y <= Math.Max( p0y, p1y );
	}

	public bool Contains(
		Polygon polygon
	) {
		return Contains( polygon.Points );
	}

	public bool Contains(
		IEnumerable<Point> points
	) {
		return points.All( Contains );
	}

	public bool HasOverlap(
		Polygon polygon
	) {
		return
			this.Contains( polygon )
			|| polygon.Contains( this )
			|| this.HasIntersection( polygon );
	}

	public bool TryIntersect(
		Polygon polygon,
		out Polygon intersectedPolygon
	) {
		// https://gorillasun.de/blog/an-algorithm-for-polygon-intersections
		IReadOnlyList<Point> intersections = Intersections( polygon.Points );
		if( intersections.Count == 0 ) {
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
		if (Points.Count != other.Points.Count) {
			return false;
		}

		IEnumerable<Point> one = Points.OrderBy( p => p.X ).ThenBy( p => p.Y );
		IEnumerable<Point> two = other.Points.OrderBy( p => p.X ).ThenBy( p => p.Y );

		return one.SequenceEqual( two );
	}
}
