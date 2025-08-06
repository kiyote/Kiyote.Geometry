namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class PolygonUnitTests {

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
	public void TryFindIntersection_Overlapping_ReturnsClippedShape() {
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

		bool result = p1.Intersect( p2, out Polygon clipped );
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
