using System.Reflection.Metadata;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Rasterizers;

// Polygon rasterization ported from here: https://www.angelfire.com/linux/myp/ConvexPolRas/ConvexPolRas.cpp

internal sealed class IntegerRasterizer : IRasterizer {

	void IRasterizer.Rasterize(
		IReadOnlyList<Point> polygon,
		Action<int, int> pixelAction
	) {
		// Find the smallest and largest Y's of the polygon
		int small_y = polygon[0].Y;
		int large_y = polygon[0].Y;
		for( int i = 1; i < polygon.Count; i++ ) {
			if( polygon[i].Y < small_y ) {
				small_y = polygon[i].Y;
			} else if( polygon[i].Y > large_y ) {
				large_y = polygon[i].Y;
			}
		}

		// Allocate an array that can hold the X(min) and X(max) for each Y value
		int delta_y = large_y - small_y + 1;

		// Horizontal line
		if( delta_y == 1 ) {
			int min = int.MaxValue;
			int max = int.MinValue;
			for( int i = 0; i < polygon.Count; i++ ) {
				int ind = ( i + 1 ) % polygon.Count;

				FindRange( polygon[i], polygon[ind], out int xMin, out int xMax );
				if( xMin < min ) {
					min = xMin;
				}
				if( xMax > max ) {
					max = xMax;
				}
			}

			int y = polygon[0].Y;
			for( int i = min; i <= max; i++ ) {
				pixelAction( i, y );
			}
			return;
		}

		int[] sl = new int[delta_y * 2];
		for (int i = 0; i < sl.Length; i += 2) {
			sl[i] = int.MaxValue;
			sl[i + 1] = int.MinValue;
		}

		// Go through every line pair, rasterizing the lines to find X(min) and
		// X(max) for each horizonal line
		for( int i = 0; i < polygon.Count; i++ ) {
			int ind = ( i + 1 ) % polygon.Count;

			// Now rasterize from polygon[i] to polygon[ind]
			( this as IRasterizer ).Rasterize(
				polygon[i],
				polygon[ind],
				( int x, int y ) => {
					int curY = (y - small_y) * 2;
					if( x < sl[curY] ) {
						sl[curY] = x;
					}
					curY += 1;
					if( x > sl[curY] ) {
						sl[curY] = x;
					}
				}
			);
		}

		// Go through each line and draw a horizonal line 
		for( int i = 0; i < delta_y; i++ ) {
			for( int j = sl[( i * 2 ) + 0]; j <= sl[( i * 2 ) + 1]; j++ ) {
				pixelAction( j, i + small_y );
			}
		}
	}

	private static void FindRange(
		Point p0,
		Point p1,
		out int xMin,
		out int xMax
	) {
		if( p0.X < p1.X ) {
			xMin = p0.X;
			xMax = p1.X;
		} else {
			xMin = p1.X;
			xMax = p0.X;
		}
	}

	void IRasterizer.Rasterize(
		Point p1,
		Point p2,
		Action<int, int> pixelAction
	) {
		int x0 = p1.X;
		int y0 = p1.Y;
		int x1 = p2.X;
		int y1 = p2.Y;
		int dx = Math.Abs( x1 - x0 );
		int sx = x0 < x1 ? 1 : -1;
		int dy = -Math.Abs( y1 - y0 );
		int sy = y0 < y1 ? 1 : -1;
		int error = dx + dy;
		while( true ) {
			pixelAction( x0, y0 );
			if( x0 == x1
				&& y0 == y1
			) {
				break;
			}
			int e2 = 2 * error;
			if( e2 >= dy ) {
				if( x0 == x1 ) {
					break;
				}
				error += dy;
				x0 += sx;
			}
			if( e2 <= dx ) {
				if( y0 == y1 ) {
					break;
				}
				error += dx;
				y0 += sy;
			}
		}
	}
}
