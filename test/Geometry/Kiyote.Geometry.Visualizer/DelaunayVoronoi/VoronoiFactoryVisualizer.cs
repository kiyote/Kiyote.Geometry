using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi;

public sealed class VoronoiFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly Bounds _bounds;
	private readonly IVoronoiFactory _voronoiFactory;
	private readonly IReadOnlyList<IPoint> _points;
	private readonly Delaunator _delaunator;

	public VoronoiFactoryVisualizer(
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
		_voronoiFactory = new VoronoiFactory();
	}

	public void Visualize() {
		Console.WriteLine( "VoronoiFactory.Create" );
		IVoronoi voronoi = _voronoiFactory.Create( _delaunator, _bounds.Width - 200, _bounds.Height - 200 );

		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		image.Mutate( ( context ) => {
			foreach (Cell cell in voronoi.Cells) {
				PointF[] lines = new PointF[cell.Polygon.Points.Count+1];
				int index = 0;
				foreach( Point point in cell.Polygon.Points ) {
					lines[index].X = point.X + 100;
					lines[index].Y = point.Y + 100;
					index++;
				}
				lines[index].X = cell.Polygon.Points[0].X + 100;
				lines[index].Y = cell.Polygon.Points[0].Y + 100;
				context.DrawLines( Color.DarkGray, 1.0f, lines );
			}
		} );

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[5];
			lines[0].X = 100;
			lines[0].Y = 100;
			lines[1].X = _bounds.Width - 100;
			lines[1].Y = 100;
			lines[2].X = _bounds.Width - 100;
			lines[2].Y = _bounds.Height - 100;
			lines[3].X = 100;
			lines[3].Y = _bounds.Height - 100;
			lines[4].X = 100;
			lines[4].Y = 100;
			context.DrawLines( Color.Yellow, 1.0f, lines );
		} );

		foreach( Cell cell in voronoi.OpenCells) {
			image[cell.Center.X + 100, cell.Center.Y + 100] = Color.Red;
		}

		foreach( Cell cell in voronoi.ClosedCells ) {
			image[cell.Center.X + 100, cell.Center.Y + 100] = Color.Green;
		}

		image.SaveAsPng( Path.Combine( _outputFolder, "VoronoiFactory.png" ) );
	}
}
