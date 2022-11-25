using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kiyote.Geometry.Visualizer.Randomization;

public sealed class FastPoissonDiscPointFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly IPointFactory _pointFactory;
	private readonly Bounds _bounds;

	public FastPoissonDiscPointFactoryVisualizer(
		string outputFolder,
		Bounds bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
	}

	public void Visualize() {
		IReadOnlyList<IPoint> points = _pointFactory.Fill( _bounds, 5 );

		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( _bounds.Width, _bounds.Height );
		foreach( IPoint p in points ) {
			image[p.X, p.Y] = white;
		}
		image.SaveAsPng( Path.Combine( _outputFolder, "FastPoissonDiscPointFactory.png" ) );

	}
}
