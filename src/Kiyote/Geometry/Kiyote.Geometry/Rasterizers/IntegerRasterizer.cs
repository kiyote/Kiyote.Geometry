using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Rasterizers;

// Polygon rasterization ported from here: https://www.angelfire.com/linux/myp/ConvexPolRas/ConvexPolRas.cpp

public sealed class IntegerRasterizer : IRasterizer {

	/// <summary>
	/// Accumulates the minimum and maximum X of each scanline the traced line
	/// touches.  Holding the span directly avoids the closure a lambda would
	/// require.
	/// </summary>
	private readonly ref struct ScanlineSink : IPixelOperation {

		private readonly Span<int> _scanlines;
		private readonly int _smallY;

		public ScanlineSink(
			Span<int> scanlines,
			int smallY
		) {
			_scanlines = scanlines;
			_smallY = smallY;
		}

		public void Pixel(
			int x,
			int y
		) {
			int curY = ( y - _smallY ) * 2;
			if( x < _scanlines[curY] ) {
				_scanlines[curY] = x;
			}
			curY += 1;
			if( x > _scanlines[curY] ) {
				_scanlines[curY] = x;
			}
		}

		/// <summary>
		/// Ref structs cannot inherit the default implementation (CS9245), so it must
		/// be provided explicitly even though <see cref="TraceLine"/> only ever calls
		/// <see cref="Pixel"/>, leaving this unreachable.  Were it to be called, only
		/// the endpoints of a run can move the min/max, so the interior of the run is
		/// skipped entirely.
		/// </summary>
		[ExcludeFromCodeCoverage( Justification = "Required by CS9245 but unreachable; TraceLine only calls Pixel." )]
		public void PixelSpan(
			int xMin,
			int xMax,
			int y
		) {
			int curY = ( y - _smallY ) * 2;
			if( xMin < _scanlines[curY] ) {
				_scanlines[curY] = xMin;
			}
			curY += 1;
			if( xMax > _scanlines[curY] ) {
				_scanlines[curY] = xMax;
			}
		}
	}

	void IRasterizer.Rasterize<TPixelOperation>(
		ReadOnlySpan<Point> polygon,
		TPixelOperation pixelAction,
		bool filled
	) {
		// Rasterizing only the border means each edge can be emitted directly
		// without needing to calculate the horizontal spans.
		if( !filled ) {
			for( int i = 0; i < polygon.Length; i++ ) {
				int ind = i + 1;
				if( ind == polygon.Length ) {
					ind = 0;
				}

				TraceLine(
					polygon[i],
					polygon[ind],
					pixelAction
				);
			}
			return;
		}

		// Find the smallest and largest Y's of the polygon
		int small_y = polygon[0].Y;
		int large_y = polygon[0].Y;
		for( int i = 1; i < polygon.Length; i++ ) {
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
			for( int i = 0; i < polygon.Length; i++ ) {
				int ind = i + 1;
				if( ind == polygon.Length ) {
					ind = 0;
				}

				FindRange( polygon[i], polygon[ind], out int xMin, out int xMax );
				if( xMin < min ) {
					min = xMin;
				}
				if( xMax > max ) {
					max = xMax;
				}
			}

			int y = polygon[0].Y;
			if( min <= max ) {
				pixelAction.PixelSpan( min, max, y );
			}
			return;
		}

		// The scanline buffer scales with the height of the polygon, so it is rented
		// rather than allocated.  Rent can hand back a larger array than requested, so
		// only the portion actually in use is initialized and passed along.
		int[] rented = ArrayPool<int>.Shared.Rent( delta_y * 2 );
		try {
			Span<int> sl = rented.AsSpan( 0, delta_y * 2 );
			for( int i = 0; i < sl.Length; i += 2 ) {
				sl[i] = int.MaxValue;
				sl[i + 1] = int.MinValue;
			}

			// Go through every line pair, rasterizing the lines to find X(min) and
			// X(max) for each horizonal line
			for( int i = 0; i < polygon.Length; i++ ) {
				int ind = i + 1;
				if( ind == polygon.Length ) {
					ind = 0;
				}

				// Now rasterize from polygon[i] to polygon[ind]
				TraceLine(
					polygon[i],
					polygon[ind],
					new ScanlineSink( sl, small_y )
				);
			}

			// Go through each line and draw a horizonal line.  Each scanline is a
			// contiguous run, so it is handed over whole rather than one pixel at a
			// time; sinks backed by contiguous storage can then fill it in one write.
			for( int i = 0; i < delta_y; i++ ) {
				int xMin = sl[( i * 2 ) + 0];
				int xMax = sl[( i * 2 ) + 1];
				if( xMin <= xMax ) {
					pixelAction.PixelSpan( xMin, xMax, i + small_y );
				}
			}
		} finally {
			ArrayPool<int>.Shared.Return( rented );
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

	void IRasterizer.Rasterize<TPixelOperation>(
		Point p1,
		Point p2,
		TPixelOperation pixelAction
	) {
		TraceLine( p1, p2, pixelAction );
	}

	/// <summary>
	/// The single Bresenham implementation used by every code path.  The sink is a
	/// struct type parameter so the JIT emits a specialized copy with the pixel
	/// handling inlined.
	/// </summary>
	private static void TraceLine<TPixelOperation>(
		Point p1,
		Point p2,
		TPixelOperation sink
	) where TPixelOperation : struct, IPixelOperation, allows ref struct {
		// The "error" accumulates differently depending on whether you draw
		// left-to-right or right-to-left, so we'll prefer drawing left-to-right
		// for consistency.
		if (p1.X > p2.X) {
			(p2, p1) = (p1, p2);
		}
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
			sink.Pixel( x0, y0 );
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
