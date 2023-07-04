using Kiyote.Geometry.Rasterizers;

namespace Kiyote.Geometry.Visualizer;

public sealed class IntegerRasterizerVisualizer {

	private readonly string _outputFolder;
	private readonly IRasterizer _rasterizer;

	public IntegerRasterizerVisualizer(
		string outputFolder
	) {
		_outputFolder = outputFolder;
		_rasterizer = new IntegerRasterizer();
	}

	public void Visualize() {
		VisualizeRotation();
	}

	public void VisualizeRotation() {

		const int size = 10;


		List<Point> points = new List<Point>() {
			new Point( 0, 0 ),
			new Point( size-1, 0 ),
			new Point( size-1, size-1 ),
			new Point( 0, size-1 ),
		};

		for( int j = 0; j < size; j++ ) {
			using Image<Rgb24> image = new Image<Rgb24>( size, size );

			bool[,] poly = new bool[size, size];
			_rasterizer.Rasterize( points, ( int x, int y ) => {
				image[x, y] = Color.DimGray;
			} );

			bool[,] line = new bool[size, size];
			for( int i = 0; i < points.Count - 1; i++ ) {
				_rasterizer.Rasterize(
					points[i],
					points[i + 1],
					( int x, int y ) => {
						image[x, y] = Color.White;
					}
				);
			}
			_rasterizer.Rasterize(
				points[^1],
				points[0],
				( int x, int y ) => {
					image[x, y] = Color.White;
				}
			);

			image.SaveAsPng( Path.Combine( _outputFolder, $"IntegerRasterizer_Rotation_{j}.png" ) );

			points = new List<Point>() {
				new Point( points[0].X + 1, points[0].Y ),
				new Point( points[1].X, points[1].Y + 1 ),
				new Point( points[2].X - 1, points[2].Y ),
				new Point( points[3].X, points[3].Y - 1 ),
			};
		}
	}

}
