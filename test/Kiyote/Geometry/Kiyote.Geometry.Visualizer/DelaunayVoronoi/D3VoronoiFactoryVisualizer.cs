using Kiyote.Buffers;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Imaging;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi;

public sealed class D3VoronoiFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly ISize _size;
	private readonly IVoronoiFactory _voronoiFactory;
	private readonly IRasterizer _rasterizer;
	private readonly IBufferFactory _bufferFactory;

	public D3VoronoiFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_size = size;

		_voronoiFactory = new D3VoronoiFactory();
		_rasterizer = new IntegerRasterizer();
		_bufferFactory = IBufferFactory.CreateArrayFactory();
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
		List<Point> points = [
			new Point( width / 4, height / 4 ),
			new Point( width / 4 * 3, height / 4 ),
			new Point( width / 4, height / 4 * 3 ),
			new Point( width / 4 * 3, height / 4 * 3 )
		];

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = _bufferFactory.Create( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Square.png" ), buffer );
	}

	private void VisualizeGrid() {
		int width = _size.Width;
		int height = _size.Height;
		int widthSlice = width / 3;
		int widthOffset = widthSlice / 2;
		int heightSlice = height / 3;
		int heightOffset = heightSlice / 2;
		List<Point> points = [
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
		];

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = _bufferFactory.Create( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Grid.png" ), buffer );
	}

	private void VisualizeRandom() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = _bufferFactory.Create( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Random.png" ), buffer );
	}

	private void VisualizeNeighbours() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = _bufferFactory.Create( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		Cell cell = voronoi.Cells[0];
		foreach( Cell neighbour in voronoi.Neighbours[cell] ) {

			_rasterizer.Rasterize( neighbour.Polygon.Points, ( int x, int y ) => {
				buffer[x, y] = 0xFF0000FFU;
			}, false );
		}

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Neighbours.png" ), buffer );
	}

	private void VisualizeOpen() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		IReadOnlyList<Point> points = pointFactory.Fill( _size, 25, false );

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = _bufferFactory.Create( _size.Width, _size.Height, 0x000000FFU );

		foreach (Cell cell in voronoi.Cells) {
			_rasterizer.Rasterize( cell.Polygon.Points, ( int x, int y ) => {
				buffer[x, y] = cell.IsOpen ? 0xFF0000FFU : 0xA9A9A9FFU;

			}, false );
		}

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Open.png" ), buffer );
	}

	private void Render(
		IBuffer<uint> buffer,
		IVoronoi voronoi
	) {
		foreach (Cell cell in voronoi.Cells) {
			_rasterizer.Rasterize( cell.Polygon.Points, ( int x, int y ) => {
				buffer[x, y] = 0xA9A9A9FFU;
			}, false );
		}

		// Render the coords
		for( int i = 0; i < voronoi.Cells.Count; i++ ) {
			buffer[voronoi.Cells[i].Center.X, voronoi.Cells[i].Center.Y] = 0xFF00FFFFU;
		}
	}
	
}
