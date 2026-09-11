using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
internal sealed class RectTests {

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

	[Test]
	public void IsEquivalentTo_IRect_SamePointsDifferentOrder_ReturnsTrue() {
		Point p1 = new Point( 100, 200 );
		Point p2 = new Point( 200, 300 );
		Rect r1 = new Rect( p1, p2 );
		Point p3 = new Point( 100, 300 );
		Point p4 = new Point( 200, 200 );
		Rect r2 = new Rect( p3, p4 );

		Assert.That( r1.IsEquivalentTo( r2 ), Is.True );
	}

	[Test]
	public void IsEquivalentTo_PointPoint_SamePointsDifferentOrder_ReturnsTrue() {
		Point p1 = new Point( 100, 200 );
		Point p2 = new Point( 200, 300 );
		Rect r1 = new Rect( p1, p2 );

		Assert.That( r1.IsEquivalentTo( p2, p1 ), Is.True );
	}

	[Test]
	public void IsEquivalentTo_IntIntIntInt_SamePointsDifferentOrder_ReturnsTrue() {
		Point p1 = new Point( 100, 200 );
		Point p2 = new Point( 200, 300 );
		Rect r1 = new Rect( p1, p2 );

		Assert.That( r1.IsEquivalentTo( p2.X, p2.Y, p1.X, p1.Y ), Is.True );
	}

	[Test]
	public void IsEquivalentTo_IntIntIntInt_DifferentPoints_ReturnsFalse() {
		Point p1 = new Point( 100, 200 );
		Point p2 = new Point( 200, 300 );
		Rect r1 = new Rect( p1, p2 );

		Assert.That( r1.IsEquivalentTo( 10, 20, 20, 30 ), Is.False );
	}

	// Top left
	[TestCase( 9, 9, false )]
	[TestCase( 10, 9, false )]
	[TestCase( 9, 10, false )]
	[TestCase( 10, 10, true )]
	// Bottom left
	[TestCase( 9, 59, false )]
	[TestCase( 9, 60, false )]
	[TestCase( 10, 60, false )]
	[TestCase( 10, 59, true )]
	// Top right
	[TestCase( 59, 9, false )]
	[TestCase( 60, 9, false )]
	[TestCase( 60, 10, false )]
	[TestCase( 59, 10, true )]
	// Bottom right
	[TestCase( 60, 60, false )]
	[TestCase( 60, 59, false )]
	[TestCase( 59, 60, false )]
	[TestCase( 59, 59, true )]
	public void Contains_IntInt_TestCases_ReturnsExpectedResult(
		int x,
		int y,
		bool expected
	) {
		Rect r = CreateRect();

		Assert.That( r.Contains( x, y ), Is.EqualTo( expected ) );
	}

	[Test]
	public void Contains_Point_PointInside_ReturnsTrue() {
		Rect r = CreateRect();
		Point p = new Point( 20, 20 );

		Assert.That( r.Contains( p ), Is.True );
	}

	[Test]
	public void Contains_Point_PointOutside_ReturnsFalse() {
		Rect r = CreateRect();
		Point p = new Point( 0, 0 );

		Assert.That( r.Contains( p ), Is.False );
	}

	[TestCase( 10, 10, 50, 50, true)]
	[TestCase( 11, 11, 48, 48, true )]
	[TestCase( 10, 10, 60, 60, false )]
	[TestCase( 5, 5, 50, 50, false )]
	[TestCase( 15, 15, 50, 50, false )]
	public void Contains_IRect_TestCases_ExpectedResult(
		int x,
		int y,
		int w,
		int h,
		bool expected
	) {
		Rect r1 = CreateRect();
		Rect r2 = new Rect( x, y, w, h );
		Assert.That( r1.Contains( r2 ), Is.EqualTo( expected ) );
	}

	[Test]
	public void Ctor_NegativeWidth_ThrowsException() {
		Assert.Throws<InvalidOperationException>( () => new Rect( 0, 0, -1, 10 ) );
	}

	[Test]
	public void Ctor_ZeroWidth_CollapsesToOrigin() {
		Rect r = new Rect( 5, 7, 0, 0 );

		Assert.That( r.X2, Is.EqualTo( 5 ) );
		Assert.That( r.Y2, Is.EqualTo( 7 ) );
	}

