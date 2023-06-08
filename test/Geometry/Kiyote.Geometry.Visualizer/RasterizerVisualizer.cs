namespace Kiyote.Geometry.Visualizer;

public sealed class RasterizerVisualizer {

	private readonly ISize _bounds;
	private readonly string _outputFolder;
	private readonly IRasterizer _rasterizer;

	public RasterizerVisualizer(
		string outputFolder,
		ISize bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_rasterizer = new Rasterizer();
	}

	public void Visualize() {
		//VisualizeAction();
		VisualizeTest();
	}

	private void VisualizeTest() {
		using Image<Rgb24> image = new Image<Rgb24>( 10, 10 );

		List<Point> points = new List<Point>() {
			new Point( 0, 3 ),
			new Point( 8, 0 ),
			new Point( 9, 0 ),
			new Point( 9, 9 ),
			new Point( 0, 9 ),
		};

		_rasterizer.Rasterize( points, ( int x, int y ) => {
			image[x, y] = Color.DimGray;
		} );

		image.SaveAsPng( Path.Combine( _outputFolder, "SlopeTest.png" ) );

		Point a;
		Point b;
		for( int i = 0; i < points.Count - 1; i++ ) {
			a = points[i];
			b = points[i + 1];
			_rasterizer.Rasterize( a, b, ( int x, int y ) => {
				image[x, y] = Color.White;
			} );
		}
		a = points[^1];
		b = points[0];
		_rasterizer.Rasterize( a, b, ( int x, int y ) => {
			image[x, y] = Color.White;
		} );

		image.SaveAsPng( Path.Combine( _outputFolder, "SlopeTest2.png" ) );
	}

	private void VisualizeAction() {
		Console.WriteLine( "Rasterizer.Action" );
		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		List<Point> points = new List<Point>() {
			new Point( 100, 200 ),
			new Point( 400, 100 ),
			new Point( _bounds.Width - 200, 100 ),
			new Point( _bounds.Width - 100, 400 ),
			new Point( _bounds.Width - 100, _bounds.Height - 400 ),
			new Point( _bounds.Width - 200, _bounds.Height - 100 ),
			new Point( 400, _bounds.Height - 100 ),
			new Point( 100, _bounds.Height - 200 )
		};

		_rasterizer.Rasterize( points, ( int x, int y ) => {
			image[x, y] = Color.DimGray;
		} );

		Point a;
		Point b;
		for( int i = 0; i < points.Count-1; i++) {
			a = points[i];
			b = points[i + 1];
			_rasterizer.Rasterize( a, b, ( int x, int y ) => {
				image[x, y] = Color.White;
			} );
		}
		a = points[^1];
		b = points[0];
		_rasterizer.Rasterize( a, b, ( int x, int y ) => {
			image[x, y] = Color.White;
		} );

		image.SaveAsPng( Path.Combine( _outputFolder, "RasterizerAction.png" ) );
	}
}
