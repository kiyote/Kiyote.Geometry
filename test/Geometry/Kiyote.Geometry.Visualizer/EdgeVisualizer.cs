using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kiyote.Geometry.Visualizer;

public sealed class EdgeVisualizer {

	private readonly string _outputFolder;
	private readonly Bounds _bounds;
	private readonly IRandom _random;

	public EdgeVisualizer(
		string outputFolder,
		Bounds bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_random = new FastRandom();
	}

	public void Visualize() {
		VisualizeTryFindIntersection();
	}

	private void VisualizeTryFindIntersection() {
		Console.WriteLine( "Edge.TryFindIntersection" );
		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		bool intersection = false;
		int circuitBreaker = 0;
		do {
			int x1 = _random.NextInt( 0, _bounds.Width );
			int y1 = _random.NextInt( 0, _bounds.Height );
			int x2 = _random.NextInt( 0, _bounds.Width );
			int y2 = _random.NextInt( 0, _bounds.Height );
			IEdge e1 = new Edge( new Point( x1, y1 ), new Point( x2, y2 ) );

			int x3 = _random.NextInt( 0, _bounds.Width );
			int y3 = _random.NextInt( 0, _bounds.Height );
			int x4 = _random.NextInt( 0, _bounds.Width );
			int y4 = _random.NextInt( 0, _bounds.Height );
			IEdge e2 = new Edge( new Point( x3, y3 ), new Point( x4, y4 ) );

			if( e1.TryFindIntersection( e2, out Point p ) ) {
				image.Mutate( ( context ) => {
					PointF[] lines = new PointF[2];
					lines[0].X = x1;
					lines[0].Y = y1;
					lines[1].X = x2;
					lines[1].Y = y2;
					context.DrawLines( Color.DarkGray, 1.0f, lines );

					lines[0].X = x3;
					lines[0].Y = y3;
					lines[1].X = x4;
					lines[1].Y = y4;
					context.DrawLines( Color.DarkGray, 1.0f, lines );
				} );

				intersection = true;
				image[p.X, p.Y] = Color.Red;
			}
			circuitBreaker++;
			if( circuitBreaker > 100 ) {
				intersection = true;
			}

		} while( !intersection );

		image.SaveAsPng( Path.Combine( _outputFolder, "EdgeIntersect.png" ) );
	}
}

