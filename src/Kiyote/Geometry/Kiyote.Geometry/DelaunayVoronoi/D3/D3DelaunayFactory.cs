namespace Kiyote.Geometry.DelaunayVoronoi.D3;

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
		Point[] sites = DiagramSites.SnapshotAndConvert( points, sanitizePoints, out double[] coordinateBuffer );
		ReadOnlySpan<double> coords = coordinateBuffer.AsSpan( 0, sites.Length * 2 );
		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( coords, rejectDuplicates: !sanitizePoints );

		ReadOnlySpan<int> triangleIndices = delaunator.Triangles;
		Triangle[] triangles = new Triangle[triangleIndices.Length / 3];
		for( int i = 0; i < triangles.Length; i++ ) {
			int edge = i * 3;
			triangles[i] = new Triangle(
				sites[triangleIndices[edge]],
				sites[triangleIndices[edge + 1]],
				sites[triangleIndices[edge + 2]]
			);
		}

		ReadOnlySpan<int> hullIndices = delaunator.Hull;
		Point[] hull = new Point[hullIndices.Length];
		for( int i = 0; i < hull.Length; i++ ) {
			hull[i] = sites[hullIndices[i]];
		}

		Delaunay result = new Delaunay(
			Array.AsReadOnly( sites ),
			Array.AsReadOnly( triangles ),
			Array.AsReadOnly( hull )
		);

		return result;
	}

	internal static void BuildIndices(
		MapboxDelaunator delaunator,
		Span<int> inedges,
		Span<int> hullIndex
	) {
		if( inedges.Length != hullIndex.Length ) {
			throw new ArgumentException( "Traversal index buffers must have the same site count.", nameof( hullIndex ) );
		}

		ReadOnlySpan<int> halfEdges = delaunator.HalfEdges;
		ReadOnlySpan<int> hull = delaunator.Hull;
		ReadOnlySpan<int> triangles = delaunator.Triangles;
		inedges.Fill( -1 );
		hullIndex.Fill( -1 );

		// Compute an index from each point to an (arbitrary) incoming halfedge
		// Used to give the first neighbor of each point; for this reason,
		// on the hull we give priority to exterior halfedges
		for( int e = 0; e < halfEdges.Length; e++ ) {
			int p = triangles[( e % 3 == 2 ? e - 2 : e + 1 )];
			if( halfEdges[e] == -1
				|| inedges[p] == -1
			) {
				inedges[p] = e;
			}
		}
		for( int i = 0; i < hull.Length; i++ ) {
			hullIndex[hull[i]] = i;
		}

		if( hull.Length <= 2 ) {
			throw new InvalidOperationException( "Degenerate diagram detected." );
		}
	}
}
