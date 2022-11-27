using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class DelaunayFactoryIntegrationTests {

	private IDelaunayFactory _delaunayFactory;
	private IPointFactory _pointFactory;

	[SetUp]
	public void SetUp() {
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
		_delaunayFactory = new DelaunayFactory();
	}

	[Test]
	public void Create_HappyPath_DelaunayCreated() {
		Bounds bounds = new Bounds( 1000, 1000 );
		IReadOnlyList<IPoint> points = _pointFactory.Fill( bounds, 25 );
		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		Delaunator delaunator = delaunatorFactory.Create( points );
		Delaunay delaunay = _delaunayFactory.Create( delaunator );

		Assert.IsNotNull( delaunay );
	}
}
