using System.Runtime.CompilerServices;

namespace Kiyote.Geometry.DelaunayVoronoi;

// Ported from: https://github.com/mapbox/delaunator/blob/main/index.js
/*
ISC License

Copyright (c) 2021, Mapbox

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

internal sealed class MapboxDelaunatorFactory {

	public static MapboxDelaunator Create(
		ReadOnlySpan<double> coords
	) {
		int n = coords.Length / 2;

		int[] edgeStack = new int[512];

		// arrays that will store the triangulation graph
		int maxTriangles = Math.Max( ( 2 * n ) - 5, 0 );
		int[] triangles = new int[maxTriangles * 3];
		int[] halfEdges = new int[maxTriangles * 3];

		// temporary arrays for tracking the edges of the advancing convex hull
		int hashSize = (int)Math.Ceiling( Math.Sqrt( n ) );
		int[] hullPrev = new int[n]; // edge to prev edge
		int[] hullNext = new int[n]; // edge to next edge
		int[] hullTri = new int[n]; // edge to adjacent triangle
		int[] hullHash = new int[hashSize];

		// temporary arrays for sorting points
		int[] ids = new int[n];
		double[] dists = new double[n];

		double minX = double.PositiveInfinity;
		double minY = double.PositiveInfinity;
		double maxX = double.NegativeInfinity;
		double maxY = double.NegativeInfinity;

		for( int i = 0; i < n; i++ ) {
			double x = coords[( 2 * i ) + 0];
			double y = coords[( 2 * i ) + 1];
			if( x < minX ) {
				minX = x;
			}

			if( y < minY ) {
				minY = y;
			}

			if( x > maxX ) {
				maxX = x;
			}

			if( y > maxY ) {
				maxY = y;
			}

			ids[i] = i;
		}
		double cx = ( minX + maxY ) / 2.0D;
		double cy = ( minY + maxY ) / 2.0D;

		int i0 = int.MinValue;
		int i1 = int.MinValue;
		int i2 = int.MinValue;
		double minDist = double.PositiveInfinity;
		// pick a seed point close to the center
		for( int i = 0; i < n; i++ ) {
			double d = Dist( cx, cy, coords[2 * i], coords[( 2 * i ) + 1] );
			if( d < minDist ) {
				i0 = i;
				minDist = d;
			}
		}

		double i0x = coords[( 2 * i0 ) + 0];
		double i0y = coords[( 2 * i0 ) + 1];

		minDist = double.PositiveInfinity;
		// find the point closest to the seed
		for( int i = 0; i < n; i++ ) {
			if( i == i0 ) {
				continue;
			}

			double d = Dist( i0x, i0y, coords[2 * i], coords[( 2 * i ) + 1] );
			if( d < minDist
				&& d > 0.0D
			) {
				i1 = i;
				minDist = d;
			}
		}

		double i1x = coords[( 2 * i1 ) + 0];
		double i1y = coords[( 2 * i1 ) + 1];

		double minRadius = double.PositiveInfinity;

		// find the third point which forms the smallest circumcircle with the first two
		for( int i = 0; i < n; i++ ) {
			if( i == i0
				|| i == i1
			) {
				continue;
			}

			double r = Circumradius( i0x, i0y, i1x, i1y, coords[2 * i], coords[( 2 * i ) + 1] );
			if( r < minRadius ) {
				i2 = i;
				minRadius = r;
			}
		}

		double i2x = coords[( 2 * i2 ) + 0];
		double i2y = coords[( 2 * i2 ) + 1];

		if( minRadius == double.PositiveInfinity ) {
			throw new InvalidOperationException( "No Delaunay triangulation exists for this input." );
		}

		if( Orient( i0x, i0y, i1x, i1y, i2x, i2y ) < 0D ) {
			int i = i1;
			double x = i1x;
			double y = i1y;

			i1 = i2;
			i1x = i2x;
			i1y = i2y;
			i2 = i;
			i2x = x;
			i2y = y;
		}

		Circumcenter( i0x, i0y, i1x, i1y, i2x, i2y, out double circumcenterX, out double circumcenterY );

		for( int i = 0; i < n; i++ ) {
			dists[i] = Dist( coords[2 * i], coords[( 2 * i ) + 1], circumcenterX, circumcenterY );
		}

		Quicksort( ids, dists, 0, n - 1 );

		int hullStart = i0;
		int hullSize = 3;

		hullNext[i0] = hullPrev[i2] = i1;
		hullNext[i1] = hullPrev[i0] = i2;
		hullNext[i2] = hullPrev[i1] = i0;

		hullTri[i0] = 0;
		hullTri[i1] = 1;
		hullTri[i2] = 2;

		Array.Fill( hullHash, -1 );
		hullHash[HashKey( i0x, i0y, hashSize, circumcenterX, circumcenterY )] = i0;
		hullHash[HashKey( i1x, i1y, hashSize, circumcenterX, circumcenterY )] = i1;
		hullHash[HashKey( i2x, i2y, hashSize, circumcenterX, circumcenterY )] = i2;

		int trianglesLen = 0;
		 AddTriangle( i0, i1, i2, -1, -1, -1, ref trianglesLen, triangles, halfEdges );

		for( int k = 0; k < ids.Length; k++ ) {
			int i = ids[k];
			double x = coords[( 2 * i ) + 0];
			double y = coords[( 2 * i ) + 1];

			// skip seed triangle points
			if( i == i0
				|| i == i1
				|| i == i2
			) {
				continue;
			}

			// find a visible edge on the convex hull using edge hash
			int start = 0;
			for( int j = 0; j < hashSize; j++ ) {
				int key = HashKey( x, y, hashSize, circumcenterX, circumcenterY );

				start = hullHash[( key + j ) % hashSize];
				if( start != -1
					&& start != hullNext[start]
				) {
					break;
				}
			}

			start = hullPrev[start];
			int e = start;
			int q = hullNext[e];
			while( Orient( x, y, coords[2 * e], coords[( 2 * e ) + 1], coords[2 * q], coords[( 2 * q ) + 1] ) >= 0.0D ) {
				e = q;
				if( e == start ) {
					e = -1;
					break;
				}
				q = hullNext[e];
			}

			// likely a near-duplicate point; skip it
			if( e == -1 ) {
				continue;
			}

			// add the first triangle from the point
			int t = AddTriangle( e, i, hullNext[e], -1, -1, hullTri[e], ref trianglesLen, triangles, halfEdges );

			// recursively flip triangles from the point until they satisfy the Delaunay condition
			hullTri[i] = Legalize( t + 2, hullStart, halfEdges, edgeStack, triangles, coords, hullTri, hullPrev );
			hullTri[e] = t; // keep track of boundary triangles on the hull
			hullSize++;

			// walk forward through the hull, adding more triangles and flipping recursively
			int next = hullNext[e];
			q = hullNext[next];
			while( Orient( x, y, coords[2 * next], coords[( 2 * next ) + 1], coords[2 * q], coords[( 2 * q ) + 1] ) < 0.0D ) {
				t = AddTriangle( next, i, q, hullTri[i], -1, hullTri[next], ref trianglesLen, triangles, halfEdges );
				hullTri[i] = Legalize( t + 2, hullStart, halfEdges, edgeStack, triangles, coords, hullTri, hullPrev );
				hullNext[next] = next; // mark as removed
				hullSize--;
				next = q;

				q = hullNext[next];
			}

			// walk backward from the other side, adding more triangles and flipping
			if( e == start ) {
				q = hullPrev[e];
				while( Orient( x, y, coords[2 * q], coords[( 2 * q ) + 1], coords[2 * e], coords[( 2 * e ) + 1] ) < 0.0D ) {
					t = AddTriangle( q, i, e, -1, hullTri[e], hullTri[q], ref trianglesLen, triangles, halfEdges );
					 Legalize( t + 2, hullStart, halfEdges, edgeStack, triangles, coords, hullTri, hullPrev );
					hullTri[q] = t;
					hullNext[e] = e; // mark as removed
					hullSize--;
					e = q;

					q = hullPrev[e];
				}
			}

			// update the hull indices
			hullStart = hullPrev[i] = e;
			hullNext[e] = hullPrev[next] = i;
			hullNext[i] = next;

			// save the two new edges in the hash table
			hullHash[HashKey( x, y, hashSize, circumcenterX, circumcenterY )] = i;
			hullHash[HashKey( coords[2 * e], coords[( 2 * e ) + 1], hashSize, circumcenterX, circumcenterY )] = e;
		}

		int[] hull = new int[hullSize];
		int s = hullStart;
		for( int i = 0; i < hullSize; i++ ) {
			hull[i] = s;
			s = hullNext[s];
		}

		// trim typed triangle mesh arrays
		triangles = triangles[0..trianglesLen];
		halfEdges = halfEdges[0..trianglesLen];

		return new MapboxDelaunator(
			hull,
			triangles,
			halfEdges
		);
	}

	// NOTE - This represents the largest single contributor to runtime
	private static int Legalize(
		int a,
		int hullStart,
		Span<int> halfEdges,
		Span<int> edgeStack,
		Span<int> triangles,
		ReadOnlySpan<double> coords,
		Span<int> hullTri,
		Span<int> hullPrev
	) {
		int i = 0;
		int ar;
		while( true ) {
			int b = halfEdges[a];

			/* if the pair of triangles doesn't satisfy the Delaunay condition
			 * (p1 is inside the circumcircle of [p0, pl, pr]), flip them,
			 * then do the same check/flip recursively for the new pair of triangles
			 *
			 *           pl                    pl
			 *          /||\                  /  \
			 *       al/ || \bl            al/    \a
			 *        /  ||  \              /      \
			 *       /  a||b  \    flip    /___ar___\
			 *     p0\   ||   /p1   =>   p0\---bl---/p1
			 *        \  ||  /              \      /
			 *       ar\ || /br             b\    /br
			 *          \||/                  \  /
			 *           pr                    pr
			 */
			int a0 = a - ( a % 3 );
			ar = a0 + ( ( a + 2 ) % 3 );

			if( b == -1 ) {
				if( i == 0 ) {
					break;
				}
				a = edgeStack[--i];
				continue;
			}

			int b0 = b - ( b % 3 );
			int a1 = a0 + ( ( a + 1 ) % 3 );
			int b1 = b0 + ( ( b + 2 ) % 3 );

			int p0 = triangles[ar];
			int pr = triangles[a];
			int pl = triangles[a1];
			int p1 = triangles[b1];

			bool illegal = InCircle(
				coords[2 * p0], coords[( 2 * p0 ) + 1],
				coords[2 * pr], coords[( 2 * pr ) + 1],
				coords[2 * pl], coords[( 2 * pl ) + 1],
				coords[2 * p1], coords[( 2 * p1 ) + 1]
			);

			if( illegal ) {
				triangles[a] = p1;
				triangles[b] = p0;

				int hb1 = halfEdges[b1];

				// edge swapped on the other side of the hull (rare); fix the halfedge reference
				if( hb1 == -1 ) {
					int e = hullStart;
					do {
						if( hullTri[e] == b1 ) {
							hullTri[e] = a;
							break;
						}
						e = hullPrev[e];
					} while( e != hullStart );
				}
				Link( a, hb1, halfEdges );
				Link( b, halfEdges[ar], halfEdges );
				Link( ar, b1, halfEdges );

				int br = b0 + ( ( b + 1 ) % 3 );

				// don't worry about hitting the cap: it can only happen on extremely degenerate input
				if( i < edgeStack.Length ) {
					edgeStack[i++] = br;
				}
			} else {
				if( i == 0 ) {
					break;
				}
				a = edgeStack[--i];
			}
		}

		return ar;
	}

	private static int AddTriangle(
		int i0,
		int i1,
		int i2,
		int a,
		int b,
		int c,
		ref int trianglesLen,
		Span<int> triangles,
		Span<int> halfEdges
	) {
		int t = trianglesLen;

		triangles[t] = i0;
		triangles[t + 1] = i1;
		triangles[t + 2] = i2;

		Link( t, a, halfEdges );
		Link( t + 1, b, halfEdges );
		Link( t + 2, c, halfEdges );

		trianglesLen += 3;

		return t;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static void Link(
		int a,
		int b,
		Span<int> halfEdges
	) {
		halfEdges[a] = b;
		if( b != -1 ) {
			halfEdges[b] = a;
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static int HashKey(
		double x,
		double y,
		int hashSize,
		double circumcenterX,
		double circumcenterY
	) {
		return (int)Math.Floor( PseudoAngle( x - circumcenterX, y - circumcenterY ) * hashSize ) % hashSize;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	internal static double Orient(
		double ax,
		double ay,
		double bx,
		double by,
		double cx,
		double cy
	) {
		return ( ( ay - cy ) * ( bx - cx ) ) - ( ( ax - cx ) * ( by - cy ) );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static double Dist(
		double ax,
		double ay,
		double bx,
		double by
	) {
		double dx = ax - bx;
		double dy = ay - by;
		return ( dx * dx ) + ( dy * dy );
	}

	private static double Circumradius(
		double ax,
		double ay,
		double bx,
		double by,
		double cx,
		double cy
	) {
		double dx = bx - ax;
		double dy = by - ay;
		double ex = cx - ax;
		double ey = cy - ay;

		double bl = ( dx * dx ) + ( dy * dy );
		double cl = ( ex * ex ) + ( ey * ey );
		double d = 0.5 / ( ( dx * ey ) - ( dy * ex ) );

		double x = ( ( ey * bl ) - ( dy * cl ) ) * d;
		double y = ( ( dx * cl ) - ( ex * bl ) ) * d;

		return ( x * x ) + ( y * y );
	}

	internal static void Circumcenter(
		double ax,
		double ay,
		double bx,
		double by,
		double cx,
		double cy,
		out double x,
		out double y
	) {
		double dx = bx - ax;
		double dy = by - ay;
		double ex = cx - ax;
		double ey = cy - ay;

		double bl = ( dx * dx ) + ( dy * dy );
		double cl = ( ex * ex ) + ( ey * ey );
		double d = 0.5D / ( ( dx * ey ) - ( dy * ex ) );

		x = ax + ( ( ( ey * bl ) - ( dy * cl ) ) * d );
		y = ay + ( ( ( dx * cl ) - ( ex * bl ) ) * d );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static double PseudoAngle(
		double dx,
		double dy
	) {
		double p = dx / ( Math.Abs( dx ) + Math.Abs( dy ) );
		return ( dy > 0.0D ? 3.0D - p : 1.0D + p ) / 4.0D; // [0..1]
	}

	private static bool InCircle(
		double ax,
		double ay,
		double bx,
		double by,
		double cx,
		double cy,
		double px,
		double py
	) {
		double dx = ax - px;
		double dy = ay - py;
		double ex = bx - px;
		double ey = by - py;
		double fx = cx - px;
		double fy = cy - py;

		double ap = ( dx * dx ) + ( dy * dy );
		double bp = ( ex * ex ) + ( ey * ey );
		double cp = ( fx * fx ) + ( fy * fy );

		return ( dx * ( ( ey * cp ) - ( bp * fy ) ) ) -
				( dy * ( ( ex * cp ) - ( bp * fx ) ) ) +
				( ap * ( ( ex * fy ) - ( ey * fx ) ) ) < 0.0D;
	}

	private static void Quicksort(
		Span<int> ids,
		Span<double> dists,
		int left,
		int right
	) {
		if( right - left <= 20 ) {
			for( int i = left + 1; i <= right; i++ ) {
				int temp = ids[i];
				double tempDist = dists[temp];
				int j = i - 1;
				while( j >= left && dists[ids[j]] > tempDist ) {
					ids[j + 1] = ids[j--];
				}
				ids[j + 1] = temp;
			}
		} else {
			int median = ( left + right ) >> 1;
			int i = left + 1;
			int j = right;
			Swap( ids, median, i );
			if( dists[ids[left]] > dists[ids[right]] ) {
				Swap( ids, left, right );
			}
			if( dists[ids[i]] > dists[ids[right]] ) {
				Swap( ids, i, right );
			}
			if( dists[ids[left]] > dists[ids[i]] ) {
				Swap( ids, left, i );
			}

			int temp = ids[i];
			double tempDist = dists[temp];
			while( true ) {
				do {
					i++;
				} while( dists[ids[i]] < tempDist );
				do {
					j--;
				} while( dists[ids[j]] > tempDist );
				if( j < i ) {
					break;
				}
				Swap( ids, i, j );
			}
			ids[left + 1] = ids[j];
			ids[j] = temp;

			if( right - i + 1 >= j - left ) {
				Quicksort( ids, dists, i, right );
				Quicksort( ids, dists, left, j - 1 );
			} else {
				Quicksort( ids, dists, left, j - 1 );
				Quicksort( ids, dists, i, right );
			}
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static void Swap(
		Span<int> arr,
		int i,
		int j
	) {
		(arr[j], arr[i]) = (arr[i], arr[j]);
	}

}

