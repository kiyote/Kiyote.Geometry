namespace Kiyote.Geometry.Tests;

[TestFixture]
public sealed class PointUnitTests {

	private IReadOnlyList<IPoint> _polygon;

	[SetUp]
	public void SetUp() {
		_polygon = CreatePolygon();
	}

	[Test]
	public void Inside_PointInside_ReturnsTrue() {
		IPoint point = new Point( 10, 10 );

		Assert.IsTrue( point.Inside( _polygon ) );
	}

	[Test]
	public void Inside_PointOutside_ReturnsFalse() {
		IPoint point = new Point( 30, 30 );

		Assert.IsFalse( point.Inside( _polygon ) );
	}

	private static IReadOnlyList<IPoint> CreatePolygon() {
		IReadOnlyList<IPoint> polygon = new List<IPoint>() {
			new Point( 0, 0 ),
			new Point( 20, 0 ),
			new Point( 20, 0 ),
			new Point( 20, 20 ),
			new Point( 20, 20 ),
			new Point( 0, 20 ),
			new Point( 0, 20 ),
			new Point( 0, 0 )
		};

		return polygon;
	}
}
