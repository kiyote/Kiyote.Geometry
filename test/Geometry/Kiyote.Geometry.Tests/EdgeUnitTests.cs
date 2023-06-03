namespace Kiyote.Geometry.Tests;

/*
[TestFixture]
internal sealed class EdgeUnitTests {

	[Test]
	public void Intersect_OtherIntersects_PointReturned() {
		Edge edge = new Edge( new Point( 0, 0 ), new Point( 10, 10 ) );
		Edge other = new Edge( new Point( 10, 0 ), new Point( 0, 10 ) );

		bool result = edge.TryFindIntersection( other, out Point intersection );

		Assert.IsTrue( result );
		Assert.AreEqual( 5, intersection.X );
		Assert.AreEqual( 5, intersection.Y );
	}

	[Test]
	public void Intersect_OtherDoesNotIntersect_ReturnsNull() {
		Edge edge = new Edge( new Point( 0, 0 ), new Point( 10, 0 ) );
		Edge other = new Edge( new Point( 0, 10 ), new Point( 10, 10 ) );

		bool result = edge.TryFindIntersection( other, out Point _ );

		Assert.IsFalse( result );
	}

	[Test]
	public void Intersect_IntersectionNotOnSegment_ReturnsNull() {
		Edge edge = new Edge( new Point( 0, 0 ), new Point( 10, 0 ) );
		Edge other = new Edge( new Point( 0, 10 ), new Point( 100, 0 ) );

		bool result = edge.TryFindIntersection( other, out Point _ );

		Assert.IsFalse( result );
	}

	[Test]
	public void Intersect_OtherIsZeroLength_ReturnsNull() {
		Edge edge = new Edge( new Point( 0, 0 ), new Point( 10, 0 ) );
		Edge other = new Edge( new Point( 0, 0 ), new Point( 0, 0 ) );

		bool result = edge.TryFindIntersection( other, out Point _ );

		Assert.IsFalse( result );
	}
}
*/
