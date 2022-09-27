namespace Kiyote.Geometry.DelaunayVoronoi;

// Ported from https://github.com/mapbox/delaunator

internal sealed class DelaunatorFactory : IDelaunatorFactory {

	private readonly double Epsilon = Math.Pow( 2, -52 );

	Delaunator IDelaunatorFactory.Create(
		IEnumerable<IPoint> input
	) {
		int[] edgeStack = new int[512];

		List<IPoint> distinctInput = input.Distinct().ToList();
		double[] coords = new double[distinctInput.Count * 2];
		for( int i = 0; i < distinctInput.Count; i++ ) {
			IPoint point = distinctInput[i];
			coords[2 * i] = point.X;
			coords[ 2 * i  + 1] = point.Y;
		}

		if( coords.Length < 6 ) {
			throw new ArgumentException( "Must provide at least 3 distinct points.", nameof( input ) );
		}

		int n = coords.Length / 2;
		int[] hullPrev = new int[n];
		int[] hullNext = new int[n];
		int[] hullTri = new int[n];
		int[] ids = new int[n];
		double[] dists = new double[n];

		int maximumTriangles =  2 * n  - 5;

		int triangleCapacity = maximumTriangles * 3;
		int[] triangles = new int[triangleCapacity];

		// Half edges represent the "connectivity" between edges.  That is,
		// given 3 edges of a triangle, there is a half-edge entry that corresponds
		// to the edge of the adjacent triangle.  If there is no triangle on the
		// other side of the edge, it will be -1.  That is: a given half-edge
		// entry indicates the edge on the other side of that edge (if connected)
		//
		// Given edges numbered
		//       1
		//    _______
		//    |\    |
		//    | \   |
		//  5 | 4\2 |  0
		//    |   \ |
		//    |____\|
		//
		//       3
		//
		// The half-edges would be
		//       -1
		//    _______
		//    |\    |
		//    | \   |
		// -1 | 2\4 |  -1
		//    |   \ |
		//    |____\|
		//
		//       -1
		//
		// So, halfEdges[2] = 4
		// And halfEdges[4] = 2
		int halfEdgeCapacity = maximumTriangles * 3;
		int[] halfEdges = new int[halfEdgeCapacity];

		int hashSize = (int)Math.Ceiling( Math.Sqrt( n ) );

		double minX = double.PositiveInfinity;
		double minY = double.PositiveInfinity;
		double maxX = double.NegativeInfinity;
		double maxY = double.NegativeInfinity;

		for( int i = 0; i < n; i++ ) {
			double x = coords[2 * i];
			double y = coords[ 2 * i  + 1];
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

		double minDist = double.PositiveInfinity;
		int i0 = 0;
		int i1 = 0;
		int i2 = 0;

		// pick a seed point close to the center
		for( int i = 0; i < n; i++ ) {
			double d = Dist( cx, cy, coords[2 * i], coords[ 2 * i  + 1] );
			if( d < minDist ) {
				i0 = i;
				minDist = d;
			}
		}

		double i0x = coords[2 * i0];
		double i0y = coords[ 2 * i0  + 1];

		minDist = double.PositiveInfinity;

		// find the point closest to the seed
		for( int i = 0; i < n; i++ ) {
			if( i == i0 ) {
				continue;
			}

			double d = Dist( i0x, i0y, coords[2 * i], coords[ 2 * i  + 1] );
			if( d < minDist
				&& d > 0.0D
			) {
				i1 = i;
				minDist = d;
			}
		}

		double i1x = coords[2 * i1];
		double i1y = coords[ 2 * i1  + 1];

		double minRadius = double.PositiveInfinity;

		// find the third point which forms the smallest circumcircle with the first two
		for( int i = 0; i < n; i++ ) {
			if( i == i0
				|| i == i1
			) {
				continue;
			}

			double r = Circumradius( i0x, i0y, i1x, i1y, coords[2 * i], coords[ 2 * i  + 1] );
			if( r < minRadius ) {
				i2 = i;
				minRadius = r;
			}
		}

		double i2x = coords[2 * i2];
		double i2y = coords[ 2 * i2  + 1];

		if( minRadius == double.PositiveInfinity ) {
			throw new InvalidOperationException( "No Delaunay triangulation exists for this input." );
		}

		if( Orient( i0x, i0y, i1x, i1y, i2x, i2y ) < 0.0D ) {
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
			dists[i] = Dist( coords[2 * i], coords[ 2 * i  + 1], circumcenterX, circumcenterY );
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

		int[] hullHash = new int[hashSize];
		Array.Fill( hullHash, -1 );
		hullHash[HashKey( i0x, i0y, hashSize, circumcenterX, circumcenterY )] = i0;
		hullHash[HashKey( i1x, i1y, hashSize, circumcenterX, circumcenterY )] = i1;
		hullHash[HashKey( i2x, i2y, hashSize, circumcenterX, circumcenterY )] = i2;

		int trianglesLen = 0;
		AddTriangle( i0, i1, i2, -1, -1, -1, ref trianglesLen, triangles, halfEdges );

		double xp = 0;
		double yp = 0;
		for( int k = 0; k < ids.Length; k++ ) {
			int i = ids[k];
			double x = coords[2 * i];
			double y = coords[ 2 * i  + 1];

			if( k > 0 && Math.Abs( x - xp ) <= Epsilon
				&& Math.Abs( y - yp ) <= Epsilon
			) {
				// Because we use integer input, this is basically not an issue
				continue;
			}
			xp = x;
			yp = y;

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
			while( Orient( x, y, coords[2 * e], coords[ 2 * e  + 1], coords[2 * q], coords[ 2 * q  + 1] ) >= 0.0D ) {
				e = q;
				if( e == start ) {
					e = -1;
					break;
				}
				q = hullNext[e];
			}

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
			while( Orient( x, y, coords[2 * next], coords[ 2 * next  + 1], coords[2 * q], coords[ 2 * q  + 1] ) < 0.0D ) {
				t = AddTriangle( next, i, q, hullTri[i], -1, hullTri[next], ref trianglesLen, triangles, halfEdges );
				hullTri[i] = Legalize( t + 2, hullStart, halfEdges, edgeStack, triangles, coords, hullTri, hullPrev );
				hullNext[next] = next; // mark as removed
				hullSize--;
				next = q;

				q = hullNext[next];
			}

			// walk backward from the other side, adding more triangles and flippin
			if( e == start ) {
				q = hullPrev[e];
				while( Orient( x, y, coords[2 * q], coords[ 2 * q  + 1], coords[2 * e], coords[ 2 * e  + 1] ) < 0.0D ) {
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
			hullHash[HashKey( coords[2 * e], coords[ 2 * e  + 1], hashSize, circumcenterX, circumcenterY )] = e;
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

		return new Delaunator( coords, hull, triangles, halfEdges );
	}

	private static int Legalize(
		int a,
		int hullStart,
		int[] halfEdges,
		int[] edgeStack,
		int[] triangles,
		double[] coords,
		int[] hullTri,
		int[] hullPrev
	) {
		int i = 0;
		int ar;
		while( true ) {
			int b = halfEdges[a];

			int a0 = a -  a % 3 ;
			ar = a0 +  ( a + 2 ) % 3 ;

			if( b == -1 ) {
				if( i == 0 ) {
					break;
				}
				a = edgeStack[--i];
				continue;
			}

			int b0 = b -  b % 3 ;
			int a1 = a0 +  ( a + 1 ) % 3 ;
			int b1 = b0 +  ( b + 2 ) % 3 ;

			int p0 = triangles[ar];
			int pr = triangles[a];
			int pl = triangles[a1];
			int p1 = triangles[b1];

			bool illegal = InCircle(
				coords[2 * p0], coords[ 2 * p0  + 1],
				coords[2 * pr], coords[ 2 * pr  + 1],
				coords[2 * pl], coords[ 2 * pl  + 1],
				coords[2 * p1], coords[ 2 * p1  + 1] );

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

				int br = b0 +  ( b + 1 ) % 3 ;

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
		int[] triangles,
		int[] halfEdges
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

	private static void Link(
		int a,
		int b,
		int[] halfEdges
	) {
		halfEdges[a] = b;
		if( b != -1 ) {
			halfEdges[b] = a;
		}
	}

	private static int HashKey(
		double x,
		double y,
		int hashSize,
		double circumcenterX,
		double circumcenterY
	) {
		return (int)Math.Floor( PseudoAngle( x - circumcenterX, y - circumcenterY ) * hashSize ) % hashSize;
	}

	private static double Orient(
		double ax,
		double ay,
		double bx,
		double by,
		double cx,
		double cy
	) {
		return  ( ay - cy ) * ( bx - cx )  -  ( ax - cx ) * ( by - cy ) ;
	}

	private static double Dist(
		double ax,
		double ay,
		double bx,
		double by
	) {
		double dx = ax - bx;
		double dy = ay - by;
		return  dx * dx  +  dy * dy ;
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

		double bl =  dx * dx  +  dy * dy ;
		double cl =  ex * ex  +  ey * ey ;
		double d = 0.5 / (  dx * ey  -  dy * ex  );

		double x = (  ey * bl  -  dy * cl  ) * d;
		double y = (  dx * cl  -  ex * bl  ) * d;

		return  x * x  +  y * y ;
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

		double bl =  dx * dx  +  dy * dy ;
		double cl =  ex * ex  +  ey * ey ;
		double d = 0.5D / (  dx * ey  -  dy * ex  );

		x = ax +  (  ey * bl  -  dy * cl  ) * d ;
		y = ay +  (  dx * cl  -  ex * bl  ) * d ;
	}

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

		double ap =  dx * dx  +  dy * dy ;
		double bp =  ex * ex  +  ey * ey ;
		double cp =  fx * fx  +  fy * fy ;

		return  dx * (  ey * cp  -  bp * fy  )  -
			    dy * (  ex * cp  -  bp * fx  )  +
			    ap * (  ex * fy  -  ey * fx  )  < 0.0D;
	}

	//quicksort(this._ids, this._dists, 0, n - 1);
	private void Quicksort(
		int[] ids,
		double[] dists,
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
			int median =  left + right  >> 1;
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

	private static void Swap(
		int[] arr,
		int i,
		int j
	) {
		(arr[j], arr[i]) = (arr[i], arr[j]);
	}

}
