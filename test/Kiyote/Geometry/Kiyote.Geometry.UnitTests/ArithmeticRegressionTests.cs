using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class ArithmeticRegressionTests {

	[TestCase( -25000, -25000, 25000, 25000, -25000, 25000, 25000, -25000, true, true, 0, 0 )]
	[TestCase( -32768, -32768, 32767, 32767, -32768, 32767, 32767, -32768, true, true, 0, 0 )]
	[TestCase( 0, 0, 10, 0, 5, 0, 15, 0, true, false, 0, 0 )]
	[TestCase( 0, 0, 10, 10, 10, 10, 20, 20, true, true, 10, 10 )]
	[TestCase( 0, 0, 10, 0, 11, 0, 20, 0, false, false, 0, 0 )]
	[TestCase( 2, 2, 2, 2, 0, 0, 4, 4, true, true, 2, 2 )]
	[TestCase( 2, 3, 2, 3, 0, 0, 4, 4, false, false, 0, 0 )]
	[TestCase( 2, 2, 2, 2, 2, 2, 2, 2, true, true, 2, 2 )]
	[TestCase( 2, 2, 2, 2, 3, 3, 3, 3, false, false, 0, 0 )]
	[TestCase( 0, 0, 0, 10, 0, 5, 0, 15, true, false, 0, 0 )]
	[TestCase( 0, 10, 10, 0, 2, 8, 8, 2, true, false, 0, 0 )]
	[TestCase( 0, 0, 1, 1, 0, 1, 1, 0, true, true, 0, 0 )]
	[TestCase( 1, 1, 2, 2, 1, 2, 2, 1, true, true, 2, 2 )]
	[TestCase( -2, -2, -1, -1, -2, -1, -1, -2, true, true, -2, -2 )]
	[TestCase( 0, 0, 1, 0, 1, 1, 2, -2, false, false, 0, 0 )]
	public void Intersection_ContactsAndRounding_AreSymmetric(
		int ax, int ay, int bx, int by,
		int cx, int cy, int dx, int dy,
		bool intersects, bool unique, int x, int y
	) {
		Edge first = new Edge( ax, ay, bx, by );
		Edge second = new Edge( cx, cy, dx, dy );
		AssertIntersection( first, second, intersects, unique, new Point( x, y ) );
		AssertIntersection( second, first, intersects, unique, new Point( x, y ) );
		AssertIntersection( new Edge( first.B, first.A ), second, intersects, unique, new Point( x, y ) );
		AssertIntersection( first, new Edge( second.B, second.A ), intersects, unique, new Point( x, y ) );
	}

	[Test]
	public void Magnitude_AndDistances_SupportBothAxisExtremes() {
		Point low = new Point( -32768, -32768 );
		Point high = new Point( 32767, 32767 );
		Edge horizontal = new Edge( -25000, 0, 25000, 0 );

		Assert.That( horizontal.Magnitude(), Is.EqualTo( 50000f ) );
		Assert.That( Edge.Magnitude( horizontal.A, horizontal.B ), Is.EqualTo( 50000f ) );
		Assert.That( low.DistanceSquared( high ), Is.EqualTo( 8589672450L ) );
		Assert.That( low.ManhattanDistance( high ), Is.EqualTo( 131070 ) );
		Assert.That( low.ChebyshevDistance( high ), Is.EqualTo( 65535 ) );
		Assert.That( new Edge( low, high ).Magnitude(), Is.EqualTo( (float)Math.Sqrt( 8589672450L ) ) );
		Assert.That( new Edge( low, high ).Normalize(), Is.EqualTo( new Point( 65535, 65535 ) ) );
	}

	[TestCase( 0f, 0, 10 )]
	[TestCase( 0.25f, 2, 8 )]
	[TestCase( 0.5f, 5, 5 )]
	[TestCase( 0.75f, 8, 2 )]
	[TestCase( 1f, 10, 0 )]
	public void Interpolation_PreservesDirectionAndRoundsToEven( float amount, int x, int y ) {
		Edge edge = new Edge( 0, 10, 10, 0 );
		Point expected = new Point( x, y );
		Assert.That( edge.Interpolate( amount ), Is.EqualTo( expected ) );
		Assert.That( Edge.Interpolate( edge.A, edge.B, amount ), Is.EqualTo( expected ) );
		Assert.That( new Edge( edge.B, edge.A ).Interpolate( 1f - amount ), Is.EqualTo( expected ) );
	}

	[TestCase( -0.1f )]
	[TestCase( 1.1f )]
	[TestCase( float.NaN )]
	[TestCase( float.PositiveInfinity )]
	[TestCase( float.NegativeInfinity )]
	public void Interpolation_InvalidAmount_Throws( float amount ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => new Edge( 0, 0, 10, 10 ).Interpolate( amount ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Edge.Interpolate( default, new Point( 10, 10 ), amount ) );
	}

	[Test]
	public void Interpolation_NegativeHalfTies_AndEndpoints() {
		Assert.That( new Edge( -2, -1, -1, 0 ).Interpolate( 0.5f ), Is.EqualTo( new Point( -2, 0 ) ) );
		Edge edge = new Edge( -32768, 32767, 32767, -32768 );
		Assert.That( edge.Interpolate( 0f ), Is.EqualTo( edge.A ) );
		Assert.That( edge.Interpolate( 1f ), Is.EqualTo( edge.B ) );
		Assert.That( edge.Interpolate( 0.5f ), Is.EqualTo( default( Point ) ) );
	}

	[TestCase( -32769, 0 )]
	[TestCase( 32768, 0 )]
	[TestCase( 0, -32769 )]
	[TestCase( 0, 32768 )]
	[TestCase( int.MinValue, int.MaxValue )]
	public void GeometryAlgorithms_RejectOutsideCoordinates( int x, int y ) {
		Point outside = new Point( x, y );
		Point origin = default;
		Edge invalid = new Edge( outside, origin );
		Edge valid = new Edge( 0, 0, 1, 1 );
		Assert.Throws<ArgumentOutOfRangeException>( () => outside.DistanceSquared( origin ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => origin.DistanceSquared( outside ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => outside.ManhattanDistance( origin ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => origin.ManhattanDistance( outside ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => outside.ChebyshevDistance( origin ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => origin.ChebyshevDistance( outside ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => outside.IsAdjacentTo( origin, true ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => origin.IsAdjacentTo( outside, false ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => invalid.Magnitude() );
		Assert.Throws<ArgumentOutOfRangeException>( () => new Edge( origin, outside ).Magnitude() );
		Assert.Throws<ArgumentOutOfRangeException>( () => invalid.Interpolate( 0f ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new Edge( origin, outside ).Interpolate( 0f ) );
		foreach( Edge candidate in new[] { invalid, new Edge( origin, outside ) } ) {
			Assert.Throws<ArgumentOutOfRangeException>( () => candidate.HasIntersection( valid ) );
			Assert.Throws<ArgumentOutOfRangeException>( () => valid.HasIntersection( candidate ) );
			Assert.Throws<ArgumentOutOfRangeException>( () => candidate.TryFindIntersection( valid, out _ ) );
			Assert.Throws<ArgumentOutOfRangeException>( () => valid.TryFindIntersection( candidate, out _ ) );
		}
		Assert.Throws<ArgumentOutOfRangeException>( () => new Polygon( [outside, origin, new Point( 1, 1 )] ) );
		Polygon polygon = new Polygon( [origin, new Point( 1, 0 ), new Point( 0, 1 )] );
		Assert.Throws<ArgumentOutOfRangeException>( () => polygon.Contains( outside ) );
	}

	[Test]
	public void SharedCoordinateArithmetic_UsesFullIntDomainWithoutWraparound() {
		Point high = new Point( int.MaxValue, int.MaxValue );
		Point low = new Point( int.MinValue, int.MinValue );
		Assert.That( high.Add( -1, -1 ), Is.EqualTo( new Point( int.MaxValue - 1, int.MaxValue - 1 ) ) );
		Assert.That( low.Subtract( -1, -1 ), Is.EqualTo( new Point( int.MinValue + 1, int.MinValue + 1 ) ) );
		Assert.Throws<OverflowException>( () => high.Add( 1, 0 ) );
		Assert.Throws<OverflowException>( () => high.Add( 0, 1 ) );
		Assert.Throws<OverflowException>( () => low.Subtract( 1, 0 ) );
		Assert.Throws<OverflowException>( () => low.Subtract( 0, 1 ) );
		Assert.Throws<OverflowException>( () => low.Negate() );
		Assert.Throws<OverflowException>( () => _ = high + new Point( 1, 1 ) );
		Assert.Throws<OverflowException>( () => _ = low - new Point( 1, 1 ) );
		Assert.Throws<OverflowException>( () => _ = -low );
		Assert.Throws<OverflowException>( () => Edge.GetBoundingBox( low, high ) );
		Assert.Throws<OverflowException>( () => new Edge( low, high ).Normalize() );
	}

	[Test]
	public void PolygonOrientation_SupportedLargeTriangle_DoesNotOverflow() {
		Polygon triangle = new Polygon( [
			new Point( -32768, -32768 ),
			new Point( 32767, -32768 ),
			new Point( 32767, 32767 )
		] );
		Assert.That( triangle.Contains( new Point( 0, -32768 ) ), Is.True );
		Assert.That( triangle.Contains( new Point( 32766, 32765 ) ), Is.True );
		Assert.That( triangle.Contains( new Point( -32767, 32766 ) ), Is.False );
	}

	[Test]
	public void PolygonOrientation_SupportedLargeSquare_ContainsNearLeftEdge() {
		Polygon square = new Polygon( [
			new Point( -32768, -32768 ), new Point( 32767, -32768 ),
			new Point( 32767, 32767 ), new Point( -32768, 32767 )
		] );
		Assert.That( square.Contains( new Point( -32767, 0 ) ), Is.True );
	}

	[Test]
	public void BoundedSeededSegments_MatchDecimalReferenceAndTranslation() {
		Random random = new Random( 7291 );
		for( int i = 0; i < 500; i++ ) {
			Edge a = new Edge( NextPoint(), NextPoint() );
			Edge b = new Edge( NextPoint(), NextPoint() );
			bool expected = ReferenceIntersects( a, b );
			Assert.That( a.HasIntersection( b ), Is.EqualTo( expected ), $"{a}, {b}" );
			Assert.That( b.HasIntersection( a ), Is.EqualTo( expected ), $"{a}, {b}" );
			if( expected ) {
				decimal denominator = ( (decimal)a.B.X - a.A.X ) * ( b.B.Y - b.A.Y )
					- ( (decimal)a.B.Y - a.A.Y ) * ( b.B.X - b.A.X );
				if( denominator != 0 ) {
					decimal numerator = ( (decimal)b.A.X - a.A.X ) * ( b.B.Y - b.A.Y )
						- ( (decimal)b.A.Y - a.A.Y ) * ( b.B.X - b.A.X );
					int x = (int)decimal.Round( ( a.A.X * denominator + numerator * ( a.B.X - a.A.X ) ) / denominator );
					int y = (int)decimal.Round( ( a.A.Y * denominator + numerator * ( a.B.Y - a.A.Y ) ) / denominator );
					AssertIntersection( a, b, true, true, new Point( x, y ) );
					AssertIntersection( b, a, true, true, new Point( x, y ) );
				}
			}
			Point translation = new Point( 100, -200 );
			Assert.That(
				new Edge( a.A + translation, a.B + translation )
					.HasIntersection( new Edge( b.A + translation, b.B + translation ) ),
				Is.EqualTo( expected ), $"{a}, {b}"
			);
			Assert.That( a.A.DistanceSquared( a.B ), Is.EqualTo( a.B.DistanceSquared( a.A ) ), $"{a}" );
		}

		Point NextPoint() => new Point( random.Next( -32000, 32001 ), random.Next( -32000, 32001 ) );
	}

	private static void AssertIntersection( Edge a, Edge b, bool intersects, bool unique, Point expected ) {
		string input = $"{a}, {b}";
		Assert.That( a.HasIntersection( b ), Is.EqualTo( intersects ), input );
		Assert.That( Intersect.HasIntersection( a.A.X, a.A.Y, a.B.X, a.B.Y, b.A.X, b.A.Y, b.B.X, b.B.Y ), Is.EqualTo( intersects ), input );
		Assert.That( a.TryFindIntersection( b, out Point actual ), Is.EqualTo( unique ), input );
		Assert.That( actual, Is.EqualTo( unique ? expected : Point.None ), input );
		Assert.That( Intersect.TryFindIntersection( a.A.X, a.A.Y, a.B.X, a.B.Y, b.A.X, b.A.Y, b.B.X, b.B.Y, out int x, out int y ), Is.EqualTo( unique ), input );
		Assert.That( new Point( x, y ), Is.EqualTo( unique ? expected : default ), input );
	}

	private static bool ReferenceIntersects( Edge a, Edge b ) {
		decimal abC = Orientation( a.A, a.B, b.A );
		decimal abD = Orientation( a.A, a.B, b.B );
		decimal cdA = Orientation( b.A, b.B, a.A );
		decimal cdB = Orientation( b.A, b.B, a.B );
		return ( abC == 0 && OnSegment( a, b.A ) )
			|| ( abD == 0 && OnSegment( a, b.B ) )
			|| ( cdA == 0 && OnSegment( b, a.A ) )
			|| ( cdB == 0 && OnSegment( b, a.B ) )
			|| ( Math.Sign( abC ) != Math.Sign( abD ) && abC != 0 && abD != 0
				&& Math.Sign( cdA ) != Math.Sign( cdB ) && cdA != 0 && cdB != 0 );
	}

	private static decimal Orientation( Point a, Point b, Point c ) {
		return ( (decimal)b.X - a.X ) * ( c.Y - a.Y ) - ( (decimal)b.Y - a.Y ) * ( c.X - a.X );
	}

	private static bool OnSegment( Edge edge, Point point ) {
		return point.X >= Math.Min( edge.A.X, edge.B.X ) && point.X <= Math.Max( edge.A.X, edge.B.X )
			&& point.Y >= Math.Min( edge.A.Y, edge.B.Y ) && point.Y <= Math.Max( edge.A.Y, edge.B.Y );
	}
}
