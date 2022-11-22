using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kiyote.Geometry.Tests;

[TestFixture]
public sealed class PoissonDiscPointFactoryIntegrationTests {
	private IRandom _random;
	private IPointFactory _pointFactory;

	[SetUp]
	public void SetUp() {
		_random = new FastRandom();

		_pointFactory = new PoissonDiscPointFactory(
			_random
		);
	}

	[Test]
	public void Fill_ValidBounds_AreaFilled() {
		Bounds bounds = new Bounds( 100, 100 );
		IReadOnlyList<IPoint> points = _pointFactory.Fill( bounds, 5 );

		Assert.IsNotEmpty( points );
	}

	[Test]
	[Ignore("Used to visualize output for inspection.")]
	public void Visualize() {
		Bounds bounds = new Bounds( 500, 500 );
		IReadOnlyList<IPoint> points = _pointFactory.Fill( bounds, 5 );

		L8 white = new L8( 255 );
		using Image<L8> image = new Image<L8>( bounds.Width, bounds.Height );
		foreach( IPoint p in points ) {
			image[p.X, p.Y] = white;
		}
		image.SaveAsPng( Path.Combine( Path.GetTempPath(), "PoissonDiscPointFactoryTests.png" ) );

	}
}
