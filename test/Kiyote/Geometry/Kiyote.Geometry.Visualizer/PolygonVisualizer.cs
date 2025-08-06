using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kiyote.Geometry.Visualizer;

public sealed class PolygonVisualizer {

	private readonly ISize _bounds;
	private readonly string _outputFolder;
	private readonly IRandom _random;

	public PolygonVisualizer(
		string outputFolder,
		ISize bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_random = new FastRandom();
	}

	public void Visualize() {
		VisualizeContains();
		VisualizeIntersections();
		VisualizeClip();
	}

	private void VisualizeClip() {
		Console.WriteLine( "Polygon.Clip" );
		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

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

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[polygon1.Points.Count + 1];
			for( int i = 0; i < polygon1.Points.Count + 1; i++ ) {
				lines[i].X = polygon1.Points[i % polygon1.Points.Count].X;
				lines[i].Y = polygon1.Points[i % polygon1.Points.Count].Y;
			}

			 context.DrawLine( Color.Yellow, 1.0f, lines );
		} );

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[polygon2.Points.Count + 1];
			for( int i = 0; i < polygon2.Points.Count + 1; i++ ) {
				lines[i].X = polygon2.Points[i % polygon2.Points.Count].X;
				lines[i].Y = polygon2.Points[i % polygon2.Points.Count].Y;
			}

			 context.DrawLine( Color.Orange, 1.0f, lines );
		} );

		 polygon1.Intersect( polygon2, out Polygon polygon3 );
		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[polygon3.Points.Count + 1];
			for( int i = 0; i < polygon3.Points.Count + 1; i++ ) {
				lines[i].X = polygon3.Points[i % polygon3.Points.Count].X;
				lines[i].Y = polygon3.Points[i % polygon3.Points.Count].Y;
			}

			 context.DrawLine( Color.White, 2.0f, lines );
		} );

		image.SaveAsPng( Path.Combine( _outputFolder, "PolygonClip.png" ) );
	}

	private void VisualizeIntersections() {
		Console.WriteLine( "Polygon.Intersections" );
		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

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

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[polygon1.Points.Count + 1];
			for( int i = 0; i < polygon1.Points.Count + 1; i++ ) {
				lines[i].X = polygon1.Points[i % polygon1.Points.Count].X;
				lines[i].Y = polygon1.Points[i % polygon1.Points.Count].Y;
			}

			 context.DrawLine( Color.Yellow, 1.0f, lines );
		} );

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[polygon2.Points.Count + 1];
			for( int i = 0; i < polygon2.Points.Count + 1; i++ ) {
				lines[i].X = polygon2.Points[i % polygon2.Points.Count].X;
				lines[i].Y = polygon2.Points[i % polygon2.Points.Count].Y;
			}

			 context.DrawLine( Color.White, 1.0f, lines );
		} );

		IReadOnlyList<Point> intersections = polygon1.Intersections( polygon2.Points );
		foreach (Point p in intersections) {
			image[p.X, p.Y] = Color.Red;
		}

		image.SaveAsPng( Path.Combine( _outputFolder, "PolygonIntersections.png" ) );
	}

	private void VisualizeContains() {
		Console.WriteLine( "Polygon.Contains" );
		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		Polygon polygon = new Polygon( [
			new Point( 200, 200 ),
			new Point( _bounds.Width - 200, 200 ),
			new Point( _bounds.Width - 200, _bounds.Height - 200 ),
			new Point( 200, _bounds.Height - 200 )
		] );

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[polygon.Points.Count + 1];
			for( int i = 0; i < polygon.Points.Count + 1; i++ ) {
				lines[i].X = polygon.Points[i % polygon.Points.Count].X;
				lines[i].Y = polygon.Points[i % polygon.Points.Count].Y;
			}

			 context.DrawLine(Color.Yellow, 1.0f, lines);
		} );

		for( int i = 0; i < 5000; i++ ) {
			int x = _random.NextInt( _bounds.Width );
			int y = _random.NextInt( _bounds.Height );
			Point p = new Point( x, y );

			image[x, y] = polygon.Contains( p ) ? Color.Green : Color.Red;
		}



		image.SaveAsPng( Path.Combine( _outputFolder, "PolygonContains.png" ) );
	}
}
