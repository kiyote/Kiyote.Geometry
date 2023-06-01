namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class MapboxDelaunatorFactoryIntegrationTests {

	[Test]
	public void Test_InsufficientPoints_ThrowsException() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		List<Point> points = new List<Point>() {
			topLeft,
			topRight
		};

		_ = Assert.Throws<InvalidOperationException>( () => MapboxDelaunatorFactory.Create( points.ToCoords() ) );
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

		_ = Assert.Throws<InvalidOperationException>( () => MapboxDelaunatorFactory.Create( points.Distinct().ToList().ToCoords() ) );
	}

	[TestCase( 0, 0, 10, 0, 0, 10, 10, 10 )]
	[TestCase( 10, 10, 20, 10, 10, 20, 20, 20 )]
	public void Create_Square_ExpectedResults(
		int x1,
		int y1,
		int x2,
		int y2,
		int x3,
		int y3,
		int x4,
		int y4
	) {
		Point topLeft = new Point( x1, y1 );
		Point topRight = new Point( x2, y2 );
		Point bottomLeft = new Point( x3, y3 );
		Point bottomRight = new Point( x4, y4 );
		List<Point> points = new List<Point>() {
			topLeft,
			topRight,
			bottomLeft,
			bottomRight
		};

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

		Assert.NotNull( delaunator );
		// The 4 points of the hull
		Assert.AreEqual( points[delaunator.Hull[0]], bottomLeft, nameof( bottomLeft ) );
		Assert.AreEqual( points[delaunator.Hull[1]], bottomRight, nameof( bottomRight ) );
		Assert.AreEqual( points[delaunator.Hull[2]], topRight, nameof( topRight ) );
		Assert.AreEqual( points[delaunator.Hull[3]], topLeft, nameof( topLeft ) );
		// The two triangles
		Assert.AreEqual( points[delaunator.Triangles[0]], topLeft, nameof( topLeft ) );
		Assert.AreEqual( points[delaunator.Triangles[1]], bottomLeft, nameof( bottomLeft ) );
		Assert.AreEqual( points[delaunator.Triangles[2]], topRight, nameof( topRight ) );
		Assert.AreEqual( points[delaunator.Triangles[3]], bottomLeft, nameof( bottomLeft ) );
		Assert.AreEqual( points[delaunator.Triangles[4]], bottomRight, nameof( bottomRight ) );
		Assert.AreEqual( points[delaunator.Triangles[5]], topRight, nameof( topRight ) );
		// Half-edges
		Assert.AreEqual( delaunator.HalfEdges[0], -1 );
		Assert.AreEqual( delaunator.HalfEdges[1], 5 );
		Assert.AreEqual( delaunator.HalfEdges[2], -1 );
		Assert.AreEqual( delaunator.HalfEdges[3], -1 );
		Assert.AreEqual( delaunator.HalfEdges[4], -1 );
		Assert.AreEqual( delaunator.HalfEdges[5], 1 );
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

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

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

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

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

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

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

	[TestCase( 0, 0, 10, 0, 0, 10, false )]
	[TestCase( 0, 0, 0, 10, 10, 0, true )]
	[TestCase( 10, 0, 10, 10, 0, 10, false )]
	[TestCase( 10, 0, 0, 10, 10, 10, true )]
	[TestCase( 10, 10, 20, 10, 10, 20, false )]
	[TestCase( 10, 10, 10, 20, 20, 10, true )]
	[TestCase( 20, 10, 20, 20, 10, 20, false )]
	[TestCase( 20, 10, 10, 20, 20, 20, true )]
	public void Orient_ThreePoints_ExpectedOrientation(
		double x1,
		double y1,
		double x2,
		double y2,
		double x3,
		double y3,
		bool positive
	) {
		double result = MapboxDelaunatorFactory.Orient( x1, y1, x2, y2, x3, y3 );
		Assert.AreNotEqual( 0.0D, result );
		Assert.AreEqual( positive, result > 0.0D );
	}
}
