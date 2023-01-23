namespace Kiyote.Geometry.DelaunayVoronoi;

// Logic ported from https://github.com/d3/d3-delaunay

internal sealed class DelaunayFactory : IDelaunayFactory {

	Delaunay IDelaunayFactory.Create(
		Delaunator delaunator
	) {
		// Turn the raw coordinates in to points
		int n = delaunator.Coords.Count / 2;
		List<Point> points = new List<Point>( n );
		for( int i = 0; i < n; i++ ) {
			points.Add(
				new Point(
					(int)delaunator.Coords[i * 2],
					(int)delaunator.Coords[( i * 2 ) + 1]
				)
			);
		}

		// Map the triangles to their associated points
		n = delaunator.Triangles.Count / 3;
		List<Triangle> triangles = new List<Triangle>( n );
		for( int i = 0; i < n; i++ ) {
			triangles.Add(
				new Triangle(
					points[delaunator.Triangles[i * 3]],
					points[delaunator.Triangles[( i * 3 ) + 1]],
					points[delaunator.Triangles[( i * 3 ) + 2]]
				)
			);
		}

		// Construct the point pairs that make up the edges of the triangles
		List<Edge> edges = new List<Edge>( delaunator.Triangles.Count );
		for( int i = 0; i < delaunator.Triangles.Count; i++ ) {
			if( i > delaunator.HalfEdges[i] ) {
				Point p = points[delaunator.Triangles[i]];
				Point q = points[delaunator.Triangles[Delaunator.NextHalfedge( i )]];
				edges.Add(
					new Edge( p, q )
				);
			}
		}

		return new Delaunay( points, edges, triangles );
	}
}
