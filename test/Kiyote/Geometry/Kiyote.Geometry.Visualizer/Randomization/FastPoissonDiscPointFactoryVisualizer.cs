using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kiyote.Geometry.Visualizer.Randomization;

public sealed class FastPoissonDiscPointFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly IPointFactory _pointFactory;
	private readonly ISize _size;

	public FastPoissonDiscPointFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_size = size;
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
	}

	public void Visualize() {
		VisualizeFill();
		VisualizeFillHeatmap();
	}

	private void VisualizeFill() {
		Console.WriteLine( "IPointFactory.Fill" );
		IReadOnlyList<Point> points = _pointFactory.Fill( _size, 25 );

		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _size.Width, _size.Height );
		foreach( Point p in points ) {
			image[p.X, p.Y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastPoissonDiscPointFactory.png" ) );
	}

	private void VisualizeFillHeatmap() {
		Console.WriteLine( "IPointFactory.Fill_Heatmap" );
		int[] heatmap = new int[_size.Width * _size.Height];
		for( int i = 0; i < 100; i++ ) {
			IReadOnlyList<Point> points = _pointFactory.Fill( _size, 25 );
			foreach( Point p in points ) {
				heatmap[p.X + ( p.Y * _size.Width )] += 1;
			}
		}

		int minValue = int.MaxValue;
		int maxValue = int.MinValue;
		for( int r = 0; r < _size.Height; r++ ) {
			for( int c = 0; c < _size.Width; c++ ) {
				int index = c + ( r * _size.Width );
				if( heatmap[index] < minValue ) {
					minValue = heatmap[index];
				}
				if( heatmap[index] > maxValue ) {
					maxValue = heatmap[index];
				}
			}
		}
		float actualRange = Math.Abs( maxValue - minValue );
		float scale = 1.0f / actualRange;

		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _size.Width, _size.Height );
		for( int r = 0; r < _size.Height; r++ ) {
			for( int c = 0; c < _size.Width; c++ ) {
				int index = c + ( r * _size.Width );
				float value = heatmap[index];

				float result = value - minValue;
				result *= scale; // Value will now be between 0..1

				image[c, r] = new L8( (byte)( result * 255 ) );
			}
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastPoissonDiscPointFactory_Heatmap.png" ) );
	}
}
