namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class EdgeTests {

	[Test]
	public void Equals_SamePointsDifferentOrder_ReturnsTrue() {
		Point p1 = new Point( 100, 200 );
		Point p2 = new Point( 300, 400 );
		Edge e1 = new Edge( p1, p2 );
		Edge e2 = new Edge( p2, p1 );

		Assert.That( e1.Equals( e2 ), Is.True );
	}
}
