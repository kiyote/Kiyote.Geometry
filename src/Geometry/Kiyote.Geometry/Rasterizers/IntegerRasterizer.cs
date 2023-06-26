using System.Reflection.Metadata;

namespace Kiyote.Geometry.Rasterizers;

// Polygon rasterization ported from here: https://www.angelfire.com/linux/myp/ConvexPolRas/ConvexPolRas.cpp

internal sealed class IntegerRasterizer : IRasterizer {
	void IRasterizer.Rasterize(
		IReadOnlyList<Point> polygon,
		Action<int, int> pixelAction
	) {
		int small_y = polygon[0].Y;
		int large_y = polygon[0].Y;
		int delta_y;                        //large_y - small_y + 1 (size of the above array)

		/* Step 1: find small and large y's of all the vertices */
		for( int i = 1; i < polygon.Count; i++ ) {
			if( polygon[i].Y < small_y ) {
				small_y = polygon[i].Y;
			} else if( polygon[i].Y > large_y ) {
				large_y = polygon[i].Y;
			}
		}

		/* Step 2: array that contains small_x and large_x values for each y. */
		delta_y = large_y - small_y + 1;
		int[] sl = new int[delta_y * 2];  //allocate enough memory to save all y values, including large/small

		for( int i = 0; i < delta_y; i++ ) {        //initialize the ScanLine array
			sl[ (i * 2)  + 0] = int.MaxValue;        //INT_MAX because all initial values are less
			sl[ (i * 2)  + 1] = int.MinValue;        //INT_MIN because all initial values are greater
		}

		if( delta_y == 1 ) { // Horizontal line
			for( int i = 0; i < polygon.Count; i++ ) {
				if( polygon[i].X < sl[0] ) {
					sl[0] = polygon[i].X;
				}
				if( polygon[i].X > sl[1] ) {
					sl[1] = polygon[i].X;
				}
			}
			for( int i = sl[0]; i <= sl[1]; i++ ) {
				pixelAction( i, small_y );
			}
			return;
		}

		/* Step 3: go through all the lines in this polygon and build min/max x array. */
		for( int i = 0; i < polygon.Count; i++ ) {
			int ind = ( i + 1 ) % polygon.Count;              //last line will link last vertex with the first (index num-1 to 0)
			if( polygon[ind].Y - polygon[i].Y != 0 ) {
				//initializing current line data (see tutorial on line rasterization for details)
				int dx = polygon[ind].X - polygon[i].X;
				int dy = polygon[ind].Y - polygon[i].Y;
				int incXH;
				int incXL;
				if( dx >= 0 ) {
					incXH = incXL = 1;
				} else {
					dx = -dx;
					incXH = incXL = -1;
				}
				int incYH;
				int incYL;
				if( dy >= 0 ) {
					incYH = incYL = 1;
				} else {
					dy = -dy;
					incYH = incYL = -1;
				}
				int shortD;
				int longD;
				if( dx >= dy ) {
					longD = dx;
					shortD = dy;
					incYL = 0;
				} else {
					longD = dy;
					shortD = dx;
					incXL = 0;
				}
				int d =  (2 * shortD)  - longD;
				int incDL = 2 * shortD;
				int incDH =  (2 * shortD)  -  (2 * longD) ;

				int xc = polygon[i].X;
				int yc = polygon[i].Y;       //initial current x/y values
				for( int j = 0; j <= longD; j++ ) { //step through the current line and remember min/max values at each y
					ind = yc - small_y;
					if( xc < sl[ (ind * 2)  + 0] ) {
						sl[ (ind * 2)  + 0] = xc;    //initial contains INT_MAX so any value is less
					}
					if( xc > sl[ (ind * 2)  + 1] ) {
						sl[ (ind * 2)  + 1] = xc;    //initial contains INT_MIN so any value is greater
					}
					//finding next point on the line ...
					if( d >= 0 ) {
						xc += incXH;
						yc += incYH;
						d += incDH;
					}  //H-type
					else {
						xc += incXL;
						yc += incYL;
						d += incDL;
					}  //L-type
				}
			} //end if
		} //end i for loop

		/* Step 4: drawing horizontal line for each y from small_x to large_x including. */
		for( int i = 0; i < delta_y; i++ ) {
			for( int j = sl[ (i * 2)  + 0]; j <= sl[ (i * 2)  + 1]; j++ ) {
				pixelAction( j, i + small_y );
			}
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
