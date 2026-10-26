using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DiagramRegressionTests {

	private static readonly Rect Bounds = new Rect( -20000, -20000, 40001, 40001 );
	private static readonly Point[] TriangleSites = [new Point( -10, -10 ), new Point( 10, -10 ), new Point( 0, 10 )];
	private readonly IVoronoiFactory _voronoi = new D3VoronoiFactory();

	[TestCase( 25 )]
	[TestCase( 26 )]
	[TestCase( 40 )]
	[TestCase( IVoronoiFactory.MaximumCellVertices - 1 )]
	[TestCase( IVoronoiFactory.MaximumCellVertices )]
	public void Create_ClosedCellAtSupportedDegrees_PreservesEveryNeighbour( int degree ) {
		Point[] sites = Ring( degree, false );
		AssertTraversalCount( sites, degree );
		IVoronoi diagram = _voronoi.Create( Bounds, sites );
		ValidateDiagram( diagram, sites );
		Assert.That( diagram.Cells[0].Polygon.Points, Has.Count.EqualTo( degree ) );
		Assert.That( diagram.Neighbours[diagram.Cells[0]], Has.Count.EqualTo( degree ) );
		Assert.That( diagram.Cells[0].IsOpen, Is.False );
	}

	[Test]
	public void Create_FractionalCircumcenter_RoundsOnlyAfterClipping() {
		Point[] sites = [new Point( 0, 0 ), new Point( 9, 2 ), new Point( 2, 9 )];
		IVoronoi diagram = _voronoi.Create( new Rect( -20, -20, 41, 41 ), sites );
		Point[] expected = [new Point( -20, -20 ), new Point( 9, -20 ), new Point( 3, 3 ), new Point( -20, 9 )];
		Assert.That( diagram.Cells[0].Polygon.Points, Is.EquivalentTo( expected ) );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Create_FullCoordinateDomain_PreservesAllSites( bool voronoi ) {
		Point[] sites = [new Point( -32768, -32768 ), new Point( 32767, -32768 ), new Point( 0, 32767 )];
		if( voronoi ) {
			IVoronoi diagram = _voronoi.Create( new Rect( -32768, -32768, 65536, 65536 ), sites );
			for( int i = 0; i < sites.Length; i++ ) {
				Assert.That( diagram.Cells[i].Polygon.Contains( sites[i] ), Is.True );
			}
		} else {
			IDelaunayFactory factory = new D3DelaunayFactory();
			Assert.That( factory.Create( sites ).Points, Is.EqualTo( sites ) );
		}
	}

	[Test]
	public void GetTriangleCapacity_RepresentableBoundary_IsCheckedWithoutAllocation() {
		int maximumCount = (int)( ( Array.MaxLength + 15L ) / 6 );
		Assert.That( MapboxDelaunatorFactory.GetTriangleCapacity( maximumCount ), Is.EqualTo( (6L * maximumCount) - 15 ) );
		Assert.That( () => MapboxDelaunatorFactory.GetTriangleCapacity( maximumCount + 1 ), Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void Create_SeededIrregularSites_RespectNearestSiteHalfPlanes() {
		Random random = new Random( 5291 );
		for( int sample = 0; sample < 10; sample++ ) {
			Point[] sites = new Point[36];
			for( int i = 0; i < sites.Length; i++ ) {
				sites[i] = new Point( -6000 + (i % 6 * 2000) + random.Next( -600, 601 ),
					-6000 + (i / 6 * 2000) + random.Next( -600, 601 ) );
			}
			ValidateDiagram( _voronoi.Create( Bounds, sites ), sites );
		}
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Create_CellAboveTraversalLimit_ThrowsExplicitly( bool open ) {
		int degree = IVoronoiFactory.MaximumCellVertices + 1;
		Point[] sites = Ring( open ? degree + 1 : degree, open );
		AssertTraversalCount( sites, degree );
		Assert.That(
			() => _voronoi.Create( Bounds, sites ),
			Throws.TypeOf<InvalidOperationException>().With.Message.Contains( IVoronoiFactory.MaximumCellVertices.ToString( CultureInfo.InvariantCulture ) )
		);
		Assert.That( _voronoi.Create( Bounds, TriangleSites ).Cells, Has.Count.EqualTo( TriangleSites.Length ) );
	}

	[Test]
	public void Create_OpenCellAtLimit_HasRoomForProjectedVertices() {
		Point[] sites = Ring( IVoronoiFactory.MaximumCellVertices + 1, true );
		AssertTraversalCount( sites, IVoronoiFactory.MaximumCellVertices );
		IVoronoi diagram = _voronoi.Create( Bounds, sites );
		ValidateDiagram( diagram, sites );
		Assert.That( diagram.Cells[0].IsOpen, Is.True );
		Assert.That( diagram.Cells[0].Polygon.Points.Count, Is.GreaterThan( IVoronoiFactory.MaximumCellVertices ) );
	}

	[TestCaseSource( nameof( InvalidSites ) )]
	public void Create_InvalidSites_ThrowsArgumentException( bool voronoi, Point[] points ) {
		Assert.That( () => CreatePoints( voronoi, points ), Throws.ArgumentException );
	}

	private static IEnumerable<TestCaseData> InvalidSites() {
		Point[][] inputs = [
			[],
			[new Point( 0, 0 )],
			[new Point( 0, 0 ), new Point( 1, 1 )],
			[new Point( -10, -10 ), new Point( 0, 0 ), new Point( 10, 10 )],
			[.. TriangleSites, TriangleSites[0]]
		];
		foreach( bool voronoi in new[] { false, true } ) {
			foreach( Point[] points in inputs ) {
				yield return new TestCaseData( voronoi, points );
			}
		}
	}

	[TestCase( false, false )]
	[TestCase( false, true )]
	[TestCase( true, false )]
	[TestCase( true, true )]
	public void Create_OutOfDomainSite_IsNeverSanitizedAway( bool voronoi, bool sanitize ) {
		Assert.That( () => CreatePoints( voronoi, [.. TriangleSites, new Point( 32768, 0 )], sanitize ),
			Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void Create_OutsideSite_RequiresExplicitSanitization() {
		Point[] points = [.. TriangleSites, new Point( 20001, 0 )];
		Assert.That( () => CreatePoints( true, points ), Throws.ArgumentException );
		Assert.That( CreatePoints( true, points, true ), Is.EqualTo( TriangleSites ) );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Create_Sanitization_PreservesFirstOccurrenceOrderAndRevalidates( bool voronoi ) {
		Assert.That( CreatePoints( voronoi, [TriangleSites[0], TriangleSites[1], TriangleSites[0], TriangleSites[2]], true ),
			Is.EqualTo( TriangleSites ) );
		Assert.That( () => CreatePoints( voronoi, [TriangleSites[0], TriangleSites[0], TriangleSites[1]], true ),
			Throws.ArgumentException );
		Assert.That( () => CreatePoints( voronoi, [new Point( 0, 0 ), new Point( 1, 1 ), new Point( 2, 2 )], true ),
			Throws.ArgumentException );
	}

	[Test]
	public void Create_FilteringLeavesTooFewSites_RejectsInsteadOfReturningPartialDiagram() {
		Assert.That( () => CreatePoints( true, [TriangleSites[0], TriangleSites[1], new Point( 20001, 0 )], true ),
			Throws.ArgumentException );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Create_InputMutation_DoesNotChangeSnapshot( bool voronoi ) {
		Point[] input = [.. TriangleSites];
		IReadOnlyList<Point> result = CreatePoints( voronoi, input );
		input[0] = new Point( 100, 100 );
		Assert.That( result, Is.EqualTo( TriangleSites ) );
		List<Point> list = [.. TriangleSites];
		result = CreatePoints( voronoi, list );
		list.Clear();
		Assert.That( result, Is.EqualTo( TriangleSites ) );
		AssertReadOnly( result );
	}

	[Test]
	public void Create_ResultCollections_DoNotExposeMutableStorage() {
		IVoronoi voronoi = _voronoi.Create( Bounds, TriangleSites );
		AssertReadOnly( voronoi.Cells );
		AssertReadOnly( voronoi.Edges );
		AssertReadOnly( voronoi.Neighbours[voronoi.Cells[0]] );
		if( voronoi.Neighbours is IDictionary<Cell, IReadOnlyList<Cell>> dictionary ) {
			Assert.That( () => dictionary[voronoi.Cells[0]] = dictionary[voronoi.Cells[0]], Throws.TypeOf<NotSupportedException>() );
		}
		IDelaunayFactory factory = new D3DelaunayFactory();
		IDelaunay delaunay = factory.Create( TriangleSites );
		AssertReadOnly( delaunay.Triangles );
		AssertReadOnly( delaunay.Hull );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Create_UnrepresentableCount_RejectsBeforeReadingOrAllocating( bool voronoi ) {
		Assert.That( () => CreatePoints( voronoi, new ImpossibleSites() ), Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[TestCase( 0, 0, 0, 0 )]
	[TestCase( 0, 0, 1, 10 )]
	[TestCase( 0, 0, 10, 1 )]
	[TestCase( -32769, 0, 65536, 10 )]
	[TestCase( 0, 0, 32769, 10 )]
	public void Create_InvalidBounds_RejectsBeforeClipping( int x, int y, int width, int height ) {
		Assert.That( () => _voronoi.Create( new Rect( x, y, width, height ), TriangleSites ),
			Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	private IReadOnlyList<Point> CreatePoints( bool voronoi, IReadOnlyList<Point> points, bool sanitize = false ) {
		if( voronoi ) {
			return _voronoi.Create( Bounds, points, sanitize ).Points;
		}
		IDelaunayFactory delaunay = new D3DelaunayFactory();
		return delaunay.Create( points, sanitize ).Points;
	}

	private static void AssertReadOnly<T>( IReadOnlyList<T> values ) {
		Assert.That( values.Count, Is.GreaterThan( 0 ) );
		if( values is IList<T> list ) {
			Assert.That( () => list[0] = values[0], Throws.TypeOf<NotSupportedException>() );
		}
	}

	private static Point[] Ring( int count, bool open ) {
		Point[] points = new Point[count + 1];
		for( int i = 0; i < count; i++ ) {
			double angle = open ? -1.4 + (2.8 * i / ( count - 1 )) : 2 * Math.PI * i / count;
			points[i + 1] = new Point( (int)Math.Round( 10000 * Math.Cos( angle ) ), (int)Math.Round( 10000 * Math.Sin( angle ) ) );
		}
		return points;
	}

	private static void AssertTraversalCount( Point[] sites, int expected ) {
		MapboxDelaunator triangulation = MapboxDelaunatorFactory.Create( sites.ToCoords() );
		int count = 0;
		foreach( int index in triangulation.Triangles ) {
			if( index == 0 ) {
				count++;
			}
		}
		Assert.That( count, Is.EqualTo( expected ), "Fixture must exercise the actual pre-clipping traversal count." );
	}

	private static void ValidateDiagram( IVoronoi diagram, Point[] sites ) {
		Assert.That( diagram.Cells, Has.Count.EqualTo( sites.Length ) );
		for( int i = 0; i < sites.Length; i++ ) {
			Cell cell = diagram.Cells[i];
			Assert.That( cell.Center, Is.EqualTo( sites[i] ) );
			Assert.That( cell.Polygon.Contains( sites[i] ), Is.True, $"Cell {i}" );
			Assert.That( cell.Polygon.Points.Count, Is.InRange( 3, Polygon.MaximumVertices ) );
			foreach( Point point in cell.Polygon.Points ) {
				Assert.That( Bounds.Contains( point ), Is.True );
				foreach( Point other in sites ) {
					long dx = (long)other.X - cell.Center.X, dy = (long)other.Y - cell.Center.Y;
					long offset = ((long)other.X * other.X) + ((long)other.Y * other.Y)
						- ((long)cell.Center.X * cell.Center.X) - ((long)cell.Center.Y * cell.Center.Y);
					long excess = (2 * ( (dx * point.X) + (dy * point.Y) )) - offset;
					long floorMargin = 2 * ( Math.Max( 0, -dx ) + Math.Max( 0, -dy ) );
					Assert.That( excess, Is.LessThanOrEqualTo( floorMargin ),
						$"Site {cell.Center}, neighbour candidate {other}, output vertex {point}" );
				}
			}
			IReadOnlyList<Cell> neighbours = diagram.Neighbours[cell];
			Assert.That( neighbours.Distinct().Count(), Is.EqualTo( neighbours.Count ) );
			foreach( Cell neighbour in neighbours ) {
				Assert.That( neighbour, Is.Not.SameAs( cell ) );
				Assert.That( diagram.Cells, Does.Contain( neighbour ) );
				Assert.That( diagram.Neighbours[neighbour], Does.Contain( cell ) );
			}
		}
	}

	private sealed class ImpossibleSites : IReadOnlyList<Point> {
		public int Count => int.MaxValue;
		public Point this[int index] => throw new AssertionException( "Sites were read before count validation." );
		public IEnumerator<Point> GetEnumerator() => throw new AssertionException( "Sites were enumerated before count validation." );
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
