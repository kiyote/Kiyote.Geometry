using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Kiyote.Geometry;

public sealed class Polygon {
	public readonly static Polygon None = new Polygon( [] );

	public Polygon(
		IReadOnlyList<Point> points
	) {
		Points = points;

		Edges = CreateEdges( points );
	}

	public IReadOnlyList<Point> Points { get; }

	public ImmutableArray<Edge> Edges { get; }

	public bool HasIntersection(
		Polygon polygon
	) {
		return HasIntersection( polygon.Edges );
	}

	public bool HasIntersection(
		ImmutableArray<Edge> edges
	) {
		for( int i = 0; i < edges.Length; i++ ) {
			if( HasIntersection( edges[i] ) ) {
				return true;
			}
		}
		return false;
	}

	public bool HasIntersection(
		Edge edge
	) {
		int pointCount = Points.Count;
		for( int i = 0; i < pointCount; i++ ) {
			int j = ( i + 1 ) % pointCount;
			Point p1 = Points[i];
			Point p2 = Points[j];

			if( Intersect.HasIntersection(
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

		List<Point>? result = null;
		for( int i = 0; i < Edges.Length; i++ ) {
			if( Intersect.TryFindIntersection(
				edge.A.X,
				edge.A.Y,
				edge.B.X,
				edge.B.Y,
				Edges[i].A.X,
				Edges[i].A.Y,
				Edges[i].B.X,
				Edges[i].B.Y,
				out int intersectionX,
				out int intersectionY
			) ) {
				result ??= [];
				result.Add( new Point( intersectionX, intersectionY ) );
			}
		}

		if( result is null ) {
			intersections = [];
			return false;
		}
		intersections = result;
		return true;
	}

	public bool TryFindIntersections(
		Polygon polygon,
		out IReadOnlyList<Point> intersections
	) {
		List<Point>? result = null;
		for( int i = 0; i < Edges.Length; i++ ) {
			for( int j = 0; j < polygon.Edges.Length; j++ ) {
				if( Intersect.TryFindIntersection(
					polygon.Edges[j].A.X,
					polygon.Edges[j].A.Y,
					polygon.Edges[j].B.X,
					polygon.Edges[j].B.Y,
					Edges[i].A.X,
					Edges[i].A.Y,
					Edges[i].B.X,
					Edges[i].B.Y,
					out int intersectionX,
					out int intersectionY
				) ) {
					result ??= [];
					result.Add( new Point( intersectionX, intersectionY ) );
				}
			}
		}

		if( result is null ) {
			intersections = [];
			return false;
		}
		intersections = result;
		return true;
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

	public bool Contains(
		Polygon polygon
	) {
		for( int i = 0; i < polygon.Points.Count; i++ ) {
			if( Contains( polygon.Points[i] ) ) {
				return true;
			}
		}
		return false;
	}

	public bool Contains(
		IEnumerable<Point> points
	) {
		return points.All( Contains );
	}

	public bool HasOverlap(
		Polygon polygon
	) {
		if( this.Contains( polygon ) ) {
			return true;
		}

		if( polygon.Contains( this ) ) {
			return true;
		}

		return HasIntersection( polygon );
	}

	public bool TryIntersect(
		Polygon polygon,
		out Polygon intersectedPolygon
	) {
		// https://gorillasun.de/blog/an-algorithm-for-polygon-intersections
		if( !TryFindIntersections( polygon, out IReadOnlyList<Point> intersections ) ) {
			intersectedPolygon = None;
			return false;
		}

		List<Point> allPoints = [];
		allPoints.AddRange( intersections );
		for( int i = 0; i < polygon.Points.Count; i++ ) {
			if( Contains( polygon.Points[i] ) ) {
				allPoints.Add( polygon.Points[i] );
			}
		}
		for( int i = 0; i < Points.Count; i++ ) {
			if( polygon.Contains( Points[i] ) ) {
				allPoints.Add( Points[i] );
			}
		}

		int centerX = allPoints.ElementAt( 0 ).X;
		int centerY = allPoints.ElementAt( 0 ).Y;
		for( int i = 1; i < allPoints.Count; i++ ) {
			centerX += allPoints.ElementAt( i ).X;
			centerY += allPoints.ElementAt( i ).Y;
		}
		centerX /= allPoints.Count;
		centerY /= allPoints.Count;

		List<Point> sortedPoints = [.. allPoints.OrderBy( p => Math.Atan2( centerY - p.Y, centerX - p.X ) )];
		intersectedPolygon = new Polygon( sortedPoints );
		return true;
	}

	public bool IsEquivalentTo(
		Polygon other
	) {
		if( Points.Count != other.Points.Count ) {
			return false;
		}

		// It's not great that this is N^2 but for our small polygons
		// this works well enough
		for( int i = 0; i < Points.Count; i++ ) {
			int iv = ( Points[i].X << 32 ) | Points[i].Y;

			bool found = false;
			for( int j = 0; j < other.Points.Count; j++ ) {
				int jv = ( other.Points[j].X << 32 ) | other.Points[j].Y;
				if( iv == jv ) {
					found = true;
					break;
				}
			}

			if( !found ) {
				return false;
			}
		}
		return true;
	}

	private static ImmutableArray<Edge> CreateEdges(
		IReadOnlyList<Point> points
	) {
		int pc = points.Count;
		Edge[] edges = new Edge[pc];
		for( int i = 0; i < pc; i++ ) {
			int ind = ( i + 1 ) % pc;
			Edge edge = new Edge( points[i], points[ind] );
			edges[i] = edge;
		}
		return [.. edges];
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
}
