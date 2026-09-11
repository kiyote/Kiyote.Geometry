using System.IO.Abstractions;
using Kiyote.Geometry.Randomization;
using Kiyote.Buffers;
using Kiyote.Imaging;
using Kiyote.Imaging.Png;
using Kiyote.Geometry.Rasterizers;

namespace Kiyote.Geometry.Visualizer.Randomization;

public sealed class FastRandomVisualizer {

	private readonly string _outputFolder;
	private readonly ISize _bounds;
	private readonly IRandom _random;
	private readonly int _count;
	private readonly IRasterizer _rasterizer;
	private readonly IFileSystem _fileSystem;

	public FastRandomVisualizer(
		string outputFolder,
		ISize bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_random = new FastRandom();
		_count = bounds.Width * bounds.Height / 100;
		_rasterizer = new IntegerRasterizer();
		_fileSystem = new FileSystem();
	}

	public void Visualize() {
		NextInt();
		NextIntUpperBound();
		NextIntLowerBoundUpperBound();
		NextUInt();
		NextDouble();
		NextFloat();
		NextBytes();
		NextFloatLowerBoundUpperBound();
	}

	private void NextInt() {
		Console.WriteLine( "FastRandom.NextInt" );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			int x = (int)( _random.NextInt() / (float)int.MaxValue * ( _bounds.Width - 1 ) );
			int y = (int)( _random.NextInt() / (float)int.MaxValue * ( _bounds.Height - 1 ) );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextInt.png" ), buffer );
	}

	private void NextIntUpperBound() {
		Console.WriteLine( "FastRandom.NextIntUpperBound" );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			int x = _random.NextInt( _bounds.Width / 2 );
			int y = _random.NextInt( _bounds.Height / 2 );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter( _fileSystem );
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextIntUpperBound.png" ), buffer );
	}

	private void NextIntLowerBoundUpperBound() {
		Console.WriteLine( "FastRandom.NextIntLowerBoundUpperBound" );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			int x = _random.NextInt( _bounds.Width / 4, _bounds.Width / 4 * 3 );
			int y = _random.NextInt( _bounds.Height / 4, _bounds.Height / 4 * 3 );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter(_fileSystem);
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextIntLowerBoundUpperBound.png" ), buffer );
	}

	private void NextUInt() {
		Console.WriteLine( "FastRandom.NextUInt" );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			int x = (int)( _random.NextUInt() / (float)uint.MaxValue * ( _bounds.Width - 1 ) );
			int y = (int)( _random.NextUInt() / (float)uint.MaxValue * ( _bounds.Height - 1 ) );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter(_fileSystem);
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextUInt.png" ), buffer );
	}

	private void NextDouble() {
		Console.WriteLine( "FastRandom.NextDouble" );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			double dx = _random.NextDouble();
			double dy = _random.NextDouble();
			int x = (int)( dx * ( _bounds.Width - 1 ) );
			int y = (int)( dy * ( _bounds.Height - 1 ) );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter(_fileSystem);
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextDouble.png" ), buffer );
	}

	private void NextFloat() {
		Console.WriteLine( "FastRandom.NextFloat" );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			double dx = _random.NextFloat();
			double dy = _random.NextFloat();
			int x = (int)( dx * ( _bounds.Width - 1 ) );
			int y = (int)( dy * ( _bounds.Height - 1 ) );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter(_fileSystem);
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextFloat.png" ), buffer );
	}

	private void NextFloatLowerBoundUpperBound() {
		Console.WriteLine( "FastRandom.nextFloatLowerBoundUpperBound" );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			double dx = _random.NextFloat( 0.25f, 0.75f );
			double dy = _random.NextFloat( 0.25f, 0.75f );
			int x = (int)( dx * ( _bounds.Width - 1 ) );
			int y = (int)( dy * ( _bounds.Height - 1 ) );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter(_fileSystem);
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextFloatLowerBoundUpperBound.png" ), buffer );
	}

	private void NextBytes() {
		Console.WriteLine( "FastRandom.NextBytes" );
		byte[] bytes = new byte[_count * 2];
		_random.NextBytes( bytes );
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _bounds.Width, _bounds.Height, 0x00 );
		for( int i = 0; i < _count; i++ ) {
			int x = (int)( bytes[i] / (float)byte.MaxValue * ( _bounds.Width - 1 ) );
			int y = (int)( bytes[i + 1] / (float)byte.MaxValue * ( _bounds.Height - 1 ) );
			buffer[x, y] = 0xFF;
		}
		IImageWriter writer = new PngWriter(_fileSystem);
		writer.WriteImage( Path.Combine( _outputFolder, "FastRandom_NextBytes.png" ), buffer );
	}
}
