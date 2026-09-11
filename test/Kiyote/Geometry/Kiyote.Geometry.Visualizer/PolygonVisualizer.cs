using System.IO.Abstractions;
using Kiyote.Buffers;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Imaging;
using Kiyote.Imaging.Png;

namespace Kiyote.Geometry.Visualizer;

public sealed class PolygonVisualizer {

	private readonly ISize _bounds;
	private readonly string _outputFolder;
	private readonly IRandom _random;

	private readonly IRasterizer _rasterizer;

	private readonly IFileSystem _fileSystem;

	public PolygonVisualizer(
		string outputFolder,
		ISize bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_random = new FastRandom();
		_rasterizer = new IntegerRasterizer();
		_fileSystem = new FileSystem();
	}

	public void Visualize() {
		VisualizeClip();
		VisualizeIntersections();
		VisualizeContains();
	}

	private void VisualizeClip() {
		Console.WriteLine( "Polygon.Clip" );
		IBuffer<uint> buffer = new ArrayBuffer<uint>( _bounds.Width, _bounds.Height, 0x000000FFU );

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

		_rasterizer.Rasterize( polygon1, new BufferPixelWriter( buffer, 0xFFFF00FFU ), false );

		_rasterizer.Rasterize( polygon2, new BufferPixelWriter( buffer, 0xFFA500FFU ), false );

		polygon1.TryIntersect( polygon2, out Polygon polygon3 );
		_rasterizer.Rasterize( polygon3, new BufferPixelWriter( buffer, 0xFFFFFFFFU ), false );

		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "PolygonClip.png" ), buffer );
	}

	private void VisualizeIntersections() {
		Console.WriteLine( "Polygon.Intersections" );
		IBuffer<uint> buffer = new ArrayBuffer<uint>( _bounds.Width, _bounds.Height, 0x000000FFU );

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


		_rasterizer.Rasterize( polygon1, new BufferPixelWriter( buffer, 0xFFFF00FFU ), false );

		_rasterizer.Rasterize( polygon2, new BufferPixelWriter( buffer, 0xFFFFFFFFU ), false );


		if (polygon1.TryFindIntersections( polygon2, out IReadOnlyList<Point> intersections)) {
			foreach( Point p in intersections ) {
				buffer[p.X, p.Y] = 0xFF0000FFU; ;
			}
		}

		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "PolygonIntersections.png" ), buffer );
	}

	private void VisualizeContains() {
		Console.WriteLine( "Polygon.Contains" );
		IBuffer<uint> buffer = new ArrayBuffer<uint>( _bounds.Width, _bounds.Height, 0x000000FFU );

		Polygon polygon = new Polygon( [
			new Point( 200, 200 ),
			new Point( _bounds.Width - 200, 200 ),
			new Point( _bounds.Width - 200, _bounds.Height - 200 ),
			new Point( 200, _bounds.Height - 200 )
		] );

		_rasterizer.Rasterize( polygon, new BufferPixelWriter( buffer, 0xFFFF00FFU ), false );

		for( int i = 0; i < 5000; i++ ) {
			int x = _random.NextInt( _bounds.Width );
			int y = _random.NextInt( _bounds.Height );
			Point p = new Point( x, y );

			buffer[x, y] = polygon.Contains( p ) ? 0x00FF00FFU : 0xFF0000FFU;
		}

		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "PolygonContains.png" ), buffer );
	}
}
