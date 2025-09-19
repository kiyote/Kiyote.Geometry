namespace Kiyote.Geometry.Randomization.Tests;

[TestFixture]
public sealed class FastPoissonDiscPointFactoryTests {
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
	public void Fill_100x100_AreaFilled() {
		IReadOnlyList<Point> points = _pointFactory.Fill( 100, 100, 5 );

		Assert.That( points, Is.Not.Empty );
	}

	[Test]
	public void Fill_500x500_AreaFilled() {
		IReadOnlyList<Point> points = _pointFactory.Fill( 500, 500, 5 );

		Assert.That( points, Is.Not.Empty );
	}

	[Test]
	public void Fill_1000x1000_AreaFilled() {
		IReadOnlyList<Point> points = _pointFactory.Fill( 1000, 1000, 5 );

		Assert.That( points, Is.Not.Empty );
	}
}
