using System.IO.Abstractions;
using Kiyote.Buffers;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Geometry.Visualizer;
using Kiyote.Imaging;
using Kiyote.Imaging.Png;

namespace Kiyote.Geometry.Noises.Visualizer;

public sealed class MidpointDisplacementNoisyEdgeFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly ISize _bounds;
	private readonly INoisyEdgeFactory _edgeFactory;
	private readonly IRasterizer _rasterizer;

	private readonly IFileSystem _fileSystem;

	public MidpointDisplacementNoisyEdgeFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_bounds = size;

		IRandom random = new FastRandom();
		_edgeFactory = new MidpointDisplacementNoisyEdgeFactory( random );
		_rasterizer = new IntegerRasterizer();
		_fileSystem = new FileSystem();
	}

	public void Visualize() {
		VisualizeCreate();
	}

	private void VisualizeCreate() {
		Console.WriteLine( "MidpointDisplacementNoisyEdgeFactoryVisualizer.Create" );
		IBuffer<uint> buffer = new ArrayBuffer<uint>( _bounds.Width, _bounds.Height, 0x000000FFU );

		int midX = (int)( _bounds.Width * 0.5f );
		int xOffset = (int)( _bounds.Width * 0.1f );
		int midY = (int)( _bounds.Height * 0.5f );
		int yOffset = (int)( _bounds.Height * 0.25f );
		Edge toSplit = new Edge( xOffset, midY, _bounds.Width - xOffset, midY );
		Edge control = new Edge( midX, yOffset, midX, _bounds.Height - yOffset );

		NoisyEdge noisyEdge = _edgeFactory.Create( toSplit, control, 0.5f, 6 );

		_rasterizer.Rasterize( noisyEdge.Source.A, noisyEdge.Source.B, new BufferPixelWriter( buffer, 0xD3D3D3FFU ) );

		_rasterizer.Rasterize( control.A, control.B, new BufferPixelWriter( buffer, 0xA9A9A9FFU ) );

		foreach( Edge e in noisyEdge.Noise ) {
			_rasterizer.Rasterize( e.A, e.B, new BufferPixelWriter( buffer, 0xFFFF00FFU ) );

			buffer[e.A.X, e.A.Y] = 0xFF00FFFFU;
			buffer[e.B.X, e.B.Y] = 0xFF00FFFFU;
		}

		buffer[control.A.X, control.A.Y] = 0xFF0000FFU;
		buffer[control.B.X, control.B.Y] = 0xFF0000FFU;
		buffer[toSplit.A.X, toSplit.A.Y] = 0x0000FFFFU;
		buffer[toSplit.B.X, toSplit.B.Y] = 0x0000FFFFU;

		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "MidpointDisplacementNoisyEdgeFactoryVisualizerCreate.png" ), buffer );
	}
}
