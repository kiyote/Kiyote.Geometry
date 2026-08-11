using Kiyote.Buffers;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Imaging;

namespace Kiyote.Geometry.Visualizer;

public sealed class PolygonVisualizer {

	private readonly ISize _bounds;
	private readonly string _outputFolder;
	private readonly IRandom _random;

	private readonly IBufferFactory _bufferFactory;

	private readonly IRasterizer _rasterizer;

	public PolygonVisualizer(
		string outputFolder,
		ISize bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_random = new FastRandom();
		_bufferFactory = IBufferFactory.CreateArrayFactory();
		_rasterizer = new IntegerRasterizer();
	}

	public void Visualize() {
		VisualizeClip();
		VisualizeIntersections();
		VisualizeContains();
	}

	private void VisualizeClip() {
		Console.WriteLine( "Polygon.Clip" );
		IBuffer<uint> buffer = _bufferFactory.Create( _bounds.Width, _bounds.Height, 0x000000FFU );

		Polygon polygon1 = new Polygon( [
			new Point( 200, 200 ),
			new Point( _bounds.Width - 200, 200 ),
			new Point( _bounds.Width - 200, _bounds.Height - 200 ),
			new Point( 200, _bounds.Height - 200 )
		] );

		Polygon polygon2 = new Polygon( [
			new Point( 250, 250 ),
			new Point( _bounds.Width - 250, 350 ),
			new Point( _bounds.Width - 350, _bounds.Height - 150 ),
			new Point( 150, _bounds.Height - 300 )
		] );

		_rasterizer.Rasterize( polygon1.Points, ( int x, int y ) => {
			buffer[x, y] = 0xFFFF00FFU;
		}, false );

		_rasterizer.Rasterize( polygon2.Points, ( int x, int y ) => {
			buffer[x, y] = 0xFFA500FFU;
		}, false );

		 polygon1.TryIntersect( polygon2, out Polygon polygon3 );
		_rasterizer.Rasterize( polygon3.Points, ( int x, int y ) => {
			buffer[x, y] = 0xFFFFFFFFU;
		}, false );

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "PolygonClip.png" ), buffer );
	}

	private void VisualizeIntersections() {
		Console.WriteLine( "Polygon.Intersections" );
		IBuffer<uint> buffer = _bufferFactory.Create( _bounds.Width, _bounds.Height, 0x000000FFU );

		Polygon polygon1 = new Polygon( [
			new Point( 200, 200 ),
			new Point( _bounds.Width - 200, 200 ),
			new Point( _bounds.Width - 200, _bounds.Height - 200 ),
			new Point( 200, _bounds.Height - 200 )
		] );

		Polygon polygon2 = new Polygon( [
			new Point( 250, 250 ),
			new Point( _bounds.Width - 250, 350 ),
			new Point( _bounds.Width - 350, _bounds.Height - 150 ),
			new Point( 150, _bounds.Height - 300 )
		] );


		_rasterizer.Rasterize( polygon1.Points, ( int x, int y ) => {
			buffer[x, y] = 0xFFFF00FFU;
		}, false );

		_rasterizer.Rasterize( polygon2.Points, ( int x, int y ) => {
			buffer[x, y] = 0xFFFFFFFFU;
		}, false );


		if (polygon1.TryFindIntersections( polygon2, out IReadOnlyList<Point> intersections)) {
			foreach( Point p in intersections ) {
				buffer[p.X, p.Y] = 0xFF0000FFU; ;
			}
		}

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "PolygonIntersections.png" ), buffer );
	}

	private void VisualizeContains() {
		Console.WriteLine( "Polygon.Contains" );
		IBuffer<uint> buffer = _bufferFactory.Create( _bounds.Width, _bounds.Height, 0x000000FFU );

		Polygon polygon = new Polygon( [
			new Point( 200, 200 ),
			new Point( _bounds.Width - 200, 200 ),
			new Point( _bounds.Width - 200, _bounds.Height - 200 ),
			new Point( 200, _bounds.Height - 200 )
		] );

		_rasterizer.Rasterize( polygon.Points, (int x, int y) => {
			buffer[x, y] = 0xFFFF00FFU;
		}, false );

		for( int i = 0; i < 5000; i++ ) {
			int x = _random.NextInt( _bounds.Width );
			int y = _random.NextInt( _bounds.Height );
			Point p = new Point( x, y );

			buffer[x, y] = polygon.Contains( p ) ? 0x00FF00FFU : 0xFF0000FFU;
		}

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "PolygonContains.png" ), buffer );
	}
}
