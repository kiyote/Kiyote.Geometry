namespace Kiyote.Geometry.Randomization.UnitTests;

[TestFixture]
public sealed class FastPoissonDiscPointFactoryIntegrationTests {
	private IRandom _random;
	private IPointFactory _pointFactory;

	[SetUp]
	public void SetUp() {
		_random = new FastRandom();

		_pointFactory = new FastPoissonDiscPointFactory(
			_random
		);
	}

	[Test]
	public void Fill_ValidBounds_AreaFilled() {
		ISize size = new Point( 100, 100 );
		IReadOnlyList<Point> points = _pointFactory.Fill( size, 5 );

		Assert.That( points, Is.Not.Empty );
	}
}