	[Test]
	public void Ctor_CoordsAndSize_MatchesWidthHeight() {
		Rect r = new Rect( 1, 2, new Rect( 0, 0, 10, 20 ) );

		Assert.That( r.Width, Is.EqualTo( 10 ) );
		Assert.That( r.Height, Is.EqualTo( 20 ) );
	}

	[Test]
	public void IsEquivalentTo_IRectInterface_SameBounds_ReturnsTrue() {
		Rect r1 = CreateRect();
		IRect r2 = new Rect( 10, 10, 50, 50 );

		Assert.That( r1.IsEquivalentTo( r2 ), Is.True );
	}

	[Test]
	public void IsEquivalentTo_IRectInterface_DifferentBounds_ReturnsFalse() {
		Rect r1 = CreateRect();
		IRect r2 = new Rect( 0, 0, 5, 5 );

		Assert.That( r1.IsEquivalentTo( r2 ), Is.False );
	}

	[Test]
	public void Equals_SameRect_ReturnsTrue() {
		Rect r1 = CreateRect();
		object r2 = new Rect( 10, 10, 50, 50 );

		Assert.That( r1.Equals( r2 ), Is.True );
	}

	[Test]
	public void Equals_DifferentRect_ReturnsFalse() {
		Rect r1 = CreateRect();
		object r2 = new Rect( 0, 0, 50, 50 );

		Assert.That( r1.Equals( r2 ), Is.False );
	}

	[Test]
	public void Equals_NotARect_ReturnsFalse() {
		Rect r1 = CreateRect();

		Assert.That( r1.Equals( "not a rect" ), Is.False );
	}

	[Test]
	public void Equals_IEquatableSameRect_ReturnsTrue() {
		IEquatable<Rect> r1 = CreateRect();

		Assert.That( r1.Equals( new Rect( 10, 10, 50, 50 ) ), Is.True );
	}

	[Test]
	public void Equals_IEquatableDifferentRect_ReturnsFalse() {
		IEquatable<Rect> r1 = CreateRect();

		Assert.That( r1.Equals( new Rect( 0, 0, 1, 1 ) ), Is.False );
	}

	[Test]
	public void GetHashCode_EquivalentRects_HashesMatch() {
		Rect r1 = CreateRect();
		Rect r2 = new Rect( 10, 10, 50, 50 );

		Assert.That( r1.GetHashCode(), Is.EqualTo( r2.GetHashCode() ) );
	}

	[Test]
	public void OperatorEquality_SameRect_ReturnsTrue() {
		Assert.That( CreateRect() == new Rect( 10, 10, 50, 50 ), Is.True );
	}

	[Test]
	public void OperatorEquality_DifferentRect_ReturnsFalse() {
		Assert.That( CreateRect() == new Rect( 0, 0, 50, 50 ), Is.False );
	}

	[Test]
	public void OperatorInequality_DifferentRect_ReturnsTrue() {
		Assert.That( CreateRect() != new Rect( 0, 0, 50, 50 ), Is.True );
	}

	[Test]
	public void OperatorInequality_SameRect_ReturnsFalse() {
		Assert.That( CreateRect() != new Rect( 10, 10, 50, 50 ), Is.False );
	}

	[TestCase( 10, 10, 50, 50, true )]
	[TestCase( 100, 100, 10, 10, false )]
	[TestCase( 55, 55, 20, 20, true )]
	public void HasOverlap_TestCases_ExpectedResult(
		int x,
		int y,
		int w,
		int h,
		bool expected
	) {
		Rect r1 = CreateRect();
		IRect r2 = new Rect( x, y, w, h );

		Assert.That( r1.HasOverlap( r2 ), Is.EqualTo( expected ) );
	}

	[Test]
	public void Contains_RectStruct_ContainedRect_ReturnsTrue() {
		Rect r1 = CreateRect();
		Rect r2 = new Rect( 11, 11, 10, 10 );

		Assert.That( r1.Contains( r2 ), Is.True );
	}

	[Test]
	public void Contains_IRectInterface_ContainedRect_ReturnsTrue() {
		Rect r1 = CreateRect();
		IRect r2 = new Rect( 11, 11, 10, 10 );

		Assert.That( r1.Contains( r2 ), Is.True );
	}

	private static Rect CreateRect() {
		return new Rect( 10, 10, 50, 50 );
	}
}
