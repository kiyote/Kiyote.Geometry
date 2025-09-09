namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class EdgeTests {

	[Test]
	public void IsEquivalentTo_SamePointsDifferentOrder_ReturnsTrue() {
		Point p1 = new Point( 100, 200 );
		Point p2 = new Point( 300, 400 );
		Edge e1 = new Edge( p1, p2 );
		Edge e2 = new Edge( p2, p1 );

		Assert.That( e1.IsEquivalentTo( e2 ), Is.True );
		Assert.That( e2.IsEquivalentTo( e1 ), Is.True );
	}

	[Test]
	public void IsEquivalentTo_SamePointsSameOrder_ReturnsTrue() {
		Point p1 = new Point( 100, 200 );
		Point p2 = new Point( 300, 400 );
		Edge e1 = new Edge( p1, p2 );
		Edge e2 = new Edge( p1, p2 );

		Assert.That( e1.IsEquivalentTo( e2 ), Is.True );
		Assert.That( e2.IsEquivalentTo( e1 ), Is.True );
	}

	[Test]
	public void GetBoundingBox_EdgeNotTopLeftBottomRight_CorrectBoundingBox() {
		Point p1 = new Point( 100, 0 );
		Point p2 = new Point( 0, 100 );
		Edge edge = new Edge( p1, p2 );

		Rect boundingBox = edge.GetBoundingBox();

		Assert.That( boundingBox.Width, Is.EqualTo( 101 ) );
		Assert.That( boundingBox.Height, Is.EqualTo( 101 ) );
		Assert.That( boundingBox.X1, Is.EqualTo( 0 ) );
		Assert.That( boundingBox.Y1, Is.EqualTo( 0 ) );
		Assert.That( boundingBox.X2, Is.EqualTo( 100 ) );
		Assert.That( boundingBox.Y2, Is.EqualTo( 100 ) );
	}

	[Test]
	public void HasIntersection_EdgesIntersect_ReturnsTrue() {
		Point p1 = new Point( 0, 0 );
		Point p2 = new Point( 100, 100 );
		Edge e1 = new Edge( p1, p2 );
		Point p3 = new Point( 100, 0 );
		Point p4 = new Point( 0, 100 );
		Edge e2 = new Edge( p3, p4 );

		bool result = e1.HasIntersection( e2 );

		Assert.That( result, Is.True );
	}

	[Test]
	public void TryFindIntersection_EdgesIntersect_ReturnsCenterPoint() {
		Point p1 = new Point( 0, 0 );
		Point p2 = new Point( 100, 100 );
		Edge e1 = new Edge( p1, p2 );
		Point p3 = new Point( 100, 0 );
		Point p4 = new Point( 0, 100 );
		Edge e2 = new Edge( p3, p4 );

		bool result = e1.TryFindIntersection( e2, out Point intersection );

		Assert.That( result, Is.True );
		Assert.That( intersection.X, Is.EqualTo( 50 ) );
		Assert.That( intersection.Y, Is.EqualTo( 50 ) );
	}
}
