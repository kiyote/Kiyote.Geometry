using Kiyote.Geometry.Rasterizers;

namespace Kiyote.Geometry.Benchmarks.Rasterizers;

/// <summary>
/// Compares filling a buffer one pixel at a time against filling whole scanline
/// runs, which is what an overridden <see cref="IPixelOperation.PixelSpan"/> enables.
/// </summary>
[MemoryDiagnoser( false )]
public class IntegerRasterizerBenchmarks {

	private const int Size = 1000;

	private readonly IRasterizer _rasterizer;
	private readonly Point[] _polygon;
	private readonly Point[] _triangle;
	private readonly uint[] _buffer;
	private readonly Point[] _smallVertices = [new Point( 1, 1 ), new Point( 8, 1 ), new Point( 8, 8 ), new Point( 1, 8 )];
	private readonly Polygon _smallPolygon;

	public IntegerRasterizerBenchmarks() {
		_rasterizer = new IntegerRasterizer();
		_buffer = new uint[Size * Size];
		_smallPolygon = new Polygon( _smallVertices );

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

	[GlobalSetup]
	public void Setup() {
		ValidateSmallPolygon( false );
		ValidateSmallPolygon( true );
	}

	private void ValidateSmallPolygon( bool validated ) {
		Array.Clear( _buffer );
		_ = validated ? FillSmallValidated_PerSpan() : FillSmallRaw_PerSpan();
		for( int y = 0; y < Size; y++ ) {
			for( int x = 0; x < Size; x++ ) {
				uint expected = x >= 1 && x <= 8 && y >= 1 && y <= 8 ? uint.MaxValue : 0;
				if( _buffer[(y * Size) + x] != expected ) {
					throw new InvalidOperationException( "Rasterizer benchmark differs from the inclusive square reference." );
				}
			}
		}
	}

	[Benchmark]
	public uint FillSmallRaw_PerSpan() {
		_rasterizer.Rasterize( _smallVertices.AsSpan(), new SpanWriter( _buffer, uint.MaxValue ) );
		return _buffer[(4 * Size) + 4];
	}

	[Benchmark]
	public uint FillSmallValidated_PerSpan() {
		_rasterizer.Rasterize( _smallPolygon, new SpanWriter( _buffer, uint.MaxValue ) );
		return _buffer[(4 * Size) + 4];
	}

	/// <summary>
	/// Uses the shared per-pixel span emitter.
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

		public void PixelSpan( int xMin, int xMax, int y ) =>
			IPixelOperation.EmitSpan( this, xMin, xMax, y );
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
