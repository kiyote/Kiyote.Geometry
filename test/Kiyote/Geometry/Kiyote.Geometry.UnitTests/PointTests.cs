namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class PointTests {

	[Test]
	public void Ctor_ValidParameters_PropertiesSet() {
		Point p = new Point( 1, 2 );

		Assert.That( p.X, Is.EqualTo( 1 ) );
		Assert.That( p.Y, Is.EqualTo( 2 ) );
	}

	[Test]
	public void Subtract_ValidPoint_NewPointCalculated() {
		Point p1 = new Point( 10, 12 );
		Point p2 = new Point( 2, 3 );

		Point p3 = p1.Subtract( p2 );

		Assert.That( p3.X, Is.EqualTo( 8 ) );
		Assert.That( p3.Y, Is.EqualTo( 9 ) );
	}

	[Test]
	public void Add_ValidPoint_NewPointCalculated() {
		Point p1 = new Point( 10, 12 );
		Point p2 = new Point( 2, 3 );

		Point p3 = p1.Add( p2 );

		Assert.That( p3.X, Is.EqualTo( 12 ) );
		Assert.That( p3.Y, Is.EqualTo( 15 ) );
	}

	[Test]
	public void IEquatableEquals_OtherIsNull_ReturnsFalse() {
		Point p1 = new Point( 10, 12 );

		bool result = ((IEquatable<Point>) p1 ).Equals( null );

		Assert.That( result, Is.False );
	}

	[Test]
	public void IEquatableEquals_OtherIsEqualPoint_ReturnsTrue() {
		Point p1 = new Point( 10, 12 );
		Point p2 = new Point( 10, 12 );

		bool result = ( (IEquatable<Point>)p1 ).Equals( p2 );

		Assert.That( result, Is.True );
	}
}
