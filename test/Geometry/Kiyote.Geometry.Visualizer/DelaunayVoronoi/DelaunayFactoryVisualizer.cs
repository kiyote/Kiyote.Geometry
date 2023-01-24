using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi;

public sealed class DelaunayFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly Bounds _bounds;
	private readonly IDelaunayFactory _delaunayFactory;
	private readonly IReadOnlyList<Point> _points;
	private readonly Delaunator _delaunator;

	public DelaunayFactoryVisualizer(
		string outputFolder,
		Bounds bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		// Reduce the area so that we can easily see the hull
		_points = pointFactory.Fill( new Bounds( bounds.Width - 200, bounds.Height - 200 ), 25 );

		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		_delaunator = delaunatorFactory.Create( _points );
		_delaunayFactory = new DelaunayFactory();
	}

	public void Visualize() {
		Console.WriteLine( "DelaunayFactory.Create" );
		Delaunay delaunay = _delaunayFactory.Create( _delaunator );

		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[4];
			for( int i = 0; i < delaunay.Triangles.Count; i++ ) {
				lines[0].X = delaunay.Triangles[i].P1.X + 100;
				lines[0].Y = delaunay.Triangles[i].P1.Y + 100;
				lines[1].X = delaunay.Triangles[i].P2.X + 100;
				lines[1].Y = delaunay.Triangles[i].P2.Y + 100;
				lines[2].X = delaunay.Triangles[i].P3.X + 100;
				lines[2].Y = delaunay.Triangles[i].P3.Y + 100;
				lines[3].X = delaunay.Triangles[i].P1.X + 100;
				lines[3].Y = delaunay.Triangles[i].P1.Y + 100;

				context.DrawLines( Color.DarkGray, 1.0f, lines );
			}
		} );

		// Render the coords
		foreach( Point p in _points ) {
			image[p.X + 100, p.Y + 100] = Color.Magenta;
		}

		image.SaveAsPng( Path.Combine( _outputFolder, "DelaunayFactory.png" ) );
	}
}
