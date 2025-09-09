namespace Kiyote.Geometry.UnitTests;

[TestFixture]
public sealed class IntersectTests {

	[TestCase(0, 0, 0, 0, 0, 0, 10, 10)]
	[TestCase( 0, 0, 10, 10, 10, 10, 10, 10 )]
	public void HasIntersection_ZeroLengthLines_ReturnsFalse(
		int aX1,
		int aY1,
		int aX2,
		int aY2,
		int bX1,
		int bY1,
		int bX2,
		int bY2
	) {
		bool result = Intersect.HasIntersection( aX1, aY1, aX2, aY2, bX1, bY1, bX2, bY2 );
		Assert.That( result, Is.False );
	}

	[TestCase( 0, 0, 0, 0, 0, 0, 10, 10 )]
	[TestCase( 0, 0, 10, 10, 10, 10, 10, 10 )]
	public void TryFindIntersection_ZeroLengthLines_ReturnsFalse(
		int aX1,
		int aY1,
		int aX2,
		int aY2,
		int bX1,
		int bY1,
		int bX2,
		int bY2
	) {
		bool result = Intersect.TryFindIntersection( aX1, aY1, aX2, aY2, bX1, bY1, bX2, bY2, out Point intersection );
		Assert.That( result, Is.False );
		Assert.That( intersection, Is.Null );
	}

	[Test]
	public void HasIntersection_ParallelLines_ReturnsFalse() {
		bool result = Intersect.HasIntersection( 0, 0, 10, 10, 0, 0, 10, 10 );
		Assert.That( result, Is.False );
	}

	[Test]
	public void TryFindIntersection_ParallelLines_ReturnsFalse() {
		bool result = Intersect.TryFindIntersection( 0, 0, 10, 10, 0, 0, 10, 10, out Point intersection );
		Assert.That( result, Is.False );
		Assert.That( intersection, Is.Null );
	}

	[Test]
	public void HasIntersection_LinesDoNotIntersect_ReturnsFalse() {
		bool result = Intersect.HasIntersection( 0, 0, 10, 10, 10, 0, 15, 10 );
		Assert.That( result, Is.False );
	}

	[Test]
	public void TryFindIntersection_LinesDoNotIntersect_ReturnsFalse() {
		bool result = Intersect.TryFindIntersection( 0, 0, 10, 10, 10, 0, 15, 10, out Point intersection );
		Assert.That( result, Is.False );
		Assert.That( intersection, Is.Null );
	}

	[Test]
	public void HasIntersection_LinesIntersect_ReturnsTrue() {
		bool result = Intersect.HasIntersection( 0, 5, 10, 5, 5, 0, 5, 10 );
		Assert.That( result, Is.True );
	}

	[Test]
	public void TryeFindIntersection_LinesIntersect_ReturnsTrue() {
		bool result = Intersect.TryFindIntersection( 0, 5, 10, 5, 5, 0, 5, 10, out Point intersection );
		Assert.That( result, Is.True );
		Assert.That( intersection, Is.Not.Null );
		Assert.That( intersection.X, Is.EqualTo( 5 ) );
		Assert.That( intersection.Y, Is.EqualTo( 5 ) );
	}
}
