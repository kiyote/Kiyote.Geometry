using Kiyote.Buffers;
using Kiyote.Geometry.Rasterizers;

namespace Kiyote.Geometry.Visualizer;

/// <summary>
/// Writes a fixed colour into a buffer.  Because this is a struct the rasterizer
/// specializes on it, so the write is inlined rather than dispatched through a
/// delegate once per pixel, and no closure is allocated per call.
/// </summary>
internal readonly struct BufferPixelWriter(
	IBuffer<uint> buffer,
	uint colour
) : IPixelOperation {

	public void Pixel(
		int x,
		int y
	) {
		buffer[x, y] = colour;
	}

	/// <summary>
	/// A whole scanline lives in one contiguous row of the buffer, so the run is
	/// filled with a single vectorized write instead of one indexer call per pixel.
	/// </summary>
	public void PixelSpan(
		int xMin,
		int xMax,
		int y
	) {
		buffer
			.GetRowSpan( y )
			.Slice( xMin, xMax - xMin + 1 )
			.Fill( colour );
	}
}
