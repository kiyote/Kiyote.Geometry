namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class TriangleTests {

	[Test]
	public void Ctor_ValidPoints_PropertiesSet() {
		Point p1 = new Point( 10, 0 );
		Point p2 = new Point( 20, 10 );
		Point p3 = new Point( 0, 10 );
		Triangle t = new Triangle( p1, p2, p3 );

		Assert.That( t.P1, Is.EqualTo( p1 ) );
		Assert.That( t.P2, Is.EqualTo( p2 ) );
		Assert.That( t.P3, Is.EqualTo( p3 ) );
	}
}
