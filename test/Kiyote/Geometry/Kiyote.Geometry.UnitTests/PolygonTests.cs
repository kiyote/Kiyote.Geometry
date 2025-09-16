namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class PolygonTests {

	private Polygon _polygon;

	[SetUp]
	public void SetUp() {
		_polygon = CreatePolygon();
	}

	[Test]
	public void Contains_PointInside_ReturnsTrue() {
		Point point = new Point( 10, 10 );

		Assert.That( _polygon.Contains( point ), Is.True );
	}

	[Test]
	public void Contains_PointOutside_ReturnsFalse() {
		Point point = new Point( 30, 30 );

		Assert.That( _polygon.Contains( point ), Is.False );
	}

	[Test]
	public void TryIntersect_NoIntersection_NoPointsReturned() {
		Polygon other = new Polygon(
			[
			new Point( 30, 0 ),
			new Point( 50, 0 ),
			new Point( 50, 20 ),
			new Point( 30, 20 )
			] );

		bool result = _polygon.TryIntersect( other, out Polygon intersectedPolygon );

		Assert.That( result, Is.False );
		Assert.That( intersectedPolygon, Is.SameAs( Polygon.None ) );
	}

	[Test]
	public void TryIntersect_Overlapping_ReturnsClippedShape() {
		Polygon p1 = new Polygon(
			[
				new Point( 200, 200 ),
				new Point( 800, 200 ),
				new Point( 800, 800 ),
				new Point( 200, 800 )
			] );

		Polygon p2 = new Polygon(
			[
				new Point( 250, 250 ),
				new Point( 750, 350 ),
				new Point( 650, 850 ),
				new Point( 150, 700 )
			] );

		bool result = p1.TryIntersect( p2, out Polygon clipped );
		Assert.That( result, Is.True );
		Assert.That( clipped.Points.Count, Is.EqualTo( 6 ) );
		Assert.That(clipped.Points[0].X, Is.EqualTo(660));
		Assert.That(clipped.Points[0].Y, Is.EqualTo(800));
		Assert.That(clipped.Points[1].X, Is.EqualTo(483));
		Assert.That(clipped.Points[1].Y, Is.EqualTo(800));
		Assert.That(clipped.Points[2].X, Is.EqualTo(200));
		Assert.That(clipped.Points[2].Y, Is.EqualTo(715));
		Assert.That(clipped.Points[3].X, Is.EqualTo(200));
		Assert.That(clipped.Points[3].Y, Is.EqualTo(475));
		Assert.That(clipped.Points[4].X, Is.EqualTo(250));
		Assert.That(clipped.Points[4].Y, Is.EqualTo(250));
		Assert.That(clipped.Points[5].X, Is.EqualTo(750));
		Assert.That(clipped.Points[5].Y, Is.EqualTo(350));
	}

	[Test]
	public void Intersections_Overlapping_ReturnsPoints() {
		Polygon other = new Polygon(
			[
				new Point( 5, 5 ),
				new Point( 10, 10 ),
				new Point( 5, 15 ),
				new Point( -5, 10 )
			] );

		IReadOnlyList<Point> intersections = _polygon.Intersections( other.Points );
		Assert.That( intersections, Is.Not.Null );
		Assert.That(intersections[0].X, Is.EqualTo(0));
		Assert.That(intersections[0].Y, Is.EqualTo(12));
		Assert.That(intersections[1].X, Is.EqualTo(0));
		Assert.That(intersections[1].Y, Is.EqualTo(8));
	}

	[Test]
	public void HasIntersection_Polygon_NoIntersection_ReturnsFalse() {
		Polygon other = new Polygon(
			[
				new Point( 30, 0 ),
				new Point( 50, 0 ),
				new Point( 50, 20 ),
				new Point( 30, 20 )
			] );

		bool result = _polygon.HasIntersection( other );

		Assert.That( result, Is.False );
	}

	[Test]
	public void HasIntersection_Polygon_Intersection_ReturnsTrue() {
		Polygon other = new Polygon(
			[
				new Point( -5, 10 ),
				new Point( 10, -5 ),
				new Point( 25, 10 ),
				new Point( 10, 25 )
			] );

		bool result = _polygon.HasIntersection( other );

		Assert.That( result, Is.True );
	}

	[Test]
	public void HasIntersection_EdgeList_NoIntersection_ReturnsFalse() {
		Polygon other = new Polygon(
			[
				new Point( 30, 0 ),
				new Point( 50, 0 ),
				new Point( 50, 20 ),
				new Point( 30, 20 )
			] );

		bool result = _polygon.HasIntersection( other.Edges );

		Assert.That( result, Is.False );
	}

	[Test]
	public void HasIntersection_EdgeList_Intersection_ReturnsTrue() {
		Polygon other = new Polygon(
			[
				new Point( -5, 10 ),
				new Point( 10, -5 ),
				new Point( 25, 10 ),
				new Point( 10, 25 )
			] );

		bool result = _polygon.HasIntersection( other.Edges );

		Assert.That( result, Is.True );
	}

	[Test]
	public void HasIntersection_Edge_NoIntersection_ReturnsFalse() {
		Polygon other = new Polygon(
			[
				new Point( 30, 0 ),
				new Point( 50, 0 ),
				new Point( 50, 20 ),
				new Point( 30, 20 )
			] );

		bool result = _polygon.HasIntersection( other.Edges[0] );

		Assert.That( result, Is.False );
	}

	[Test]
	public void HasIntersection_Edge_Intersection_ReturnsTrue() {
		Polygon other = new Polygon(
			[
				new Point( -5, 10 ),
				new Point( 10, -5 ),
				new Point( 25, 10 ),
				new Point( 10, 25 )
			] );

		bool result = _polygon.HasIntersection( other.Edges[0] );

		Assert.That( result, Is.True );
	}

	[Test]
	public void TryFindIntersections_Intersection_ReturnsPoints() {
		Edge edge = new Edge(
			new Point( -5, 10 ),
			new Point( 10, -5 )
		);

		bool result = _polygon.TryFindIntersections(
			edge,
			out IReadOnlyList<Point> intersections
		);

		Assert.That( result, Is.True );
		Assert.That( intersections, Has.Count.EqualTo( 2 ) );

		IEnumerable<Point> expected = [
			new Point( 5, 0 ),
			new Point( 0, 5 )
		];
		Assert.That( intersections, Is.EquivalentTo( expected ) );
	}

	[Test]
	public void TryFindIntersections_NoIntersection_ReturnsFalse() {
		Edge edge = new Edge(
			new Point( 30, 0 ),
			new Point( 50, 20 )
		);

		bool result = _polygon.TryFindIntersections(
			edge,
			out IReadOnlyList<Point> intersections
		);

		Assert.That( result, Is.False );
		Assert.That( intersections, Is.Empty );
	}

	[TestCase( -1, -1, false )]
	[TestCase( 0, 0, true )]
	[TestCase( 0, 0, true )]
	[TestCase( 21, -1, false )]
	[TestCase( 20, 0, true )]
	[TestCase( 19, 1, true )]
	[TestCase( 21, 21, false )]
	[TestCase( 20, 20, true )]
	[TestCase( 19, 19, true )]
	[TestCase( -1, 21, false )]
	[TestCase( 0, 20, true )]
	[TestCase( 1, 19, true )]
	public void Contains_IntInt_PointInside_ReturnsTrue(
		int x,
		int y,
		bool expected
	) {
		Assert.That( _polygon.Contains( x, y ), Is.EqualTo( expected ) );
	}

	[Test]
	public void Contains_Polygon_PolygonInside_ReturnsTrue() {
		Polygon other = new Polygon(
			[
				new Point( 5, 10 ),
				new Point( 10, 5 ),
				new Point( 15, 10 ),
				new Point( 10, 15 )
			] );

		bool result = _polygon.Contains( other );

		Assert.That( result, Is.True );
	}

	[Test]
	public void Contains_Polygon_PolygonOutside_ReturnsFalse() {
		Polygon other = new Polygon(
			[
				new Point( -5, 10 ),
				new Point( 10, -5 ),
				new Point( 25, 10 ),
				new Point( 10, 25 )
			] );

		bool result = _polygon.Contains( other );

		Assert.That( result, Is.False );
	}

	[Test]
	public void HasOverlap_FullyOverlappingPolygon_ReturnsTrue() {
		Polygon other = new Polygon(
			[
				new Point( -5, 10 ),
				new Point( 10, -5 ),
				new Point( 25, 10 ),
				new Point( 10, 25 )
			] );

		bool result = _polygon.HasOverlap( other );

		Assert.That( result, Is.True );
	}

	[Test]
	public void HasOverlap_PartiallyOverlappingPolygon_ReturnsTrue() {
		Polygon other = new Polygon(
			[
			new Point( 10, 5 ),
			new Point( 30, 5 ),
			new Point( 30, 15 ),
			new Point( 10, 15 )
			] );

		bool result = _polygon.HasOverlap( other );

		Assert.That( result, Is.True );
	}

	[Test]
	public void HasOverlap_ContainedPolygon_ReturnsTrue() {
		Polygon other = new Polygon(
			[
			new Point( 10, 5 ),
			new Point( 15, 5 ),
			new Point( 15, 15 ),
			new Point( 10, 15 )
			] );

		bool result = _polygon.HasOverlap( other );

		Assert.That( result, Is.True );
	}

	[Test]
	public void HasOverlap_ContainingPolygon_ReturnsTrue() {
		Polygon other = new Polygon(
			[
			new Point( -5, -5 ),
			new Point( 25, -5 ),
			new Point( 25, 25 ),
			new Point( -5, 25 )
			] );

		bool result = _polygon.HasOverlap( other );

		Assert.That( result, Is.True );
	}

	[Test]
	public void HasOverlap_NoOverlap_ReturnsFalse() {
		Polygon other = new Polygon(
			[
			new Point( 30, 0 ),
			new Point( 50, 0 ),
			new Point( 50, 20 ),
			new Point( 30, 20 )
			] );

		bool result = _polygon.HasOverlap( other );

		Assert.That( result, Is.False );
	}

	[Test]
	public void IsEquivalentTo_DifferentOrder_ReturnsTrue() {
		Polygon other = new Polygon( [
			new Point( 0, 20 ),
			new Point( 20, 20 ),
			new Point( 20, 0 ),
			new Point( 0, 0 )
		] );

		bool result = _polygon.IsEquivalentTo( other );

		Assert.That( result, Is.True );
	}

	[Test]
	public void IsEquivalentTo_DifferentShape_ReturnsFalse() {
		Polygon other = new Polygon( [
			new Point( 0, 20 ),
			new Point( 20, 20 ),
			new Point( 10, 10 ),
			new Point( 20, 0 ),
			new Point( 0, 0 )
		] );

		bool result = _polygon.IsEquivalentTo( other );

		Assert.That( result, Is.False );
	}

	[Test]
	public void IsEquivalentTo_DifferentPosition_ReturnsFalse() {
		Polygon other = new Polygon( [
			new Point( 10, 30 ),
			new Point( 30, 30 ),
			new Point( 30, 10 ),
			new Point( 10, 10 )
		] );

		bool result = _polygon.IsEquivalentTo( other );

		Assert.That( result, Is.False );
	}


	private static Polygon CreatePolygon() {
		IReadOnlyList<Point> polygon = [
			new Point( 0, 0 ),
			new Point( 20, 0 ),
			new Point( 20, 20 ),
			new Point( 0, 20 )
		];

		return new Polygon( polygon );
	}
}
