using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kiyote.Geometry.Visualizer.Randomization;
public sealed class FastRandomVisualizer {

	private readonly string _outputFolder;
	private readonly Bounds _bounds;
	private readonly IRandom _random;
	private readonly int _count;

	public FastRandomVisualizer(
		string outputFolder,
		Bounds bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_random = new FastRandom();
		_count = bounds.Width * bounds.Height / 100;
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
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			int x = (int)( _random.NextInt() / (float)int.MaxValue * ( _bounds.Width - 1 ) );
			int y = (int)( _random.NextInt() / (float)int.MaxValue * ( _bounds.Height - 1 ) );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextInt.png" ) );
	}

	private void NextIntUpperBound() {
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			int x = _random.NextInt( _bounds.Width / 2 );
			int y = _random.NextInt( _bounds.Height / 2 );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextIntUpperBound.png" ) );
	}

	private void NextIntLowerBoundUpperBound() {
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			int x = _random.NextInt( _bounds.Width / 4, _bounds.Width / 4 * 3 );
			int y = _random.NextInt( _bounds.Height / 4, _bounds.Height / 4 * 3 );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextIntLowerBoundUpperBound.png" ) );
	}

	private void NextUInt() {
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			int x = (int)( _random.NextUInt() / (float)uint.MaxValue * ( _bounds.Width - 1 ) );
			int y = (int)( _random.NextUInt() / (float)uint.MaxValue * ( _bounds.Height - 1 ) );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextUInt.png" ) );
	}

	private void NextDouble() {
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			double dx = _random.NextDouble();
			double dy = _random.NextDouble();
			int x = (int)( dx * ( _bounds.Width - 1 ) );
			int y = (int)( dy * ( _bounds.Height - 1 ) );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextDouble.png" ) );
	}

	private void NextFloat() {
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			double dx = _random.NextFloat();
			double dy = _random.NextFloat();
			int x = (int)( dx * ( _bounds.Width - 1 ) );
			int y = (int)( dy * ( _bounds.Height - 1 ) );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextFloat.png" ) );
	}

	private void NextFloatLowerBoundUpperBound() {
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			double dx = _random.NextFloat( 0.25f, 0.75f );
			double dy = _random.NextFloat( 0.25f, 0.75f );
			int x = (int)( dx * ( _bounds.Width - 1 ) );
			int y = (int)( dy * ( _bounds.Height - 1 ) );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextFloatLowerBoundUpperBound.png" ) );
	}

	private void NextBytes() {
		byte[] bytes = new byte[_count * 2];
		_random.NextBytes( bytes );
		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		for( int i = 0; i < _count; i++ ) {
			int x = (int)( bytes[i] / (float)byte.MaxValue * ( _bounds.Width - 1 ) );
			int y = (int)( bytes[i + 1] / (float)byte.MaxValue * ( _bounds.Height - 1 ) );
			image[x, y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastRandom_NextBytes.png" ) );
	}
}
