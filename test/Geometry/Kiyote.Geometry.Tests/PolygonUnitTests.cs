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
