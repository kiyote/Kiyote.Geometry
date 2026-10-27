using System.IO.Abstractions;
using Kiyote.Buffers;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Imaging;
using Kiyote.Imaging.Png;

namespace Kiyote.Geometry.Visualizer.DelaunayVoronoi.D3;

public sealed class D3VoronoiFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly ISize _size;
	private readonly IVoronoiFactory _voronoiFactory;
	private readonly IRasterizer _rasterizer;

	private readonly IFileSystem _fileSystem;

	public D3VoronoiFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_size = size;

		_voronoiFactory = new D3VoronoiFactory();
		_rasterizer = new IntegerRasterizer();
		_fileSystem = new FileSystem();
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

		IBuffer<uint> buffer = new RaggedArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		IImageWriter writer = new PngWriter( _fileSystem );
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

		IBuffer<uint> buffer = new RaggedArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Grid.png" ), buffer );
	}

	private void VisualizeRandom() {
		int cellWidth = _size.Width / 20;
		int cellHeight = _size.Height / 20;

		List<Point> points = [];
		for( int c = cellWidth / 2; c < _size.Width; c += cellWidth ) {
			for( int r = cellHeight / 2; r < _size.Height; r += cellHeight ) {
				points.Add( new Point( c, r ) );
			}
		}

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = new RaggedArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Random.png" ), buffer );
	}

	private void VisualizeNeighbours() {
		int cellWidth = _size.Width / 20;
		int cellHeight = _size.Height / 20;

		List<Point> points = [];
		for( int c = cellWidth / 2; c < _size.Width; c += cellWidth ) {
			for( int r = cellHeight / 2; r < _size.Height; r += cellHeight ) {
				points.Add( new Point( c, r ) );
			}
		}

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = new RaggedArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		Render( buffer, voronoi );

		foreach( int neighbourIndex in voronoi.GetNeighbourIndices( 0 ) ) {
			_rasterizer.Rasterize( voronoi.GetCellVertices( neighbourIndex ), new BufferPixelWriter( buffer, 0xFF0000FFU ), false );
		}

		IImageWriter writer = new PngWriter(_fileSystem);
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Neighbours.png" ), buffer );
	}

	private void VisualizeOpen() {
		int cellWidth = _size.Width / 20;
		int cellHeight = _size.Height / 20;

		List<Point> points = [];
		for( int c = cellWidth / 2; c < _size.Width; c += cellWidth ) {
			for( int r = cellHeight / 2; r < _size.Height; r += cellHeight ) {
				points.Add( new Point( c, r ) );
			}
		}

		Rect bounds = new Rect( 0, 0, _size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		IBuffer<uint> buffer = new RaggedArrayBuffer<uint>( _size.Width, _size.Height, 0x000000FFU );

		for( int i = 0; i < voronoi.Cells.Length; i++ ) {
			_rasterizer.Rasterize(
				voronoi.GetCellVertices( i ),
				new BufferPixelWriter( buffer, voronoi.Cells[i].IsOpen ? 0xFF0000FFU : 0xA9A9A9FFU ),
				false
			);
		}

		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "D3VoronoiFactory_Open.png" ), buffer );
	}

	private void Render(
		IBuffer<uint> buffer,
		IVoronoi voronoi
	) {
		for( int i = 0; i < voronoi.Cells.Length; i++ ) {
			_rasterizer.Rasterize( voronoi.GetCellVertices( i ), new BufferPixelWriter( buffer, 0xA9A9A9FFU ), false );
		}

		// Render the coords
		for( int i = 0; i < voronoi.Points.Count; i++ ) {
			buffer[voronoi.Points[i].X, voronoi.Points[i].Y] = 0xFF00FFFFU;
		}
	}
	
}
