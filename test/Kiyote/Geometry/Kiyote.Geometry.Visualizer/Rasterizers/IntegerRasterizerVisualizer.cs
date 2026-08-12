using Kiyote.Buffers;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Imaging;

namespace Kiyote.Geometry.Visualizer.Rasterizers;

public sealed class IntegerRasterizerVisualizer {

	private readonly string _outputFolder;
	private readonly IRasterizer _rasterizer;
	private readonly ISize _size;

	public IntegerRasterizerVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_rasterizer = new IntegerRasterizer();
		_size = size;
	}

	public void Visualize() {
		VisualizeLines();
		VisualizeRotation();
		VisualizeVoronoiEdges();
	}

	public void VisualizeLines() {
		IBuffer<uint> buffer = new ArrayBuffer<uint>( 50, 50, 0x000000FFU );

		Point p1 = new Point( 632, 537 );
		Point p2 = new Point( 648, 551 );

		Point n1 = p1.Subtract( p1 ).Add( 25, 25 );
		Point n2 = p2.Subtract( p1 ).Add( 25, 25 );

		_rasterizer.Rasterize( n1, n2, new BufferPixelWriter( buffer, 0xFFFFFFFFU ) );

		_rasterizer.Rasterize( n2, n1, new BufferPixelWriter( buffer, 0xFFFFFFFFU ) );

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "IntegerRasterizer_Lines.png" ), buffer );

	}

	public void VisualizeRotation() {

		const int size = 10;

		List<Point> points = [
			new Point( 0, 0 ),
			new Point( size - 1, 0 ),
			new Point( size - 1, size - 1 ),
			new Point( 0, size - 1 ),
		];

		for( int j = 0; j < size; j++ ) {
			IBuffer<uint> buffer = new ArrayBuffer<uint>( size, size, 0x000000FFU );

			_rasterizer.Rasterize( points, new BufferPixelWriter( buffer, 0x696969FFU ) );

			for( int i = 0; i < points.Count - 1; i++ ) {
				_rasterizer.Rasterize(
					points[i],
					points[i + 1],
					new BufferPixelWriter( buffer, 0xFFFFFFFFU )
				);
			}
			_rasterizer.Rasterize(
				points[^1],
				points[0],
				new BufferPixelWriter( buffer, 0xFFFFFFFFU )
			);

			IImageWriter writer = IImageWriter.CreatePng();
			writer.WriteImage( Path.Combine( _outputFolder, $"IntegerRasterizer_Rotation_{j}.png" ), buffer );

			points = [
				new Point( points[0].X + 1, points[0].Y ),
				new Point( points[1].X, points[1].Y + 1 ),
				new Point( points[2].X - 1, points[2].Y ),
				new Point( points[3].X, points[3].Y - 1 ),
			];
		}
	}

	public void VisualizeVoronoiEdges() {
		IBuffer<uint> buffer = new ArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );
		IVoronoiFactory voronoiFactory = new D3VoronoiFactory();
		IVoronoi voronoi = voronoiFactory.Create( new Rect( 0, 0, _size ), points );

		foreach( Cell cell in voronoi.Cells ) {
			byte value = (byte)random.NextInt( 255 );
			uint color = (uint)( ( value << 24 ) | ( value << 16 ) | ( value << 8 ) | 0xFF );
			_rasterizer.Rasterize( cell.Polygon, new BufferPixelWriter( buffer, color ) );
		}

		foreach( Edge edge in voronoi.Edges ) {
			_rasterizer.Rasterize( edge.A, edge.B, new BufferPixelWriter( buffer, 0x8B0000FFU ) );
		}

		foreach( Cell cell in voronoi.Cells ) {
			foreach( Point p in cell.Polygon.Points ) {
				buffer[p.X, p.Y] = 0xFF00FFFFU;
			}
			buffer[cell.Center.X, cell.Center.Y] = 0xFFD700FFU;
		}

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "IntegerRasterizer_Voronoi.png" ), buffer );
	}
}
