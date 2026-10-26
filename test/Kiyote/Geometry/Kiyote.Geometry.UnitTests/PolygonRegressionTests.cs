using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class PolygonRegressionTests {

	[Test]
	public void Contains_PartialContainedAndEmptyTargets_ReturnsTrueOnlyForWholeNonemptyTargets() {
		Polygon outer = Box( 0, 0, 10, 10 );
		Assert.That( outer.Contains( Box( 5, 5, 15, 15 ) ), Is.False );
		Assert.That( outer.Contains( Box( 2, 2, 4, 4 ) ), Is.True );
		Assert.That( outer.Contains( outer ), Is.True );
		Assert.That( outer.Contains( Polygon.None ), Is.False );
		Assert.That( Polygon.None.Contains( outer ), Is.False );
		Assert.That( Polygon.None.Contains( Polygon.None ), Is.False );
		Assert.That( outer.Contains( Array.Empty<Point>().AsEnumerable() ), Is.False );
	}

	[Test]
	public void TryIntersect_FullContainment_ReturnsInnerPolygonWithoutBoundaryCrossings() {
		Polygon outer = Box( 0, 0, 10, 10 );
		Polygon inner = Box( 2, 2, 4, 4 );
		Assert.That( outer.HasIntersection( inner ), Is.True );
		Assert.That( inner.HasIntersection( outer ), Is.True );
		Assert.That( outer.TryIntersect( inner, out Polygon result ), Is.True );
		AssertVertices( result, [new( 2, 2 ), new( 4, 2 ), new( 4, 4 ), new( 2, 4 )], 8 );
		Assert.That( inner.TryIntersect( outer, out result ), Is.True );
		AssertVertices( result, [new( 2, 2 ), new( 4, 2 ), new( 4, 4 ), new( 2, 4 )], 8 );
		Assert.That( outer.TryFindBoundaryIntersections( inner, out IReadOnlyList<Point> boundary ), Is.False );
		Assert.That( boundary, Is.Empty );
	}

	[TestCase( 10, 0 )]
	[TestCase( 10, 10 )]
	public void TryIntersect_ContactWithoutArea_ReturnsFalseAndNone( int x, int y ) {
		Polygon a = Box( 0, 0, 10, 10 );
		Polygon b = Box( x, y, x + 10, y + 10 );
		Assert.That( a.HasIntersection( b ), Is.True );
		Assert.That( b.HasIntersection( a ), Is.True );
		Assert.That( a.HasOverlap( b ), Is.True );
		Assert.That( a.TryIntersect( b, out Polygon result ), Is.False );
		Assert.That( result, Is.SameAs( Polygon.None ) );
		Assert.That( b.TryIntersect( a, out result ), Is.False );
		Assert.That( result, Is.SameAs( Polygon.None ) );
	}

	[Test]
	public void TryIntersect_IdenticalAndEmptyPolygons_ReturnsOriginalOrNoneRespectively() {
		Polygon box = Box( 0, 0, 10, 10 );
		Assert.That( box.TryIntersect( box, out Polygon result ), Is.True );
		AssertVertices( result, [new( 0, 0 ), new( 10, 0 ), new( 10, 10 ), new( 0, 10 )], 200 );
		Assert.That( box.TryIntersect( Polygon.None, out result ), Is.False );
		Assert.That( result, Is.SameAs( Polygon.None ) );
		Assert.That( Polygon.None.TryIntersect( box, out result ), Is.False );
		Assert.That( result, Is.SameAs( Polygon.None ) );
		Assert.That( Polygon.None.HasIntersection( box ), Is.False );
		Assert.That( Polygon.None.HasIntersection( Polygon.None ), Is.False );
	}

	[Test]
	public void IsEquivalentTo_CoordinateKeyCollisions_ReturnsFalseForDifferentTriangles() {
		Polygon a = new Polygon( [new( 0, 0 ), new( 1, 0 ), new( 0, 2 )] );
		Polygon b = new Polygon( [new( 0, 0 ), new( 0, 1 ), new( 2, 0 )] );
		Assert.That( a.IsEquivalentTo( b ), Is.False );
		Assert.That( b.IsEquivalentTo( a ), Is.False );
	}

	[Test]
	public void Ctor_RedundantBoundary_NormalizesWithoutDependingOnStartOrWinding() {
		Point[] input = [
			new( 5, 0 ), new( 10, 0 ), new( 10, 0 ), new( 10, 10 ),
			new( 0, 10 ), new( 0, 0 ), new( 3, 0 ), new( 5, 0 ), new( 5, 0 )
		];
		Point[] corners = [new( 0, 0 ), new( 10, 0 ), new( 10, 10 ), new( 0, 10 )];
		Polygon normalized = new Polygon( input );
		AssertVertices( normalized, corners, 200 );
		for( int start = 0; start < corners.Length; start++ ) {
			Point[] rotated = Enumerable.Range( 0, corners.Length )
				.Select( i => corners[( start + i ) % corners.Length] ).ToArray();
			Polygon candidate = new Polygon( rotated );
			Assert.That( normalized.IsEquivalentTo( candidate ), Is.True );
			Array.Reverse( rotated );
			Assert.That( normalized.IsEquivalentTo( new Polygon( rotated ) ), Is.True );
		}
		Assert.That( new Polygon( [] ).IsEquivalentTo( Polygon.None ), Is.True );
		Assert.That( Polygon.None.IsEquivalentTo( normalized ), Is.False );
	}

	[TestCaseSource( nameof( InvalidBoundaries ) )]
	public void Ctor_InvalidNonemptyBoundary_ThrowsArgumentException( Point[] points ) {
		Assert.Throws<ArgumentException>( () => new Polygon( points ) );
	}

	private static IEnumerable<Point[]> InvalidBoundaries() {
		yield return [new( 0, 0 )];
		yield return [new( 0, 0 ), new( 1, 1 )];
		yield return [new( 0, 0 ), new( 1, 1 ), new( 2, 2 )];
		yield return [new( 0, 0 ), new( 0, 0 ), new( 0, 0 )];
		yield return [new( 0, 0 ), new( 6, 0 ), new( 6, 6 ), new( 3, 2 ), new( 0, 6 )];
		yield return [new( 0, 0 ), new( 4, 4 ), new( 0, 4 ), new( 4, 0 )];
		yield return [new( 0, 0 ), new( 10, 0 ), new( 5, 0 ), new( 10, 10 ), new( 0, 10 )];
		yield return [new( 0, 0 ), new( 4, 0 ), new( 4, 4 ), new( 0, 0 ), new( 0, 4 )];
		yield return [new( 0, 0 ), new( 4, 0 ), new( 4, 4 ), new( 0, 4 ),
			new( 0, 0 ), new( 4, 0 ), new( 4, 4 ), new( 0, 4 )];
		Point[] pentagon = Regular( 5 );
		yield return [pentagon[0], pentagon[2], pentagon[4], pentagon[1], pentagon[3]];
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Ctor_MutableInput_OwnsSnapshotAndExposesReadOnlyStorage( bool useList ) {
		Point[] input = [new( 0, 0 ), new( 10, 0 ), new( 10, 10 ), new( 0, 10 )];
		IList<Point> source = useList ? input.ToList() : input;
		Polygon polygon = new Polygon( (IReadOnlyList<Point>)source );
		source[0] = new Point( 100, 100 );
		Assert.That( polygon.Points[0], Is.EqualTo( default( Point ) ) );
		Assert.That( polygon.Contains( new Point( 1, 1 ) ), Is.True );
		if( polygon.Points is IList<Point> exposed ) {
			Assert.That( exposed.IsReadOnly, Is.True );
			Assert.Throws<NotSupportedException>( () => exposed[0] = new Point( 100, 100 ) );
		}
		for( int i = 0; i < polygon.Points.Count; i++ ) {
			Assert.That( polygon.Edges[i], Is.EqualTo( new Edge( polygon.Points[i], polygon.Points[( i + 1 ) % polygon.Points.Count] ) ) );
		}
	}

	[Test]
	public void Ctor_RedundantAndOverLimitBoundaries_AppliesVertexLimitAfterNormalization() {
		Assert.That( Polygon.MaximumVertices, Is.EqualTo( 128 ) );
		Assert.That( new Polygon( Regular( 128 ) ).Points, Has.Count.EqualTo( 128 ) );
		Assert.Throws<ArgumentException>( () => new Polygon( Regular( 129 ) ) );
		List<Point> redundant = [];
		for( int i = 0; i < 1000; i++ ) {
			redundant.Add( new Point( i, 0 ) );
		}
		redundant.Add( new Point( 999, 10 ) );
		redundant.Add( new Point( 0, 10 ) );
		Assert.That( new Polygon( redundant ).Points, Has.Count.EqualTo( 4 ) );
	}

	[Test]
	public void Ctor_MaximumBoundary_NormalizesBothSidesOfClosingSeam() {
		Point[] corners = Regular( Polygon.MaximumVertices );
		Assert.That( corners[0], Is.EqualTo( new Point( 20000, 0 ) ) );
		Assert.That( corners[1], Is.EqualTo( new Point( 19976, 981 ) ) );
		List<Point> input = [new Point( 19984, 654 )];
		input.AddRange( corners.Skip( 1 ) );
		input.Add( corners[0] );
		input.Add( new Point( 19992, 327 ) );
		Assert.That( new Polygon( input ).Points, Is.EquivalentTo( corners ) );
		input.Add( input[0] );
		Assert.That( new Polygon( input ).Points, Is.EquivalentTo( corners ) );
	}

	[Test]
	public void HasIntersectionAndTryFindBoundaryIntersections_InvalidEdges_ValidateBeforeEarlyReturn() {
		Polygon box = Box( 0, 0, 10, 10 );
		Edge inside = new Edge( 2, 2, 4, 4 );
		Edge invalid = new Edge( 32768, 0, 32768, 1 );
		Assert.Throws<ArgumentOutOfRangeException>( () => box.HasIntersection( [inside, invalid] ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Polygon.None.HasIntersection( invalid ) );
		Assert.Throws<ArgumentOutOfRangeException>( () => Polygon.None.TryFindBoundaryIntersections( invalid, out _ ) );
		Assert.Throws<ArgumentException>( () => box.HasIntersection( default( System.Collections.Immutable.ImmutableArray<Edge> ) ) );
	}

	[Test]
	public void TryIntersect_OverLimitResult_ThrowsWithoutChangingInputs() {
		Polygon a = new Polygon( Regular( 128 ) );
		Polygon b = new Polygon( Regular( 128, Math.PI / 128 ) );
		Assert.Throws<InvalidOperationException>( () => a.TryIntersect( b, out _ ) );
		Assert.That( a.Points, Has.Count.EqualTo( 128 ) );
		Assert.That( b.Points, Has.Count.EqualTo( 128 ) );
	}

	[Test]
	public void TryIntersect_ResultExactlyAtVertexLimit_ReturnsPolygonAtLimit() {
		Polygon a = new Polygon( Regular( 64 ) );
		Polygon b = new Polygon( Regular( 64, Math.PI / 64 ) );
		Assert.That( a.TryIntersect( b, out Polygon result ), Is.True );
		Assert.That( result.Points, Has.Count.EqualTo( 128 ) );
	}

	[Test]
	public void TryIntersect_FractionalCoordinates_RoundsOnlyForOutput() {
		Polygon box = Box( 0, 0, 4, 4 );
		Polygon triangle = new Polygon( [new( -1, 1 ), new( 5, 1 ), new( 2, 5 )] );
		Assert.That( box.TryIntersect( triangle, out Polygon result ), Is.True );
		AssertVertices( result, [new( 0, 1 ), new( 4, 1 ), new( 4, 2 ), new( 3, 4 ), new( 1, 4 ), new( 0, 2 )], 20 );
	}

	[TestCase( 1 )]
	[TestCase( -1 )]
	public void TryIntersect_HalfTies_RoundsToEven( int sign ) {
		Polygon box = new Polygon( new Point[] { new( 0, 0 ), new( 4, 0 ), new( 4, 4 ), new( 0, 4 ) }
			.Select( p => new Point( sign * p.X, sign * p.Y ) ).ToArray() );
		Polygon triangle = new Polygon( new Point[] { new( -1, -1 ), new( 5, 3 ), new( -1, 7 ) }
			.Select( p => new Point( sign * p.X, sign * p.Y ) ).ToArray() );
		Assert.That( box.TryIntersect( triangle, out Polygon result ), Is.True );
		Point[] expected = new Point[] { new( 0, 0 ), new( 4, 2 ), new( 4, 4 ), new( 0, 4 ) }
			.Select( p => new Point( sign * p.X, sign * p.Y ) ).ToArray();
		AssertVertices( result, expected, 24 );
	}

	[Test]
	public void TryIntersect_QuantizationCollapse_ReturnsNone() {
		Polygon a = new Polygon( [new( 0, 0 ), new( 10, 0 ), new( 0, 1 )] );
		Polygon b = new Polygon( [new( 0, 1 ), new( 9, 1 ), new( 9, 0 )] );
		Assert.That( a.HasIntersection( b ), Is.True );
		Assert.That( a.TryIntersect( b, out Polygon result ), Is.False );
		Assert.That( result, Is.SameAs( Polygon.None ) );
	}

	[Test]
	public void TryIntersect_SnappingWouldIntroduceConcavity_ReturnsConvexHull() {
		Polygon a = new Polygon( [new( -8, -3 ), new( 6, -7 ), new( 5, 6 ), new( 3, 7 )] );
		Polygon b = new Polygon( [new( -2, 6 ), new( 2, 0 ), new( 8, -8 ), new( 5, 5 ), new( -2, 7 )] );
		Assert.That( a.TryIntersect( b, out Polygon result ), Is.True );
		AssertVertices( result, [new( -1, 4 ), new( 2, 0 ), new( 6, -5 ), new( 5, 5 ), new( 2, 6 )], 71 );
	}

	[Test]
	public void HasIntersectionAndTryFindBoundaryIntersections_ContainedEdges_DistinguishInteriorFromBoundaryContacts() {
		Polygon box = Box( 0, 0, 10, 10 );
		Edge inside = new Edge( 2, 2, 4, 4 );
		Assert.That( box.HasIntersection( inside ), Is.True );
		Assert.That( box.HasIntersection( [inside] ), Is.True );
		Assert.That( box.TryFindBoundaryIntersections( inside, out IReadOnlyList<Point> points ), Is.False );
		Assert.That( points, Is.Empty );
		Assert.That( box.HasIntersection( new Edge( 2, 2, 2, 2 ) ), Is.True );
		Assert.That( box.TryFindBoundaryIntersections( new Edge( 0, 0, 10, 10 ), out points ), Is.True );
		Assert.That( points, Is.EquivalentTo( new Point[] { new( 0, 0 ), new( 10, 10 ) } ) );
		if( points is IList<Point> exposed ) {
			Assert.That( exposed.IsReadOnly, Is.True );
			Assert.Throws<NotSupportedException>( () => exposed[0] = new Point( 5, 5 ) );
		}
	}

	[Test]
	public void ContainsAndTryIntersect_SeededBoxes_MatchIndependentIntegerBoundsAndArea() {
		Random random = new Random( 7291 );
		for( int i = 0; i < 250; i++ ) {
			int ax = random.Next( -20, 20 ), ay = random.Next( -20, 20 );
			int bx = random.Next( -20, 20 ), by = random.Next( -20, 20 );
			int ar = ax + random.Next( 1, 15 ), at = ay + random.Next( 1, 15 );
			int br = bx + random.Next( 1, 15 ), bt = by + random.Next( 1, 15 );
			Polygon a = Box( ax, ay, ar, at ), b = Box( bx, by, br, bt );
			int left = Math.Max( ax, bx ), right = Math.Min( ar, br );
			int bottom = Math.Max( ay, by ), top = Math.Min( at, bt );
			bool contact = left <= right && bottom <= top;
			bool area = left < right && bottom < top;
			string input = $"A=({ax},{ay},{ar},{at}), B=({bx},{by},{br},{bt})";
			Assert.That( a.Contains( b ), Is.EqualTo( ax <= bx && ay <= by && ar >= br && at >= bt ), input );
			Assert.That( a.HasIntersection( b ), Is.EqualTo( contact ), input );
			Assert.That( b.HasIntersection( a ), Is.EqualTo( contact ), input );
			Assert.That( a.TryIntersect( b, out Polygon result ), Is.EqualTo( area ), input );
			if( area ) {
				AssertVertices( result, [new( left, bottom ), new( right, bottom ), new( right, top ), new( left, top )],
					2L * ( right - left ) * ( top - bottom ) );
			} else {
				Assert.That( result, Is.SameAs( Polygon.None ), input );
			}
		}
	}

	[Test]
	public void HasIntersectionAndTryIntersect_SeededTriangles_MatchSeparatingAxisReferenceAndSnappingBounds() {
		Random random = new Random( 8734 );
		for( int i = 0; i < 500; i++ ) {
			Point[] first = NextTriangle(), second = NextTriangle();
			Polygon a = new Polygon( first ), b = new Polygon( second );
			bool contact = !HasSeparatingAxis( first, second ) && !HasSeparatingAxis( second, first );
			string input = $"Seed 8734, iteration {i}, A={string.Join( ';', first )}, B={string.Join( ';', second )}";
			Assert.That( a.HasIntersection( b ), Is.EqualTo( contact ), input );
			bool clipped = a.TryIntersect( b, out Polygon result );
			Assert.That( b.TryIntersect( a, out Polygon reversed ), Is.EqualTo( clipped ), input );
			Assert.That( reversed.Points, Is.EquivalentTo( result.Points ), input );
			if( clipped ) {
				Assert.That( contact, Is.True, input );
				AssertGridApproximation( result, first, input );
				AssertGridApproximation( result, second, input );
			} else {
				Assert.That( result, Is.SameAs( Polygon.None ), input );
			}
		}

		Point[] NextTriangle() {
			Point[] points;
			do {
				points = Enumerable.Range( 0, 3 )
					.Select( _ => new Point( random.Next( -32768, 32768 ), random.Next( -32768, 32768 ) ) ).ToArray();
			} while( Cross( points[0], points[1], points[2] ) == 0 );
			return points;
		}
	}

	private static bool HasSeparatingAxis( Point[] a, Point[] b ) {
		for( int i = 0; i < a.Length; i++ ) {
			Point start = a[i], end = a[( i + 1 ) % a.Length];
			long nx = start.Y - end.Y, ny = end.X - start.X;
			long minA = long.MaxValue, maxA = long.MinValue;
			long minB = long.MaxValue, maxB = long.MinValue;
			foreach( Point point in a ) {
				long projection = (point.X * nx) + (point.Y * ny);
				minA = Math.Min( minA, projection );
				maxA = Math.Max( maxA, projection );
			}
			foreach( Point point in b ) {
				long projection = (point.X * nx) + (point.Y * ny);
				minB = Math.Min( minB, projection );
				maxB = Math.Max( maxB, projection );
			}
			if( maxA < minB || maxB < minA ) {
				return true;
			}
		}
		return false;
	}

	private static void AssertGridApproximation( Polygon result, Point[] source, string input ) {
		int winding = Math.Sign( Cross( source[0], source[1], source[2] ) );
		for( int i = 0; i < source.Length; i++ ) {
			Point a = source[i], b = source[( i + 1 ) % source.Length];
			long margin = Math.Abs( b.X - a.X ) + Math.Abs( b.Y - a.Y );
			foreach( Point point in result.Points ) {
				Assert.That( 2 * Cross( a, b, point ) * winding, Is.GreaterThanOrEqualTo( -margin ), input );
			}
		}
		int resultWinding = Math.Sign( Cross( result.Points[0], result.Points[1], result.Points[2] ) );
		for( int i = 0; i < result.Points.Count; i++ ) {
			long turn = Cross( result.Points[i], result.Points[( i + 1 ) % result.Points.Count], result.Points[( i + 2 ) % result.Points.Count] );
			Assert.That( turn * resultWinding, Is.GreaterThan( 0 ), input );
		}
	}

	private static long Cross( Point a, Point b, Point c ) {
		return (( (long)b.X - a.X ) * ( c.Y - a.Y )) - (( (long)b.Y - a.Y ) * ( c.X - a.X ));
	}

	private static Polygon Box( int x1, int y1, int x2, int y2 ) {
		return new Polygon( [new( x1, y1 ), new( x2, y1 ), new( x2, y2 ), new( x1, y2 )] );
	}

	private static Point[] Regular( int count, double phase = 0 ) {
		return Enumerable.Range( 0, count ).Select( i => {
			double angle = ( 2 * Math.PI * i / count ) + phase;
			return new Point( (int)Math.Round( 20000 * Math.Cos( angle ) ), (int)Math.Round( 20000 * Math.Sin( angle ) ) );
		} ).ToArray();
	}

	private static void AssertVertices( Polygon polygon, Point[] expected, long twiceArea ) {
		Assert.That( polygon.Points, Is.EquivalentTo( expected ) );
		long area = 0;
		int sign = 0;
		for( int i = 0; i < polygon.Points.Count; i++ ) {
			Point a = polygon.Points[i];
			Point b = polygon.Points[( i + 1 ) % polygon.Points.Count];
			Point c = polygon.Points[( i + 2 ) % polygon.Points.Count];
			area += ((long)a.X * b.Y) - ((long)a.Y * b.X);
			long cross = (( (long)b.X - a.X ) * ( c.Y - a.Y )) - (( (long)b.Y - a.Y ) * ( c.X - a.X ));
			Assert.That( cross, Is.Not.Zero );
			sign = sign == 0 ? Math.Sign( cross ) : sign;
			Assert.That( Math.Sign( cross ), Is.EqualTo( sign ) );
		}
		Assert.That( Math.Abs( area ), Is.EqualTo( twiceArea ) );
	}
}
