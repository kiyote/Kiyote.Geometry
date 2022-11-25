namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class DelaunatorFactoryIntegrationTests {

	private IDelaunatorFactory _delaunatorFactory;

	[SetUp]
	public void SetUp() {
		_delaunatorFactory = new DelaunatorFactory();
	}

	[Test]
	public void Test_InsufficientPoints_ThrowsException() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		List<Point> points = new List<Point>() {
			topLeft,
			topRight
		};

		Assert.Throws<ArgumentException>( () => _delaunatorFactory.Create( points ) );
	}

	[Test]
	public void Test_DuplicatePointsAreInsufficient_ThrowsException() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		List<Point> points = new List<Point>() {
			topLeft,
			topRight,
			topLeft,
			topRight
		};

		Assert.Throws<ArgumentException>( () => _delaunatorFactory.Create( points ) );
	}

	[Test]
	public void Create_Square_ExpectedResults() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		Point bottomLeft = new Point( 0, 10 );
		Point bottomRight = new Point( 10, 10 );
		List<Point> points = new List<Point>() {
			topLeft,
			topRight,
			bottomLeft,
			bottomRight
		};

		Delaunator delaunator = _delaunatorFactory.Create( points );

		Assert.NotNull( delaunator );
		// The 4 points of the hull
		Assert.AreEqual( points[delaunator.Hull[0]], bottomLeft );
		Assert.AreEqual( points[delaunator.Hull[1]], bottomRight );
		Assert.AreEqual( points[delaunator.Hull[2]], topRight );
		Assert.AreEqual( points[delaunator.Hull[3]], topLeft );
		// The two triangles
		Assert.AreEqual( points[delaunator.Triangles[0]], topLeft );
		Assert.AreEqual( points[delaunator.Triangles[1]], bottomLeft );
		Assert.AreEqual( points[delaunator.Triangles[2]], topRight );
		Assert.AreEqual( points[delaunator.Triangles[3]], bottomLeft );
		Assert.AreEqual( points[delaunator.Triangles[4]], bottomRight );
		Assert.AreEqual( points[delaunator.Triangles[5]], topRight );
	}

	[Test]
	public void Create_Star_InteriorPointNotInHull() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		Point bottomLeft = new Point( 0, 10 );
		Point bottomRight = new Point( 10, 10 );
		Point center = new Point( 5, 5 );
		List<Point> points = new List<Point>() {
			topLeft,
			topRight,
			bottomLeft,
			bottomRight,
			center
		};

		Delaunator delaunator = _delaunatorFactory.Create( points );

		Assert.NotNull( delaunator );
		// Four points for the hull
		Assert.AreEqual( points[delaunator.Hull[0]], bottomLeft );
		Assert.AreEqual( points[delaunator.Hull[1]], bottomRight );
		Assert.AreEqual( points[delaunator.Hull[2]], topRight );
		Assert.AreEqual( points[delaunator.Hull[3]], topLeft );
		// Four triangles
		Assert.AreEqual( points[delaunator.Triangles[0]], center );
		Assert.AreEqual( points[delaunator.Triangles[1]], topRight );
		Assert.AreEqual( points[delaunator.Triangles[2]], topLeft );
		Assert.AreEqual( points[delaunator.Triangles[3]], topLeft );
		Assert.AreEqual( points[delaunator.Triangles[4]], bottomLeft );
		Assert.AreEqual( points[delaunator.Triangles[5]], center );
		Assert.AreEqual( points[delaunator.Triangles[6]], bottomLeft );
		Assert.AreEqual( points[delaunator.Triangles[7]], bottomRight );
		Assert.AreEqual( points[delaunator.Triangles[8]], center );
		Assert.AreEqual( points[delaunator.Triangles[9]], center );
		Assert.AreEqual( points[delaunator.Triangles[10]], bottomRight );
		Assert.AreEqual( points[delaunator.Triangles[11]], topRight );
	}

	[Test]
	public void Create_TriangleWithExternalCentroid_ResultsWoundCorrectly() {
		Point p1 = new Point( 0, 0 );
		Point p2 = new Point( 10, 0 );
		Point p3 = new Point( 0, 5 );
		List<Point> points = new List<Point>() {
			p1,
			p2,
			p3
		};

		Delaunator delaunator = _delaunatorFactory.Create( points );

		Assert.NotNull( delaunator );
		// The 3 points of the hull
		Assert.AreEqual( points[delaunator.Hull[0]], p1 );
		Assert.AreEqual( points[delaunator.Hull[1]], p3 );
		Assert.AreEqual( points[delaunator.Hull[2]], p2 );
		// The two triangles
		Assert.AreEqual( points[delaunator.Triangles[0]], p1 );
		Assert.AreEqual( points[delaunator.Triangles[1]], p3 );
		Assert.AreEqual( points[delaunator.Triangles[2]], p2 );
	}

	[Test]
	public void Create_FlippedTriangleWithExternalCentroid_ResultsWoundCorrectly() {
		Point p1 = new Point( 0, 0 );
		Point p2 = new Point( 5, 0 );
		Point p3 = new Point( 0, 10 );
		List<Point> points = new List<Point>() {
			p1,
			p2,
			p3
		};

		Delaunator delaunator = _delaunatorFactory.Create( points );

		Assert.NotNull( delaunator );
		// The 3 points of the hull
		Assert.AreEqual( points[delaunator.Hull[0]], p2 );
		Assert.AreEqual( points[delaunator.Hull[1]], p1 );
		Assert.AreEqual( points[delaunator.Hull[2]], p3 );
		// The two triangles
		Assert.AreEqual( points[delaunator.Triangles[0]], p2 );
		Assert.AreEqual( points[delaunator.Triangles[1]], p1 );
		Assert.AreEqual( points[delaunator.Triangles[2]], p3 );
	}
}
