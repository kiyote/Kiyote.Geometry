using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class RoundedPolygonTests {
	[TestCase( 3, 729 )]
	[TestCase( 4, 6561 )]
	public void FromRoundedConvexVertices_ExhaustiveTinyInputs_MatchesExactGiftWrappedHull( int count, int samples ) {
		Point[] points = new Point[count];
		for( int sample = 0; sample < samples; sample++ ) {
			int code = sample;
			for( int i = 0; i < count; i++ ) {
				int position = code % 9;
				points[i] = new Point( position % 3 - 1, position / 3 - 1 );
				code /= 9;
			}
			AssertExactHull( points );
		}
	}

	[TestCase( 3 )]
	[TestCase( 4 )]
	[TestCase( 8 )]
	public void FromRoundedConvexVertices_SeededFullDomainInputs_MatchesExactGiftWrappedHull( int count ) {
		Random random = new Random( 1789 );
		Point[] points = new Point[count];
		for( int sample = 0; sample < 1000; sample++ ) {
			for( int i = 0; i < count; i++ ) {
				points[i] = new Point( random.Next( -32768, 32768 ), random.Next( -32768, 32768 ) );
			}
			AssertExactHull( points );
		}
	}

	[Test]
	public void FromRoundedConvexVertices_StarBoundary_UsesConvexHullInsteadOfLocalTurns() {
		Point[] points = [new( 0, -10 ), new( 6, 8 ), new( -10, -3 ), new( 10, -3 ), new( -6, 8 )];
		AssertExactHull( points );
	}

	[Test]
	public void FromRoundedConvexVertices_FullDomainQuadrilateral_PreservesCanonicalOrderAndOwnsPoints() {
		Point[] points = [new( 32767, 32767 ), new( 32767, -32768 ), new( -32768, -32768 ), new( -32768, 32767 )];
		Polygon polygon = Polygon.FromRoundedConvexVertices( points );
		points[0] = new Point( 0, 0 );
		Assert.That( polygon.Points, Is.EqualTo( new Point[] {
			new( -32768, -32768 ), new( 32767, -32768 ), new( 32767, 32767 ), new( -32768, 32767 )
		} ) );
	}

	[Test]
	public void FromRoundedConvexVertices_InvalidTinyInput_ValidatesBeforeFastPath() {
		Point[] points = [new( 0, 0 ), new( 32768, 0 ), new( 0, 10 )];
		Assert.That( () => Polygon.FromRoundedConvexVertices( points ), Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	private static void AssertExactHull( Point[] points ) {
		Point[] expected = GiftWrap( points );
		Polygon actual = Polygon.FromRoundedConvexVertices( points );
		Assert.That( actual.Points, Is.EqualTo( expected ) );
	}

	// Independent of the production sort-and-monotone-chain algorithm.
	private static Point[] GiftWrap( Point[] points ) {
		Point[] distinct = points.Distinct().OrderBy( point => point.X ).ThenBy( point => point.Y ).ToArray();
		if( distinct.Length < 3 ) {
			return [];
		}
		List<Point> hull = new List<Point>();
		int current = 0;
		do {
			hull.Add( distinct[current] );
			int next = (current + 1) % distinct.Length;
			for( int i = 0; i < distinct.Length; i++ ) {
				Int128 turn = Cross( distinct[current], distinct[next], distinct[i] );
				if( turn < 0 || (turn == 0 && Distance( distinct[current], distinct[i] ) > Distance( distinct[current], distinct[next] )) ) {
					next = i;
				}
			}
			current = next;
			Assert.That( hull.Count, Is.LessThanOrEqualTo( distinct.Length ) );
		} while( current != 0 );
		return hull.Count >= 3 ? hull.ToArray() : [];
	}

	private static Int128 Cross( Point a, Point b, Point c ) {
		return ((Int128)b.X - a.X) * ((Int128)c.Y - a.Y) - ((Int128)b.Y - a.Y) * ((Int128)c.X - a.X);
	}

	private static long Distance( Point a, Point b ) {
		long dx = (long)b.X - a.X;
		long dy = (long)b.Y - a.Y;
		return (dx * dx) + (dy * dy);
	}
}
