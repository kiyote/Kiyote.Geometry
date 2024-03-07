namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
public sealed class MapboxDelaunatorFactoryIntegrationTests {

	[Test]
	public void Test_InsufficientPoints_ThrowsException() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		List<Point> points = [
			topLeft,
			topRight
		];

		_ = Assert.Throws<InvalidOperationException>( () => MapboxDelaunatorFactory.Create( points.ToCoords() ) );
	}

	[Test]
	public void Test_DuplicatePointsAreInsufficient_ThrowsException() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		List<Point> points = [
			topLeft,
			topRight,
			topLeft,
			topRight
		];

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
		List<Point> points = [
			topLeft,
			topRight,
			bottomLeft,
			bottomRight
		];

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

		Assert.That( delaunator, Is.Not.Null );
		// The 4 points of the hull
		Assert.That( points[delaunator.Hull[0]], Is.EqualTo( bottomLeft ), nameof( bottomLeft ) );
		Assert.That( points[delaunator.Hull[1]], Is.EqualTo( bottomRight ), nameof( bottomRight ) );
		Assert.That( points[delaunator.Hull[2]], Is.EqualTo( topRight ), nameof( topRight ) );
		Assert.That( points[delaunator.Hull[3]], Is.EqualTo( topLeft ), nameof( topLeft ) );
		// The two triangles
		Assert.That( points[delaunator.Triangles[0]], Is.EqualTo( topLeft ), nameof( topLeft ) );
		Assert.That( points[delaunator.Triangles[1]], Is.EqualTo( bottomLeft ), nameof( bottomLeft ) );
		Assert.That( points[delaunator.Triangles[2]], Is.EqualTo( topRight ), nameof( topRight ) );
		Assert.That( points[delaunator.Triangles[3]], Is.EqualTo( bottomLeft ), nameof( bottomLeft ) );
		Assert.That( points[delaunator.Triangles[4]], Is.EqualTo( bottomRight ), nameof( bottomRight ) );
		Assert.That( points[delaunator.Triangles[5]], Is.EqualTo( topRight ), nameof( topRight ) );
		// Half-edges
		Assert.That( delaunator.HalfEdges[0], Is.EqualTo( -1 ) );
		Assert.That( delaunator.HalfEdges[1], Is.EqualTo( 5 ) );
		Assert.That( delaunator.HalfEdges[2], Is.EqualTo( -1 ) );
		Assert.That( delaunator.HalfEdges[3], Is.EqualTo( -1 ) );
		Assert.That( delaunator.HalfEdges[4], Is.EqualTo( -1 ) );
		Assert.That( delaunator.HalfEdges[5], Is.EqualTo( 1 ) );
	}

	[Test]
	public void Create_Star_InteriorPointNotInHull() {
		Point topLeft = new Point( 0, 0 );
		Point topRight = new Point( 10, 0 );
		Point bottomLeft = new Point( 0, 10 );
		Point bottomRight = new Point( 10, 10 );
		Point center = new Point( 5, 5 );
		List<Point> points = [
			topLeft,
			topRight,
			bottomLeft,
			bottomRight,
			center
		];

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

		Assert.That( delaunator, Is.Not.Null );
		// Four points for the hull
		Assert.That( points[delaunator.Hull[0]], Is.EqualTo( bottomLeft ), nameof( bottomLeft ) );
		Assert.That( points[delaunator.Hull[1]], Is.EqualTo( bottomRight ), nameof( bottomRight ) );
		Assert.That( points[delaunator.Hull[2]], Is.EqualTo( topRight ), nameof( topRight ) );
		Assert.That( points[delaunator.Hull[3]], Is.EqualTo( topLeft ), nameof( topLeft ) );
		// Four triangles
		Assert.That( points[delaunator.Triangles[0]], Is.EqualTo( center ), nameof( center ) );
		Assert.That( points[delaunator.Triangles[1]], Is.EqualTo( topRight ), nameof( topRight ) );
		Assert.That( points[delaunator.Triangles[2]], Is.EqualTo( topLeft ), nameof( topLeft ) );
		Assert.That( points[delaunator.Triangles[3]], Is.EqualTo( topLeft ), nameof( topLeft ) );
		Assert.That( points[delaunator.Triangles[4]], Is.EqualTo( bottomLeft ), nameof( bottomLeft ) );
		Assert.That( points[delaunator.Triangles[5]], Is.EqualTo( center ), nameof( center ) );
		Assert.That( points[delaunator.Triangles[6]], Is.EqualTo( bottomLeft ), nameof( bottomLeft ) );
		Assert.That( points[delaunator.Triangles[7]], Is.EqualTo( bottomRight ), nameof( bottomRight ) );
		Assert.That( points[delaunator.Triangles[8]], Is.EqualTo( center ), nameof( center ) );
		Assert.That( points[delaunator.Triangles[9]], Is.EqualTo( center ), nameof( center ) );
		Assert.That( points[delaunator.Triangles[10]], Is.EqualTo( bottomRight ), nameof( bottomRight ) );
		Assert.That( points[delaunator.Triangles[11]], Is.EqualTo( topRight ), nameof( topRight ) );
	}

	[Test]
	public void Create_TriangleWithExternalCentroid_ResultsWoundCorrectly() {
		Point p1 = new Point( 0, 0 );
		Point p2 = new Point( 10, 0 );
		Point p3 = new Point( 0, 5 );
		List<Point> points = [
			p1,
			p2,
			p3
		];

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

		Assert.That( delaunator, Is.Not.Null );
		// The 3 points of the hull
		Assert.That( points[delaunator.Hull[0]], Is.EqualTo( p1 ), nameof( p1 ) );
		Assert.That( points[delaunator.Hull[1]], Is.EqualTo( p3 ), nameof( p3 ) );
		Assert.That( points[delaunator.Hull[2]], Is.EqualTo( p2 ), nameof( p2 ) );
		// The two triangles
		Assert.That( points[delaunator.Triangles[0]], Is.EqualTo( p1 ), nameof( p1 ) );
		Assert.That( points[delaunator.Triangles[1]], Is.EqualTo( p3 ), nameof( p3 ) );
		Assert.That( points[delaunator.Triangles[2]], Is.EqualTo( p2 ), nameof( p2 ) );
	}

	[Test]
	public void Create_FlippedTriangleWithExternalCentroid_ResultsWoundCorrectly() {
		Point p1 = new Point( 0, 0 );
		Point p2 = new Point( 5, 0 );
		Point p3 = new Point( 0, 10 );
		List<Point> points = [
			p1,
			p2,
			p3
		];

		MapboxDelaunator delaunator = MapboxDelaunatorFactory.Create( points.ToCoords() );

		Assert.That( delaunator, Is.Not.Null );
		// The 3 points of the hull
		Assert.That( points[delaunator.Hull[0]], Is.EqualTo( p2 ), nameof( p2 ) );
		Assert.That( points[delaunator.Hull[1]], Is.EqualTo( p1 ), nameof( p1 ) );
		Assert.That( points[delaunator.Hull[2]], Is.EqualTo( p3 ), nameof( p3 ) );
		// The two triangles
		Assert.That( points[delaunator.Triangles[0]], Is.EqualTo( p2 ), nameof( p2 ) );
		Assert.That( points[delaunator.Triangles[1]], Is.EqualTo( p1 ), nameof( p1 ) );
		Assert.That( points[delaunator.Triangles[2]], Is.EqualTo( p3 ), nameof( p3 ) );
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
		Assert.That( result, Is.Not.EqualTo( 0.0D ) );
		Assert.That( result > 0.0D, Is.EqualTo( positive ) );
	}
}
