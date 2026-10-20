using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class RectRegressionTests {

	[Test]
	public void Overlaps_AdjacentRectangles_ReturnsFalseWithoutSharedCells() {
		Rect a = new Rect( 0, 0, 10, 10 );
		Rect b = new Rect( 10, 0, 10, 10 );
		Assert.That( a.Overlaps( b ), Is.False );
		Assert.That( b.Overlaps( a ), Is.False );
		Assert.That( a.Overlaps( (IRect)b ), Is.False );
		Assert.That( a.Touches( b ), Is.True );
	}

	[TestCase( 0, 0 )]
	[TestCase( 0, 20 )]
	[TestCase( 20, 0 )]
	public void Ctor_ZeroDimension_NormalizesAllFieldsThroughBothSizeOverloads( int width, int height ) {
		Rect rect = new Rect( int.MaxValue, int.MinValue, width, height );
		Assert.That( rect.Width, Is.Zero );
		Assert.That( rect.Height, Is.Zero );
		Assert.That( rect.X1, Is.Zero );
		Assert.That( rect.X2, Is.Zero );
		Assert.That( rect.Y1, Is.Zero );
		Assert.That( rect.Y2, Is.Zero );
		Assert.That( rect, Is.EqualTo( default( Rect ) ) );
		Assert.That( rect.IsEmpty, Is.True );
		Assert.That( ((IRect)rect).IsEmpty, Is.True );
		Assert.That( new Rect( 123, 456, new Point( width, height ) ), Is.EqualTo( default( Rect ) ) );
	}

	[TestCase( -1, 0 )]
	[TestCase( 0, -1 )]
	[TestCase( 10, -1 )]
	[TestCase( -1, 10 )]
	public void Ctor_NegativeDimension_ThrowsEvenWhenOtherDimensionIsZero( int width, int height ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => new Rect( 0, 0, width, height ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new Rect( 0, 0, new Point( width, height ) ) );
	}

	[Test]
	public void Contains_EmptyRectangle_ReturnsFalseThroughAllOverloads() {
		Rect empty = default;
		Rect occupied = new Rect( 0, 0, 1, 1 );
		IRect emptyInterface = empty;
		IRect occupiedInterface = occupied;
		Assert.That( empty.Contains( default( Point ) ), Is.False );
		Assert.That( empty.Contains( 0, 0 ), Is.False );
		Assert.That( emptyInterface.Contains( default( Point ) ), Is.False );
		Assert.That( emptyInterface.Contains( 0, 0 ), Is.False );
		Assert.That( empty.Contains( empty ), Is.False );
		Assert.That( empty.Contains( occupied ), Is.False );
		Assert.That( occupied.Contains( empty ), Is.False );
		Assert.That( emptyInterface.Contains( emptyInterface ), Is.False );
		Assert.That( emptyInterface.Contains( occupiedInterface ), Is.False );
		Assert.That( occupiedInterface.Contains( emptyInterface ), Is.False );
	}

	[Test]
	public void OverlapsAndTouchesAndTryIntersect_EmptyRectangle_ReturnFalseAndCanonicalEmptyIntersection() {
		Rect empty = default;
		foreach( Rect other in new[] { empty, new Rect( 0, 0, 1, 1 ), new Rect( -1, -1, 3, 3 ) } ) {
			Assert.That( empty.Overlaps( other ), Is.False );
			Assert.That( other.Overlaps( empty ), Is.False );
			Assert.That( empty.Overlaps( (IRect)other ), Is.False );
			Assert.That( other.Overlaps( (IRect)empty ), Is.False );
			Assert.That( empty.Touches( other ), Is.False );
			Assert.That( other.Touches( empty ), Is.False );
			Assert.That( empty.Touches( (IRect)other ), Is.False );
			Assert.That( other.Touches( (IRect)empty ), Is.False );
			Assert.That( empty.TryIntersect( other, out Rect intersection ), Is.False );
			Assert.That( intersection.Width, Is.Zero );
			Assert.That( intersection.Height, Is.Zero );
			Assert.That( Rect.TryIntersect( other, empty, out intersection ), Is.False );
			Assert.That( intersection, Is.EqualTo( default( Rect ) ) );
		}
	}

	[Test]
	public void UnionAndEquals_EmptyRectangle_TreatAsIdentityAndDistinguishFromOriginCell() {
		Rect empty = default;
		Rect occupied = new Rect( 100, 200, 2, 3 );
		Rect origin = new Rect( 0, 0, 1, 1 );
		Assert.That( origin.IsEmpty, Is.False );
		Assert.That( empty.Union( occupied ), Is.EqualTo( occupied ) );
		Assert.That( occupied.Union( empty ), Is.EqualTo( occupied ) );
		Assert.That( empty.Union( empty ), Is.EqualTo( empty ) );
		Assert.That( empty == origin, Is.False );
		Assert.That( empty != origin, Is.True );
		Assert.That( empty.Equals( (object)origin ), Is.False );
		Assert.That( ((IEquatable<Rect>)empty).Equals( origin ), Is.False );
		Assert.That( empty.IsEquivalentTo( origin ), Is.False );
		Assert.That( empty.IsEquivalentTo( (IRect)origin ), Is.False );
		Assert.That( origin.IsEquivalentTo( empty ), Is.False );
		Assert.That( origin.IsEquivalentTo( (IRect)empty ), Is.False );
		Assert.That( empty.IsEquivalentTo( empty ), Is.True );
		Assert.That( empty.IsEquivalentTo( (IRect)empty ), Is.True );
		Assert.That( empty.IsEquivalentTo( 0, 0, 0, 0 ), Is.False );
		Assert.That( empty.IsEquivalentTo( default( Point ), default( Point ) ), Is.False );
		Assert.That( origin.IsEquivalentTo( 0, 0, 0, 0 ), Is.True );
		Assert.That( new HashSet<Rect> { empty, origin }.Count, Is.EqualTo( 2 ) );
	}

	[TestCase( int.MaxValue, int.MaxValue )]
	[TestCase( int.MinValue, int.MinValue )]
	public void Ctor_FullIntDomainSingleCells_PreservesBoundsAndSelfIntersection( int x, int y ) {
		Rect cell = new Rect( x, y, 1, 1 );
		Assert.That( cell.X2, Is.EqualTo( x ) );
		Assert.That( cell.Y2, Is.EqualTo( y ) );
		Assert.That( cell.Contains( new Point( x, y ) ), Is.True );
		Assert.That( cell.Touches( cell ), Is.True );
		Assert.That( cell.Touches( (IRect)cell ), Is.True );
		Assert.That( cell.Overlaps( cell ), Is.True );
		Assert.That( cell.TryIntersect( cell, out Rect result ), Is.True );
		Assert.That( result, Is.EqualTo( cell ) );
	}

	[Test]
	public void Touches_IntDomainExtremes_RejectsWraparoundAndIncludesCornerAdjacency() {
		Rect high = new Rect( int.MaxValue, int.MaxValue, 1, 1 );
		Rect low = new Rect( int.MinValue, int.MinValue, 1, 1 );
		Rect nearHigh = new Rect( int.MaxValue - 1, int.MaxValue - 1, 1, 1 );
		Assert.That( high.Touches( low ), Is.False );
		Assert.That( low.Touches( high ), Is.False );
		Assert.That( high.Touches( (IRect)low ), Is.False );
		Assert.That( low.Touches( (IRect)high ), Is.False );
		Assert.That( high.Touches( nearHigh ), Is.True );
		Assert.That( nearHigh.Touches( high ), Is.True );
		Assert.That( high.Touches( (IRect)nearHigh ), Is.True );
		Assert.That( nearHigh.Touches( (IRect)high ), Is.True );
		Assert.That( high.Overlaps( nearHigh ), Is.False );
	}

	[TestCase( int.MaxValue, 0, 2, 1 )]
	[TestCase( 0, int.MaxValue, 1, 2 )]
	[TestCase( 2, 0, int.MaxValue, 1 )]
	public void Ctor_UnrepresentableEndpoints_ThrowsArgumentOutOfRangeException( int x, int y, int width, int height ) {
		Assert.Throws<ArgumentOutOfRangeException>( () => new Rect( x, y, width, height ) );
	}

	[Test]
	public void CtorAndUnion_UnrepresentableDimensions_ThrowArgumentOutOfRangeException() {
		Point low = new Point( int.MinValue, int.MinValue );
		Point high = new Point( int.MaxValue, int.MaxValue );
		Assert.Throws<ArgumentOutOfRangeException>( () => new Rect( low, high ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new Rect( high, low ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => new Rect( 0, 0, 1, 1 ).Union( new Rect( int.MaxValue, 0, 1, 1 ) ) );
	}

	[Test]
	public void Ctor_LargestRepresentableDimensions_DoesNotOverflowIntermediateArithmetic() {
		Rect rect = new Rect( 1, 1, int.MaxValue, int.MaxValue );
		Assert.That( rect.X2, Is.EqualTo( int.MaxValue ) );
		Assert.That( rect.Y2, Is.EqualTo( int.MaxValue ) );
		Assert.That( rect, Is.EqualTo( new Rect( new Point( 1, 1 ), new Point( int.MaxValue, int.MaxValue ) ) ) );
	}

	[Test]
	public void ContainsAndOverlapsAndTouchesAndTryIntersect_SeededSmallRectangles_MatchOccupiedCellReference() {
		Random random = new Random( 7291 );
		for( int i = 0; i < 300; i++ ) {
			Rect a = NextRect();
			Rect b = NextRect();
			HashSet<Point> first = Cells( a );
			HashSet<Point> second = Cells( b );
			bool overlaps = first.Overlaps( second );
			bool contains = second.Count != 0 && second.IsSubsetOf( first );
			bool touches = first.Any( p => second.Any( q => Math.Abs( p.X - q.X ) <= 1 && Math.Abs( p.Y - q.Y ) <= 1 ) );
			string input = $"A=({a.X1},{a.Y1},{a.Width},{a.Height}), B=({b.X1},{b.Y1},{b.Width},{b.Height})";
			Assert.That( a.Overlaps( b ), Is.EqualTo( overlaps ), input );
			Assert.That( a.Overlaps( (IRect)b ), Is.EqualTo( overlaps ), input );
			Assert.That( b.Overlaps( a ), Is.EqualTo( overlaps ), input );
			Assert.That( a.Contains( b ), Is.EqualTo( contains ), input );
			Assert.That( a.Contains( (IRect)b ), Is.EqualTo( contains ), input );
			Assert.That( a.Touches( b ), Is.EqualTo( touches ), input );
			Assert.That( a.Touches( (IRect)b ), Is.EqualTo( touches ), input );
			Assert.That( b.Touches( a ), Is.EqualTo( touches ), input );
			Assert.That( a.TryIntersect( b, out Rect intersection ), Is.EqualTo( overlaps ), input );
			first.IntersectWith( second );
			Assert.That( Cells( intersection ), Is.EquivalentTo( first ), input );
		}

		Rect NextRect() => new Rect( random.Next( -5, 6 ), random.Next( -5, 6 ), random.Next( 0, 5 ), random.Next( 0, 5 ) );
	}

	private static HashSet<Point> Cells( Rect rect ) {
		HashSet<Point> result = [];
		for( int y = 0; y < rect.Height; y++ ) {
			for( int x = 0; x < rect.Width; x++ ) {
				result.Add( new Point( rect.X1 + x, rect.Y1 + y ) );
			}
		}
		return result;
	}
}
