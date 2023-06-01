using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class VoronoiFactoryIntegrationTests {

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
		Bounds size = new Bounds( 1000, 1000 );
		IRect bounds = new Rect( 0, 0, size );
		IReadOnlyList<Point> points = _pointFactory.Fill( size, 25 );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		Assert.IsNotNull( voronoi );
	}

	[Test]
	public void Create_FixedPoints_ReferenceDiagramCreated() {
		Bounds size = new Bounds( 30, 30 );
		IRect bounds = new Rect( 0, 0, size );
		IReadOnlyList<Point> points = new List<Point>() {
			new Point(10, 10),
			new Point(20, 10),
			new Point(10, 20),
			new Point(20, 20)
		};
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		Assert.IsNotNull( voronoi );
	}
}

