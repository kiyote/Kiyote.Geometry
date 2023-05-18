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
		_voronoiFactory = new VoronoiFactory();
	}

	[Test]
	public void Create_HappyPath_DelaunayCreated() {
		Bounds bounds = new Bounds( 1000, 1000 );
		IReadOnlyList<Point> points = _pointFactory.Fill( bounds, 25 );
		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		Delaunator delaunator = delaunatorFactory.Create( points );
		IVoronoi voronoi = _voronoiFactory.Create( delaunator, bounds.Width, bounds.Height );

		Assert.IsNotNull( voronoi );
	}

	[Test]
	public void Create_FixedPoints_ReferenceDiagramCreated() {
		Bounds bounds = new Bounds( 30, 30 );
		IReadOnlyList<Point> points = new List<Point>() {
			new Point(10, 10),
			new Point(20, 10),
			new Point(10, 20),
			new Point(20, 20)
		};
		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		Delaunator delaunator = delaunatorFactory.Create( points );
		IVoronoi voronoi = _voronoiFactory.Create( delaunator, bounds.Width, bounds.Height );

		Assert.IsNotNull( voronoi );
	}
}
