using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Kiyote.Geometry.Rasterizers;

public interface IRasterizer {

	/// <summary>
	/// The core polygon rasterization primitive.  Taking a span allows callers to
	/// supply vertices from the stack, an array, or a pooled buffer without any
	/// allocation, and taking a struct pixel action lets the callback be inlined.
	/// </summary>
	void Rasterize<TPixelOperation>(
		ReadOnlySpan<Point> polygon,
		TPixelOperation pixelAction,
		bool filled = true
	) where TPixelOperation : struct, IPixelOperation, allows ref struct;

	/// <summary>
	/// The core line rasterization primitive.
	/// </summary>
	void Rasterize<TPixelOperation>(
		Point p1,
		Point p2,
		TPixelOperation pixelAction
	) where TPixelOperation : struct, IPixelOperation, allows ref struct;

	void Rasterize<TPixelOperation>(
		IReadOnlyList<Point> polygon,
		TPixelOperation pixelAction,
		bool filled = true
	) where TPixelOperation : struct, IPixelOperation, allows ref struct {
		ArgumentNullException.ThrowIfNull( polygon );

		// The common list types can expose their storage directly, so no copy is
		// needed at all.
		switch( polygon ) {
			case Point[] array:
				Rasterize( array.AsSpan(), pixelAction, filled );
				return;
			case List<Point> list:
				Rasterize( CollectionsMarshal.AsSpan( list ), pixelAction, filled );
				return;
			case ImmutableArray<Point> immutable:
				Rasterize( immutable.AsSpan(), pixelAction, filled );
				return;
		}

		// Anything else has to be copied, but it can be copied into a pooled buffer
		// so that no garbage is produced.
		Point[] rented = ArrayPool<Point>.Shared.Rent( polygon.Count );
		try {
			for( int i = 0; i < polygon.Count; i++ ) {
				rented[i] = polygon[i];
			}

			Rasterize( rented.AsSpan( 0, polygon.Count ), pixelAction, filled );
		} finally {
			ArrayPool<Point>.Shared.Return( rented );
		}
	}

	void Rasterize<TPixelOperation>(
		Polygon polygon,
		TPixelOperation pixelAction,
		bool filled = true
	) where TPixelOperation : struct, IPixelOperation, allows ref struct {
		ArgumentNullException.ThrowIfNull( polygon );

		Rasterize( polygon.Points, pixelAction, filled );
	}

	void Rasterize<TPixelOperation>(
		Triangle triangle,
		TPixelOperation pixelAction,
		bool filled = true
	) where TPixelOperation : struct, IPixelOperation, allows ref struct {
		ArgumentNullException.ThrowIfNull( triangle );

		// A collection expression targeting ReadOnlySpan<Point> is stack allocated,
		// so this forwards to the span overload without allocating.
		ReadOnlySpan<Point> points = [triangle.P1, triangle.P2, triangle.P3];

		Rasterize( points, pixelAction, filled );
	}

	void Rasterize(
		ReadOnlySpan<Point> polygon,
		Action<int, int> pixelAction,
		bool filled = true
	) {
		Rasterize( polygon, new ActionPixelOperation( pixelAction ), filled );
	}

	void Rasterize(
		IReadOnlyList<Point> polygon,
		Action<int, int> pixelAction,
		bool filled = true
	) {
		Rasterize( polygon, new ActionPixelOperation( pixelAction ), filled );
	}

	void Rasterize(
		Polygon polygon,
		Action<int, int> pixelAction,
		bool filled = true
	) {
		Rasterize( polygon, new ActionPixelOperation( pixelAction ), filled );
	}

	void Rasterize(
		Point p1,
		Point p2,
		Action<int, int> pixelAction
	) {
		Rasterize( p1, p2, new ActionPixelOperation( pixelAction ) );
	}

	void Rasterize(
		Triangle triangle,
		Action<int, int> pixelAction,
		bool filled = true
	) {
		Rasterize( triangle, new ActionPixelOperation( pixelAction ), filled );
	}
}
