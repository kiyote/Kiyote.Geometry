namespace Kiyote.Geometry.Tests;

[TestFixture]
internal sealed class RectUnitTests {

	[TestCase( 0, 0, 10, 10, 9, 9 )]
	[TestCase( 0, 10, 20, 30, 19, 39 )]
	[TestCase( -10, 0, 30, 10, 19, 9 )]
	public void Ctor_CoordsAndSize_ExpectedCoordinates(
		int x,
		int y,
		int width,
		int height,
		int expectedX2,
		int expectedY2
	) {
		Rect r = new Rect( x, y, width, height );

		Assert.AreEqual( x, r.X1, "X1 does not match" );
		Assert.AreEqual( y, r.Y1, "Y1 does not match" );
		Assert.AreEqual( width, r.Width, "Width does not match" );
		Assert.AreEqual( height, r.Height, "Height does not match" );
		Assert.AreEqual( expectedX2, r.X2, "X2 does not match" );
		Assert.AreEqual( expectedY2, r.Y2, "Y2 does not match" );
	}

	[TestCase( 0, 10, 20, 40, 21, 31 )]
	[TestCase( 0, 0, 9, 9, 10, 10 )]
	public void Ctor_TwoPoints_ExpectedDimensions(
		int x1,
		int y1,
		int x2,
		int y2,
		int expectedWidth,
		int expectedHeight
	) {
		Rect r = new Rect( new Point( x1, y1 ), new Point( x2, y2 ) );

		Assert.AreEqual( x1, r.X1, "X1 does not match" );
		Assert.AreEqual( y1, r.Y1, "Y1 does not match" );
		Assert.AreEqual( x2, r.X2, "X2 does not match" );
		Assert.AreEqual( y2, r.Y2, "Y2 does not match" );
		Assert.AreEqual( expectedWidth, r.Width, "Width does not match" );
		Assert.AreEqual( expectedHeight, r.Height, "Height does not match" );
	}
}
