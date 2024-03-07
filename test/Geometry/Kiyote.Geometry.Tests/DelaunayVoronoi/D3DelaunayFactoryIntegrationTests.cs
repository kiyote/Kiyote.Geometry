using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class D3DelaunayFactoryIntegrationTests {

	private IDelaunayFactory _delaunayFactory;
	private IPointFactory _pointFactory;

	[SetUp]
	public void SetUp() {
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
		_delaunayFactory = new D3DelaunayFactory();
	}

	[Test]
	public void Create_HappyPath_DelaunayCreated() {
		ISize bounds = new Point( 1000, 1000 );
		IReadOnlyList<Point> points = _pointFactory.Fill( bounds, 25 );
		IDelaunay delaunay = _delaunayFactory.Create( points );

		Assert.That( delaunay, Is.Not.Null );
	}
}
