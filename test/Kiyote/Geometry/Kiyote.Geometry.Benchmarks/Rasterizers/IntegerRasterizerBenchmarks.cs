using Kiyote.Geometry.Rasterizers;

namespace Kiyote.Geometry.Benchmarks.Rasterizers;

/// <summary>
/// Compares filling a buffer one pixel at a time against filling whole scanline
/// runs, which is what an overridden <see cref="IPixelOperation.PixelSpan"/> enables.
/// </summary>
[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
public class IntegerRasterizerBenchmarks {

	private const int Size = 1000;

	private readonly IRasterizer _rasterizer;
	private readonly Point[] _polygon;
	private readonly Point[] _triangle;
	private readonly uint[] _buffer;

	public IntegerRasterizerBenchmarks() {
		_rasterizer = new IntegerRasterizer();
		_buffer = new uint[Size * Size];

		_polygon = [
			new Point( 50, 20 ),
			new Point( 940, 90 ),
			new Point( 880, 950 ),
			new Point( 90, 870 ),
		];

		_triangle = [
			new Point( 10, 10 ),
			new Point( 980, 300 ),
			new Point( 400, 970 ),
		];
	}

	/// <summary>
	/// Uses the default per-pixel implementation of PixelSpan.
	/// </summary>
	private readonly struct PixelWriter(
		uint[] buffer,
		uint colour
	) : IPixelOperation {

		public void Pixel(
			int x,
			int y
		) {
			buffer[x + ( y * Size )] = colour;
		}
	}

	/// <summary>
	/// Fills each run with a single vectorized write.
	/// </summary>
	private readonly struct SpanWriter(
		uint[] buffer,
		uint colour
	) : IPixelOperation {

		public void Pixel(
			int x,
			int y
		) {
			buffer[x + ( y * Size )] = colour;
		}

		public void PixelSpan(
			int xMin,
			int xMax,
			int y
		) {
			buffer
				.AsSpan( ( y * Size ) + xMin, xMax - xMin + 1 )
				.Fill( colour );
		}
	}

	[Benchmark( Baseline = true )]
	public void FillPolygon_PerPixel() {
		_rasterizer.Rasterize( _polygon.AsSpan(), new PixelWriter( _buffer, 0xFFFFFFFFU ) );
	}

	[Benchmark]
	public void FillPolygon_PerSpan() {
		_rasterizer.Rasterize( _polygon.AsSpan(), new SpanWriter( _buffer, 0xFFFFFFFFU ) );
	}

	[Benchmark]
	public void FillTriangle_PerPixel() {
		_rasterizer.Rasterize( _triangle.AsSpan(), new PixelWriter( _buffer, 0xFFFFFFFFU ) );
	}

	[Benchmark]
	public void FillTriangle_PerSpan() {
		_rasterizer.Rasterize( _triangle.AsSpan(), new SpanWriter( _buffer, 0xFFFFFFFFU ) );
	}

	[Benchmark]
	public void Outline_PerPixel() {
		_rasterizer.Rasterize( _polygon.AsSpan(), new PixelWriter( _buffer, 0xFFFFFFFFU ), false );
	}

	[Benchmark]
	public void Outline_PerSpan() {
		_rasterizer.Rasterize( _polygon.AsSpan(), new SpanWriter( _buffer, 0xFFFFFFFFU ), false );
	}
}
