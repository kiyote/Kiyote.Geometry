namespace Kiyote.Geometry.Tests;

[TestFixture]
public sealed class PolygonUnitTests {

	private IPolygon _polygon;

	[SetUp]
	public void SetUp() {
		_polygon = CreatePolygon();
	}

	[Test]
	public void Contains_PointInside_ReturnsTrue() {
		IPoint point = new Point( 10, 10 );

		Assert.IsTrue( _polygon.Contains( point ) );
	}

	[Test]
	public void Contains_PointOutside_ReturnsFalse() {
		IPoint point = new Point( 30, 30 );

		Assert.IsFalse( _polygon.Contains( point ) );
	}

	[Test]
	public void TryFindIntersection_Overlapping_ReturnsClippedShape() {
		IPolygon p1 = new Polygon(
			new List<IPoint>() {
				new Point( 200, 200 ),
				new Point( 800, 200 ),
				new Point( 800, 800 ),
				new Point( 200, 800 )
			} );

		IPolygon p2 = new Polygon(
			new List<IPoint>() {
				new Point( 250, 250 ),
				new Point( 750, 350 ),
				new Point( 650, 850 ),
				new Point( 150, 700 )
			} );

		bool result = p1.TryFindIntersection( p2, out IPolygon clipped );
		Assert.IsTrue( result );
		Assert.AreEqual( 6, clipped.Points.Count );
		Assert.AreEqual( 660, clipped.Points[0].X );
		Assert.AreEqual( 800, clipped.Points[0].Y );
		Assert.AreEqual( 483, clipped.Points[1].X );
		Assert.AreEqual( 800, clipped.Points[1].Y );
		Assert.AreEqual( 200, clipped.Points[2].X );
		Assert.AreEqual( 715, clipped.Points[2].Y );
		Assert.AreEqual( 200, clipped.Points[3].X );
		Assert.AreEqual( 475, clipped.Points[3].Y );
		Assert.AreEqual( 250, clipped.Points[4].X );
		Assert.AreEqual( 250, clipped.Points[4].Y );
		Assert.AreEqual( 750, clipped.Points[5].X );
		Assert.AreEqual( 350, clipped.Points[5].Y );
	}

	[Test]
	public void Intersections_Overlapping_ReturnsPoints() {
		IPolygon other = new Polygon(
			new List<IPoint>() {
				new Point( 5, 5 ),
				new Point( 10, 10 ),
				new Point( 5, 15 ),
				new Point( -5, 10 )
			} );

		IReadOnlyList<IPoint> intersections = _polygon.Intersections( other.Points );
		Assert.IsNotNull( intersections );
		Assert.AreEqual( 0, intersections[0].X );
		Assert.AreEqual( 12, intersections[0].Y );
		Assert.AreEqual( 0, intersections[1].X );
		Assert.AreEqual( 8, intersections[1].Y );
	}

	private static IPolygon CreatePolygon() {
		IReadOnlyList<IPoint> polygon = new List<IPoint>() {
			new Point( 0, 0 ),
			new Point( 20, 0 ),
			new Point( 20, 20 ),
			new Point( 0, 20 )
		};

		return new Polygon( polygon );
	}
}
