using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;

namespace Kiyote.Geometry.Visualizer.Rasterizers;

public sealed class IntegerRasterizerVisualizer {

	private readonly string _outputFolder;
	private readonly IRasterizer _rasterizer;
	private readonly ISize _size;

	public IntegerRasterizerVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_rasterizer = new IntegerRasterizer();
		_size = size;
	}

	public void Visualize() {
		VisualizeRotation();
		VisualizeVoronoiEdges();
	}

	public void VisualizeRotation() {

		const int size = 10;

		List<Point> points = [
			new Point( 0, 0 ),
			new Point( size - 1, 0 ),
			new Point( size - 1, size - 1 ),
			new Point( 0, size - 1 ),
		];

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

			points = [
				new Point( points[0].X + 1, points[0].Y ),
				new Point( points[1].X, points[1].Y + 1 ),
				new Point( points[2].X - 1, points[2].Y ),
				new Point( points[3].X, points[3].Y - 1 ),
			];
		}
	}

	public void VisualizeVoronoiEdges() {
		using Image<Rgb24> image = new Image<Rgb24>( _size.Width, _size.Height );

		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );
		IVoronoiFactory voronoiFactory = new D3VoronoiFactory();
		IVoronoi voronoi = voronoiFactory.Create( new Rect( 0, 0, _size ), points );

		foreach( Cell cell in voronoi.Cells ) {
			byte value = (byte)random.NextInt( 255 );
			Rgb24 color = new Rgb24( value, value, value );
			_rasterizer.Rasterize( cell.Polygon.Points, ( int x, int y ) => {
				image[x, y] = color;
			} );
		}

		foreach( Edge edge in voronoi.Edges ) {
			_rasterizer.Rasterize( edge.A, edge.B, ( int x, int y ) => {
				image[x, y] = Color.DarkGray;
			} );
		}

		foreach( Cell cell in voronoi.Cells ) {
			foreach( Point p in cell.Polygon.Points ) {
				image[p.X, p.Y] = Color.Black;
			}
		}

		image.SaveAsPng( Path.Combine( _outputFolder, "IntegerRasterizer_Voronoi.png" ) );
	}
}
