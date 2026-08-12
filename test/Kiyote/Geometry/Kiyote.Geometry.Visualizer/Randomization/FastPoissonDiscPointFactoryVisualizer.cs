using Kiyote.Buffers;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Imaging;

namespace Kiyote.Geometry.Visualizer.Randomization;

public sealed class FastPoissonDiscPointFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly IPointFactory _pointFactory;
	private readonly ISize _size;
	private readonly IRasterizer _rasterizer;

	public FastPoissonDiscPointFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_size = size;
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
		_rasterizer = new IntegerRasterizer();
	}

	public void Visualize() {
		VisualizeFill();
		VisualizeFillHeatmap();
	}

	private void VisualizeFill() {
		Console.WriteLine( "IPointFactory.Fill" );
		IReadOnlyList<Point> points = _pointFactory.Fill( _size, 25 );

		IBuffer<bool> buffer = new ArrayBuffer<bool>( _size.Width, _size.Height, false );
		foreach( Point p in points ) {
			buffer[p.X, p.Y] = true;
		}

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "FastPoissonDiscPointFactory.png" ), buffer );
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
		IBuffer<byte> buffer = new ArrayBuffer<byte>( _size.Width, _size.Height, 0 );
		for( int r = 0; r < _size.Height; r++ ) {
			for( int c = 0; c < _size.Width; c++ ) {
				int index = c + ( r * _size.Width );
				float value = heatmap[index];

				float result = value - minValue;
				result *= scale; // Value will now be between 0..1

				buffer[c, r] = (byte)( result * 255 );
			}
		}

		IImageWriter writer = IImageWriter.CreatePng();
		writer.WriteImage( Path.Combine( _outputFolder, "FastPoissonDiscPointFactory_Heatmap.png" ), buffer );
	}
}
