namespace Kiyote.Geometry.Tests;

[TestFixture]
internal class EdgeUnitTests {

	[Test]
	public void Intersect_OtherIntersects_PointReturned() {
		IEdge edge = new Edge( new Point( 0, 0 ), new Point( 10, 10 ) );
		IEdge other = new Edge( new Point( 10, 0 ), new Point( 0, 10 ) );

		bool result = edge.TryFindIntersection( other, out IPoint intersection );

		Assert.IsTrue( result );
		Assert.AreEqual( 5, intersection.X );
		Assert.AreEqual( 5, intersection.Y );
	}

	[Test]
	public void Intersect_OtherDoesNotIntersect_ReturnsNull() {
		IEdge edge = new Edge( new Point( 0, 0 ), new Point( 10, 0 ) );
		IEdge other = new Edge( new Point( 0, 10 ), new Point( 10, 10 ) );

		bool result = edge.TryFindIntersection( other, out IPoint _ );

		Assert.IsFalse( result );
	}

	[Test]
	public void Intersect_IntersectionNotOnSegment_ReturnsNull() {
		IEdge edge = new Edge( new Point( 0, 0 ), new Point( 10, 0 ) );
		IEdge other = new Edge( new Point( 0, 10 ), new Point( 100, 0 ) );

		bool result = edge.TryFindIntersection( other, out IPoint _ );

		Assert.IsFalse( result );
	}

	[Test]
	public void Intersect_OtherIsZeroLength_ReturnsNull() {
		IEdge edge = new Edge( new Point( 0, 0 ), new Point( 10, 0 ) );
		IEdge other = new Edge( new Point( 0, 0 ), new Point( 0, 0 ) );

		bool result = edge.TryFindIntersection( other, out IPoint _ );

		Assert.IsFalse( result );
	}
}
