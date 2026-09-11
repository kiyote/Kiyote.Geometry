using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Randomization.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
internal sealed class FastPoissonDiscPointFactoryTests {

	private const int Seed = 98765;
	private const int Width = 200;
	private const int Height = 150;
	private const int DistanceApart = 10;

	private IPointFactory _factory;

	[SetUp]
	public void SetUp() {
		_factory = new FastPoissonDiscPointFactory( new FastRandom( Seed ) );
	}

	[Test]
	public void Fill_WidthHeight_ReturnsPoints() {
		IReadOnlyList<Point> points = _factory.Fill( Width, Height, DistanceApart );

		Assert.That( points, Is.Not.Empty );
	}

	[Test]
	public void Fill_Size_MatchesWidthHeight() {
		ISize size = new Rect( 0, 0, Width, Height );

		IReadOnlyList<Point> points = _factory.Fill( size, DistanceApart );

		Assert.That( points, Is.Not.Empty );
		AssertWithinBounds( points );
	}

	[Test]
	public void Fill_PointSize_ReturnsPoints() {
		IReadOnlyList<Point> points = _factory.Fill( new Point( Width, Height ), DistanceApart );

		Assert.That( points, Is.Not.Empty );
		AssertWithinBounds( points );
	}

	[Test]
	public void Fill_SizeClipToBounds_ReturnsPoints() {
		ISize size = new Rect( 0, 0, Width, Height );

		IReadOnlyList<Point> points = _factory.Fill( size, DistanceApart, true );

		Assert.That( points, Is.Not.Empty );
		AssertWithinBounds( points );
	}

	[Test]
	public void Fill_PointSizeClipToBounds_ReturnsPoints() {
		IReadOnlyList<Point> points = _factory.Fill( new Point( Width, Height ), DistanceApart, true );

		Assert.That( points, Is.Not.Empty );
		AssertWithinBounds( points );
	}

	[Test]
	public void Fill_ClipToBounds_AllPointsWithinBounds() {
		IReadOnlyList<Point> points = _factory.Fill( Width, Height, DistanceApart, true );

		AssertWithinBounds( points );
	}

	[Test]
	public void Fill_NoClipToBounds_ReturnsPoints() {
		IReadOnlyList<Point> points = _factory.Fill( Width, Height, DistanceApart, false );

		Assert.That( points, Is.Not.Empty );
	}

	[Test]
	public void Fill_ClipToBounds_ProducesNoMorePointsThanUnclipped() {
		IReadOnlyList<Point> clipped = _factory.Fill( Width, Height, DistanceApart, true );
		IPointFactory other = new FastPoissonDiscPointFactory( new FastRandom( Seed ) );
		IReadOnlyList<Point> unclipped = other.Fill( Width, Height, DistanceApart, false );

		Assert.That( clipped.Count, Is.LessThanOrEqualTo( unclipped.Count ) );
	}

	[Test]
	public void Fill_Points_AreAtLeastDistanceApart() {
		IReadOnlyList<Point> points = _factory.Fill( Width, Height, DistanceApart, true );

		// Separation is enforced in floating point and then rounded to integer
		// coordinates, so rounding each of X and Y can shorten the distance by up to
		// sqrt(2).  Allow for that quantization rather than demanding the exact radius.
		const double tolerance = 1.4143;
		double minimum = DistanceApart - tolerance;
		for( int i = 0; i < points.Count; i++ ) {
			for( int j = i + 1; j < points.Count; j++ ) {
				int dx = points[i].X - points[j].X;
				int dy = points[i].Y - points[j].Y;
				double distance = Math.Sqrt( ( dx * dx ) + ( dy * dy ) );

				Assert.That(
					distance,
					Is.GreaterThanOrEqualTo( minimum ),
					$"Points {points[i]} and {points[j]} are too close together."
				);
			}
		}
	}

	[Test]
	public void Fill_SameSeed_ProducesSameResult() {
		IPointFactory other = new FastPoissonDiscPointFactory( new FastRandom( Seed ) );

		IReadOnlyList<Point> first = _factory.Fill( Width, Height, DistanceApart );
		IReadOnlyList<Point> second = other.Fill( Width, Height, DistanceApart );

		Assert.That( second, Is.EqualTo( first ) );
	}

	[Test]
	public void Fill_DifferentSeed_ProducesDifferentResult() {
		IPointFactory other = new FastPoissonDiscPointFactory( new FastRandom( Seed + 1 ) );

		IReadOnlyList<Point> first = _factory.Fill( Width, Height, DistanceApart );
		IReadOnlyList<Point> second = other.Fill( Width, Height, DistanceApart );

		Assert.That( second, Is.Not.EqualTo( first ) );
	}

	[Test]
	public void Fill_LargeDistanceApart_ReturnsFewerPoints() {
		IPointFactory other = new FastPoissonDiscPointFactory( new FastRandom( Seed ) );

		IReadOnlyList<Point> dense = _factory.Fill( Width, Height, DistanceApart );
		IReadOnlyList<Point> sparse = other.Fill( Width, Height, DistanceApart * 4 );

		Assert.That( sparse.Count, Is.LessThan( dense.Count ) );
	}

	[Test]
	public void Fill_DistanceApartLargerThanArea_ReturnsAtLeastOnePoint() {
		IReadOnlyList<Point> points = _factory.Fill( 10, 10, 100 );

		Assert.That( points, Is.Not.Empty );
	}

	[Test]
	public void Fill_SquareArea_ReturnsPoints() {
		IReadOnlyList<Point> points = _factory.Fill( 100, 100, 5 );

		Assert.That( points, Is.Not.Empty );
		AssertWithinBounds( points, 100, 100 );
	}

	private static void AssertWithinBounds(
		IReadOnlyList<Point> points
	) {
		AssertWithinBounds( points, Width, Height );
	}

	private static void AssertWithinBounds(
		IReadOnlyList<Point> points,
		int width,
		int height
	) {
		foreach( Point point in points ) {
			Assert.That( point.X, Is.GreaterThanOrEqualTo( 0 ) );
			Assert.That( point.X, Is.LessThan( width ) );
			Assert.That( point.Y, Is.GreaterThanOrEqualTo( 0 ) );
			Assert.That( point.Y, Is.LessThan( height ) );
		}
	}
}
