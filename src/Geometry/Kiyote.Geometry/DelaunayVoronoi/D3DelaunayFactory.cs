namespace Kiyote.Geometry.DelaunayVoronoi;

// Ported from: https://github.com/d3/d3-delaunay/blob/main/src/delaunay.js
/*
Copyright 2018-2021 Observable, Inc.
Copyright 2021 Mapbox

Permission to use, copy, modify, and/or distribute this software for any purpose
with or without fee is hereby granted, provided that the above copyright notice
and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
*/

internal sealed class D3DelaunayFactory : IDelaunayFactory {

	IDelaunay IDelaunayFactory.Create(
		IReadOnlyList<Point> points
	) {
		return ( this as IDelaunayFactory ).Create( points, false );
	}

	IDelaunay IDelaunayFactory.Create(
		IReadOnlyList<Point> points,
		bool sanitizePoints
	) {
		if( sanitizePoints ) {
			points = points.Distinct().ToList();
		}
		double[] coords = points.ToCoords();
		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( coords );

		List<Triangle> triangles = new List<Triangle>();
		for( int i = 0; i < delaunator.Triangles.Length; i += 3 ) {
			int t1 = delaunator.Triangles[i + 0] * 2;
			int t2 = delaunator.Triangles[i + 1] * 2;
			int t3 = delaunator.Triangles[i + 2] * 2;
			double x1 = coords[t1 + 0];
			double y1 = coords[t1 + 1];
			double x2 = coords[t2 + 0];
			double y2 = coords[t2 + 1];
			double x3 = coords[t3 + 0];
			double y3 = coords[t3 + 1];

			triangles.Add(
				new Triangle(
					new Point( (int)x1, (int)y1 ),
					new Point( (int)x2, (int)y2 ),
					new Point( (int)x3, (int)y3 )
				)
			);
		}

		List<Point> hull = new List<Point>();
		for( int i = 0; i < delaunator.Hull.Length; i++ ) {
			int c = delaunator.Hull[i];
			if( c >= 0 ) {
				double x = coords[( c * 2 ) + 0];
				double y = coords[( c * 2 ) + 1];
				hull.Add( new Point( (int)x, (int)y ) );
			}
		}

		Delaunay result = new Delaunay(
			points,
			triangles,
			hull
		);

		return result;
	}

	internal static D3Delaunay Create(
		ReadOnlySpan<double> coords,
		MapboxDelaunator delaunator
	) {
		int n = coords.Length / 2;
		int[] inedges = new int[n];
		int[] hullIndex = new int[n];

		ReadOnlySpan<int> halfEdges = delaunator.HalfEdges;
		ReadOnlySpan<int> hull = delaunator.Hull;
		ReadOnlySpan<int> triangles = delaunator.Triangles;
		Array.Fill( inedges, -1 );
		Array.Fill( hullIndex, -1 );

		// Compute an index from each point to an (arbitrary) incoming halfedge
		// Used to give the first neighbor of each point; for this reason,
		// on the hull we give priority to exterior halfedges
		for( int e = 0; e < halfEdges.Length; e++ ) {
			int p = triangles[(e % 3 == 2 ? e - 2 : e + 1)];
			if( halfEdges[e] == -1
				|| inedges[p] == -1
			) {
				inedges[p] = e;
			}
		}
		for( int i = 0; i < hull.Length; i++ ) {
			hullIndex[hull[i]] = i;
		}

		if( hull.Length <= 2
			&& hull.Length > 0
		) {
			throw new InvalidOperationException( "Degenerate diagram detected." );
		}

		return new D3Delaunay( inedges, hullIndex );
	}
}
