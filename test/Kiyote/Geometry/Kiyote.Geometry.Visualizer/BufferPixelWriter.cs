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
}
