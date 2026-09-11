using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
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

	[Test]
	public void TryFindIntersection_EdgesDoNotIntersect_ReturnsFalse() {
		Edge e1 = new Edge( 0, 0, 10, 0 );
		Edge e2 = new Edge( 0, 50, 10, 50 );

		bool result = e1.TryFindIntersection( e2, out Point intersection );

		Assert.That( result, Is.False );
		Assert.That( intersection, Is.EqualTo( Point.None ) );
	}

	[Test]
	public void HasIntersection_EdgesDoNotIntersect_ReturnsFalse() {
		Edge e1 = new Edge( 0, 0, 10, 0 );
		Edge e2 = new Edge( 0, 50, 10, 50 );

		Assert.That( e1.HasIntersection( e2 ), Is.False );
	}

	[Test]
	public void Ctor_Coordinates_MatchesPointCtor() {
		Edge e = new Edge( 1, 2, 3, 4 );

		Assert.That( e.A, Is.EqualTo( new Point( 1, 2 ) ) );
		Assert.That( e.B, Is.EqualTo( new Point( 3, 4 ) ) );
	}

	[Test]
	public void None_DefaultEdge_UsesNonePoints() {
		Assert.That( Edge.None.A, Is.EqualTo( Point.None ) );
		Assert.That( Edge.None.B, Is.EqualTo( Point.None ) );
	}

	[Test]
	public void Normalize_Instance_ReturnsBoundingBoxExtents() {
		Edge e = new Edge( 10, 20, 40, 60 );

		Point normalized = e.Normalize();

		Assert.That( normalized.X, Is.EqualTo( 30 ) );
		Assert.That( normalized.Y, Is.EqualTo( 40 ) );
	}

	[Test]
	public void Normalize_InstanceReversedEdge_MatchesForwardEdge() {
		Edge forward = new Edge( 10, 20, 40, 60 );
		Edge reversed = new Edge( 40, 60, 10, 20 );

		Assert.That( reversed.Normalize(), Is.EqualTo( forward.Normalize() ) );
	}

	[Test]
	public void Equals_SameEdge_ReturnsTrue() {
		Edge e1 = new Edge( 1, 2, 3, 4 );
		object e2 = new Edge( 1, 2, 3, 4 );

		Assert.That( e1.Equals( e2 ), Is.True );
	}

	[Test]
	public void Equals_DifferentEdge_ReturnsFalse() {
		Edge e1 = new Edge( 1, 2, 3, 4 );
		object e2 = new Edge( 4, 3, 2, 1 );

		Assert.That( e1.Equals( e2 ), Is.False );
	}

	[Test]
	public void Equals_NotAnEdge_ReturnsFalse() {
		Edge e1 = new Edge( 1, 2, 3, 4 );

		Assert.That( e1.Equals( "not an edge" ), Is.False );
	}

	[Test]
	public void Equals_IEquatableSameEdge_ReturnsTrue() {
		IEquatable<Edge> e1 = new Edge( 1, 2, 3, 4 );

		Assert.That( e1.Equals( new Edge( 1, 2, 3, 4 ) ), Is.True );
	}

	[Test]
	public void Equals_IEquatableDifferentEdge_ReturnsFalse() {
		IEquatable<Edge> e1 = new Edge( 1, 2, 3, 4 );

		Assert.That( e1.Equals( new Edge( 4, 3, 2, 1 ) ), Is.False );
	}

	[Test]
	public void GetHashCode_EquivalentEdges_HashesMatch() {
		Edge e1 = new Edge( 1, 2, 3, 4 );
		Edge e2 = new Edge( 1, 2, 3, 4 );

		Assert.That( e1.GetHashCode(), Is.EqualTo( e2.GetHashCode() ) );
	}

	[Test]
	public void OperatorEquality_SameEdge_ReturnsTrue() {
		Assert.That( new Edge( 1, 2, 3, 4 ) == new Edge( 1, 2, 3, 4 ), Is.True );
	}

	[Test]
	public void OperatorEquality_DifferentEdge_ReturnsFalse() {
		Assert.That( new Edge( 1, 2, 3, 4 ) == new Edge( 4, 3, 2, 1 ), Is.False );
	}

	[Test]
	public void OperatorInequality_DifferentEdge_ReturnsTrue() {
		Assert.That( new Edge( 1, 2, 3, 4 ) != new Edge( 4, 3, 2, 1 ), Is.True );
	}

	[Test]
	public void OperatorInequality_SameEdge_ReturnsFalse() {
		Assert.That( new Edge( 1, 2, 3, 4 ) != new Edge( 1, 2, 3, 4 ), Is.False );
	}

	[Test]
	public void ToString_Edge_ContainsBothPoints() {
		Edge e = new Edge( 1, 2, 3, 4 );

		string result = e.ToString();

		Assert.That( result, Does.Contain( e.A.ToString() ) );
		Assert.That( result, Does.Contain( e.B.ToString() ) );
	}
}
