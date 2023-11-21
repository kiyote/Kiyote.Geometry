using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi;

public sealed class D3VoronoiFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly ISize _size;
	private readonly IVoronoiFactory _voronoiFactory;

	public D3VoronoiFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_size = size;

		_voronoiFactory = new D3VoronoiFactory();
	}

	public void Visualize() {
		Console.WriteLine( "D3VoronoiFactory.Create" );
		VisualizeSquare();
		VisualizeGrid();
		VisualizeRandom();
		VisualizeNeighbours();
		VisualizeOpen();
	}

	private void VisualizeSquare() {
		int width = _size.Width;
		int height = _size.Height;
		List<Point> points = new List<Point>(4) {
			new Point( width / 4, height / 4 ),
			new Point( width / 4 * 3, height / 4 ),
			new Point( width / 4, height / 4 * 3 ),
			new Point( width / 4 * 3, height / 4 * 3 )
		};

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		using Image<Rgb24> image = new Image<Rgb24>( _size.Width, _size.Height );

		Render( image, voronoi );

		image.SaveAsPng( Path.Combine( _outputFolder, "D3VoronoiFactory_Square.png" ) );
	}

	private void VisualizeGrid() {
		int width = _size.Width;
		int height = _size.Height;
		int widthSlice = width / 3;
		int widthOffset = widthSlice / 2;
		int heightSlice = height / 3;
		int heightOffset = heightSlice / 2;
		List<Point> points = new List<Point>( 9 ) {
			new Point(
				(widthSlice * 0) + widthOffset,
				(heightSlice * 0) + heightOffset
			),
			new Point(
				(widthSlice * 1) + widthOffset,
				(heightSlice * 0) + heightOffset
			),
			new Point(
				(widthSlice * 2) + widthOffset,
				(heightSlice * 0) + heightOffset
			),
			new Point(
				(widthSlice * 0) + widthOffset,
				(heightSlice * 1) + heightOffset
			),
			new Point(
				(widthSlice * 1) + widthOffset,
				(heightSlice * 1) + heightOffset
			),
			new Point(
				(widthSlice * 2) + widthOffset,
				(heightSlice * 1) + heightOffset
			),
			new Point(
				(widthSlice * 0) + widthOffset,
				(heightSlice * 2) + heightOffset
			),
			new Point(
				(widthSlice * 1) + widthOffset,
				(heightSlice * 2) + heightOffset
			),
			new Point(
				(widthSlice * 2) + widthOffset,
				(heightSlice * 2) + heightOffset
			)
		};

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		using Image<Rgb24> image = new Image<Rgb24>( _size.Width, _size.Height );

		Render( image, voronoi );

		image.SaveAsPng( Path.Combine( _outputFolder, "D3VoronoiFactory_Grid.png" ) );
	}

	private void VisualizeRandom() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		using Image<Rgb24> image = new Image<Rgb24>( _size.Width, _size.Height );

		Render( image, voronoi );

		image.SaveAsPng( Path.Combine( _outputFolder, "D3VoronoiFactory_Random.png" ) );
	}

	private void VisualizeNeighbours() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		using Image<Rgb24> image = new Image<Rgb24>( _size.Width, _size.Height );

		Render( image, voronoi );

		Cell cell = voronoi.Cells[0];
		foreach (Cell neighbour in voronoi.Neighbours[cell]) {
			image.Mutate( ( context ) => {
				PointF[] lines = new PointF[neighbour.Polygon.Points.Count + 1];
				int index = 0;
				foreach( Point point in neighbour.Polygon.Points ) {
					lines[index].X = point.X;
					lines[index].Y = point.Y;
					index++;
				}
				lines[index].X = neighbour.Polygon.Points[0].X;
				lines[index].Y = neighbour.Polygon.Points[0].Y;
				_ = context.DrawLine( Color.Red, 1.0f, lines );
			} );
		}

		image.SaveAsPng( Path.Combine( _outputFolder, "D3VoronoiFactory_Neighbours.png" ) );
	}

	private void VisualizeOpen() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		using Image<Rgb24> image = new Image<Rgb24>( _size.Width, _size.Height );

		//Render( image, voronoi );

		image.Mutate( ( context ) => {
			foreach( Cell cell in voronoi.Cells ) {
				PointF[] lines = new PointF[cell.Polygon.Points.Count + 1];
				int index = 0;
				foreach( Point point in cell.Polygon.Points ) {
					lines[index].X = point.X;
					lines[index].Y = point.Y;
					index++;
				}
				lines[index].X = cell.Polygon.Points[0].X;
				lines[index].Y = cell.Polygon.Points[0].Y;
				_ = context.DrawLine( cell.IsOpen ? Color.Red : Color.DarkGray, 1.0f, lines );
			}
		} );

		image.SaveAsPng( Path.Combine( _outputFolder, "D3VoronoiFactory_Open.png" ) );
	}

	private static void Render(
		Image<Rgb24> image,
		IVoronoi voronoi
	) {
		image.Mutate( ( context ) => {
			foreach( Cell cell in voronoi.Cells ) {
				PointF[] lines = new PointF[cell.Polygon.Points.Count + 1];
				int index = 0;
				foreach( Point point in cell.Polygon.Points ) {
					lines[index].X = point.X;
					lines[index].Y = point.Y;
					index++;
				}
				lines[index].X = cell.Polygon.Points[0].X;
				lines[index].Y = cell.Polygon.Points[0].Y;
				_ = context.DrawLine( Color.DarkGray, 1.0f, lines );
			}
		} );

		// Render the coords
		for( int i = 0; i < voronoi.Cells.Count; i++ ) {
			image[voronoi.Cells[i].Center.X, voronoi.Cells[i].Center.Y] = Color.Magenta;
		}
	}
}
