namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class EdgeTests {

	[TestCase(0, 9, 5, 9, 0, 9, 5, 9, true)]
	[TestCase( 0, 9, 5, 9, 5, 9, 0, 9, true )]
	[TestCase( 5, 9, 0, 9, 0, 9, 5, 9, true )]
	[TestCase( 0, 5, 9, 9, 0, 9, 5, 5, false )]
	public void IsEquivalentTo(
		int ax1,
		int ay1,
		int ax2,
		int ay2,
		int bx1,
		int by1,
		int bx2,
		int by2,
		bool expected
	) {
		Edge a = new Edge( ax1, ay1, ax2, ay2 );
		Edge b = new Edge( bx1, by1, bx2, by2 );

		Assert.That( a.IsEquivalentTo( b ), Is.EqualTo( expected ) );
	}

	[Test]
	public void BoundingBox_EdgeNotTopLeftBottomRight_CorrectBoundingBox() {
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
