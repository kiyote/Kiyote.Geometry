namespace Kiyote.Geometry.Randomization.Tests;

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
}
