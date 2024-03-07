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

		Assert.That( r.X1, Is.EqualTo( x ), "X1 does not match" );
		Assert.That( r.Y1, Is.EqualTo( y ), "Y1 does not match" );
		Assert.That( r.Width, Is.EqualTo( width ), "Width does not match" );
		Assert.That( r.Height, Is.EqualTo( height ), "Height does not match" );
		Assert.That( r.X2, Is.EqualTo( expectedX2 ), "X2 does not match" );
		Assert.That( r.Y2, Is.EqualTo( expectedY2 ), "Y2 does not match" );
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

		Assert.That( r.X1, Is.EqualTo( x1 ), "X1 does not match" );
		Assert.That( r.Y1, Is.EqualTo( y1 ), "Y1 does not match" );
		Assert.That( r.X2, Is.EqualTo( x2 ), "X2 does not match" );
		Assert.That( r.Y2, Is.EqualTo( y2 ), "Y2 does not match" );
		Assert.That( r.Width, Is.EqualTo( expectedWidth ), "Width does not match" );
		Assert.That( r.Height, Is.EqualTo( expectedHeight ), "Height does not match" );
	}
}
