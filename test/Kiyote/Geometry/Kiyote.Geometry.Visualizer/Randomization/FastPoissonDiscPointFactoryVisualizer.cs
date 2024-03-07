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
		Console.WriteLine( "IPointFactory.Fill" );
		IReadOnlyList<Point> points = _pointFactory.Fill( _size, 5 );

		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _size.Width, _size.Height );
		foreach( Point p in points ) {
			image[p.X, p.Y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastPoissonDiscPointFactory.png" ) );

	}
}
