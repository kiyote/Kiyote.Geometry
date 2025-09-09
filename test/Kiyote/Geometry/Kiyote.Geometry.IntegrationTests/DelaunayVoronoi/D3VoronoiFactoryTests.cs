using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class D3VoronoiFactoryTests {

	private IVoronoiFactory _voronoiFactory;
	private IPointFactory _pointFactory;

	[SetUp]
	public void SetUp() {
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
		_voronoiFactory = new D3VoronoiFactory();
	}

	[Test]
	public void Create_HappyPath_DelaunayCreated() {
		ISize size = new Point( 1000, 1000 );
		Rect bounds = new Rect( 0, 0, size );
		IReadOnlyList<Point> points = _pointFactory.Fill( size, 25 );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		Assert.That( voronoi, Is.Not.Null );
	}

	[Test]
	public void Create_FixedPoints_ReferenceDiagramCreated() {
		ISize size = new Point( 30, 30 );
		Rect bounds = new Rect( 0, 0, size );
		IReadOnlyList<Point> points = [
			new Point(10, 10),
			new Point(20, 10),
			new Point(10, 20),
			new Point(20, 20)
		];
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		Assert.That( voronoi, Is.Not.Null );
	}
}

