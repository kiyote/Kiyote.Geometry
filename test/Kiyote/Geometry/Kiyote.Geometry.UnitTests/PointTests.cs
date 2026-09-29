using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
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

	[Test]
	public void Deconstruct_ValidPoint_ComponentsReturned() {
		(int x, int y) = new Point( 3, -4 );

		Assert.That( x, Is.EqualTo( 3 ) );
		Assert.That( y, Is.EqualTo( -4 ) );
	}

	[Test]
	public void Operators_ValidPoints_MatchNamedMethods() {
		Point p1 = new Point( 10, 12 );
		Point p2 = new Point( 2, 3 );

		Assert.That( p1 + p2, Is.EqualTo( new Point( 12, 15 ) ) );
		Assert.That( p1 - p2, Is.EqualTo( new Point( 8, 9 ) ) );
		Assert.That( -p1, Is.EqualTo( new Point( -10, -12 ) ) );
		Assert.That( p1.Negate(), Is.EqualTo( new Point( -10, -12 ) ) );
	}

	[Test]
	public void MinMax_ValidPoints_ComponentWise() {
		Point p1 = new Point( 1, 9 );
		Point p2 = new Point( 5, 2 );

		Assert.That( Point.Min( p1, p2 ), Is.EqualTo( new Point( 1, 2 ) ) );
		Assert.That( Point.Max( p1, p2 ), Is.EqualTo( new Point( 5, 9 ) ) );
	}

	[Test]
	public void Distances_ValidPoints_ExpectedValues() {
		Point p1 = new Point( 1, 2 );
		Point p2 = new Point( 4, -2 );

		Assert.That( p1.ManhattanDistance( p2 ), Is.EqualTo( 7 ) );
		Assert.That( p1.ChebyshevDistance( p2 ), Is.EqualTo( 4 ) );
		Assert.That( p1.DistanceSquared( p2 ), Is.EqualTo( 25L ) );
	}

	[Test]
	public void DistanceSquared_ExtremeValues_DoesNotOverflow() {
		Point p1 = new Point( 0, 0 );
		Point p2 = new Point( int.MaxValue, 0 );

		long expected = (long)int.MaxValue * int.MaxValue;

		Assert.That( p1.DistanceSquared( p2 ), Is.EqualTo( expected ) );
	}

	[TestCase( 1, 0, false, true )]
	[TestCase( 1, 1, false, false )]
	[TestCase( 1, 1, true, true )]
	[TestCase( 0, 0, true, false )]
	[TestCase( 2, 0, true, false )]
	public void IsAdjacentTo_TestCases_ExpectedResult(
		int dx,
		int dy,
		bool includeDiagonals,
		bool expected
	) {
		Point p1 = new Point( 5, 5 );
		Point p2 = new Point( 5 + dx, 5 + dy );

		Assert.That( p1.IsAdjacentTo( p2, includeDiagonals ), Is.EqualTo( expected ) );
	}

	[TestCase( 0, 0, 0, 0, 0, 0 )]
	[TestCase( 31, 31, 0, 0, 31, 31 )]
	[TestCase( 32, 64, 1, 2, 0, 0 )]
	[TestCase( -1, -1, -1, -1, 31, 31 )]
	[TestCase( -32, -33, -1, -2, 0, 31 )]
	public void ShiftRightAnd_ChunkSize32_MapsToChunkAndLocal(
		int x,
		int y,
		int chunkX,
		int chunkY,
		int localX,
		int localY
	) {
		Point p = new Point( x, y );

		Assert.That( p.ShiftRight( 5 ), Is.EqualTo( new Point( chunkX, chunkY ) ) );
		Assert.That( p.And( 31 ), Is.EqualTo( new Point( localX, localY ) ) );
	}
}
