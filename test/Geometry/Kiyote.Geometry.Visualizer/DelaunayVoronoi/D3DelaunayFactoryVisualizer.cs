using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi;

public sealed class D3DelaunayFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly Bounds _bounds;
	private readonly IDelaunayFactory _delaunayFactory;

	public D3DelaunayFactoryVisualizer(
		string outputFolder,
		Bounds bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_delaunayFactory = new D3DelaunayFactory();
	}

	public void Visualize() {
		Console.WriteLine( "D3DelaunayFactory.Create" );
		VisualizeSquare();
		VisualizeRandom();
	}

	private void VisualizeSquare() {
		int width = _bounds.Width;
		int height = _bounds.Height;

		List<Point> points = new List<Point>( 4 ) {
			new Point( width / 4, height / 4 ),
			new Point( width / 4 * 3, height / 4 ),
			new Point( width / 4, height / 4 * 3 ),
			new Point( width / 4 * 3, height / 4 * 3 )
		};
		IDelaunay delaunay = _delaunayFactory.Create( points );

		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		Render( image, delaunay );

		image.SaveAsPng( Path.Combine( _outputFolder, "D3DelaunayFactory_Square.png" ) );
	}

	private void VisualizeRandom() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );

		IReadOnlyList<Point> points = pointFactory.Fill( new Bounds( _bounds.Width, _bounds.Height ), 25 );
		IDelaunay delaunay = _delaunayFactory.Create( points );

		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		Render( image, delaunay );

		image.SaveAsPng( Path.Combine( _outputFolder, "D3DelaunayFactory_Random.png" ) );
	}

	private static void Render(
		Image<Rgb24> image,
		IDelaunay delaunay
	) {
		// Draw the triangles
		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[4];
			for( int i = 0; i < delaunay.Triangles.Count; i++ ) {
				lines[0].X = delaunay.Triangles[i].P1.X;
				lines[0].Y = delaunay.Triangles[i].P1.Y;
				lines[1].X = delaunay.Triangles[i].P2.X;
				lines[1].Y = delaunay.Triangles[i].P2.Y;
				lines[2].X = delaunay.Triangles[i].P3.X;
				lines[2].Y = delaunay.Triangles[i].P3.Y;
				lines[3].X = delaunay.Triangles[i].P1.X;
				lines[3].Y = delaunay.Triangles[i].P1.Y;
				_ = context.DrawLines( Color.DarkGray, 1.0f, lines );
			}
		} );

		// Draw the hull
		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[delaunay.Hull.Count + 1];
			for( int i = 0; i < delaunay.Hull.Count; i++ ) {
				lines[i].X = delaunay.Hull[i].X;
				lines[i].Y = delaunay.Hull[i].Y;
			}
			lines[^1].X = delaunay.Hull[0].X;
			lines[^1].Y = delaunay.Hull[0].Y;
			_ = context.DrawLines( Color.Yellow, 1.0f, lines );
		} );

		// Draw the points
		for( int i = 0; i < delaunay.Points.Count; i++ ) {
			image[delaunay.Points[i].X, delaunay.Points[i].Y] = Color.Magenta;
		}
	}
}
