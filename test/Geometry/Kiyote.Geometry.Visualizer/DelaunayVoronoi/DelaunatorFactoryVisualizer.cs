using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi;

public sealed class DelaunatorFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly Bounds _bounds;
	private readonly IDelaunatorFactory _delaunatorFactory;
	private readonly IReadOnlyList<IPoint> _points;

	public DelaunatorFactoryVisualizer(
		string outputFolder,
		Bounds bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		// Reduce the area so that we can easily see the hull
		_points = pointFactory.Fill( new Bounds(bounds.Width - 200, bounds.Height - 200), 25 );

		_delaunatorFactory = new DelaunatorFactory();
	}

	public void Visualize() {
		Console.WriteLine( "Delaunator.Create" );
		Delaunator delaunator = _delaunatorFactory.Create( _points );

		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[4];
			for (int i = 0; i < delaunator.Triangles.Count; i += 3) {
				for( int t = 0; t < 3; t++) {
					int index = delaunator.Triangles[i + t];
					double x = delaunator.Coords[ index * 2];
					double y = delaunator.Coords[( index * 2 ) + 1];
					x += 100;
					y += 100;
					lines[t].X = (float)x;
					lines[t].Y = (float)y;
				}
				lines[3].X = lines[0].X;
				lines[3].Y = lines[0].Y;

				context.DrawLines( Color.DarkGray, 1.0f, lines );
			}
		} );

		// Render the Hull
		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[delaunator.Hull.Count + 1];

			for( int i = 0; i < delaunator.Hull.Count; i++ ) {
				int index = delaunator.Hull[i];
				double x = delaunator.Coords[( index * 2 )];
				double y = delaunator.Coords[( index * 2 ) + 1];
				x += 100;
				y += 100;
				lines[i].X = (float)x;
				lines[i].Y = (float)y;
			}
			lines[delaunator.Hull.Count].X = lines[0].X;
			lines[delaunator.Hull.Count].Y = lines[0].Y;

			context.DrawLines( Color.Yellow, 1.0f, lines );
		} );

		// Render the coords
		foreach( IPoint p in _points ) {
			image[p.X + 100, p.Y + 100] = Color.Magenta;
		}

		image.SaveAsPng( Path.Combine( _outputFolder, "DelaunatorFactory.png" ) );
	}
}
