using Kiyote.Buffers;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Imaging;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi;

public sealed class D3DelaunayFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly ISize _size;
	private readonly IDelaunayFactory _delaunayFactory;
	private readonly IRasterizer _rasterizer;

	public D3DelaunayFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_size = size;
		_delaunayFactory = new D3DelaunayFactory();
		_rasterizer = new IntegerRasterizer();
	}

	public void Visualize() {
		Console.WriteLine( "D3DelaunayFactory.Create" );
		VisualizeSquare();
		VisualizeRandom();
	}

	private void VisualizeSquare() {
		int width = _size.Width;
		int height = _size.Height;

		List<Point> points = [
			new Point( width / 4, height / 4 ),
			new Point( width / 4 * 3, height / 4 ),
			new Point( width / 4, height / 4 * 3 ),
			new Point( width / 4 * 3, height / 4 * 3 )
		];
		IDelaunay delaunay = _delaunayFactory.Create( points );

		IBuffer<uint> buffer = new ArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, delaunay );

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "D3DelaunayFactory_Square.png" ), buffer );
	}

	private void VisualizeRandom() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );

		IReadOnlyList<Point> points = pointFactory.Fill( new Point( _size.Width, _size.Height ), 25 );
		IDelaunay delaunay = _delaunayFactory.Create( points );

		IBuffer<uint> buffer = new ArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, delaunay );

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "D3DelaunayFactory_Random.png" ), buffer );
	}

	private void Render(
		IBuffer<uint> buffer,
		IDelaunay delaunay
	) {
		// Draw the triangles
		for (int i = 0; i < delaunay.Triangles.Count; i++ ) {
			_rasterizer.Rasterize( delaunay.Triangles[i], new BufferPixelWriter( buffer, 0xA9A9A9FFU ), false );
		}

		// Draw the hull
		_rasterizer.Rasterize( delaunay.Hull, new BufferPixelWriter( buffer, 0xFFFF00FFU ), false );

		// Draw the points
		for( int i = 0; i < delaunay.Points.Count; i++ ) {
			buffer[delaunay.Points[i].X, delaunay.Points[i].Y] = 0xFF00FFFFU;
		}
	}
}
