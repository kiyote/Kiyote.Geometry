using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class AllocationRegressionTests {

	[Test]
	public void Ctor_ThousandTriangles_DoesNotAllocateOneObjectPerItem() {
		GC.KeepAlive( CreateTriangles( 1000 ) );
		long before = GC.GetAllocatedBytesForCurrentThread();
		Triangle[] triangles = CreateTriangles( 1000 );
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		GC.KeepAlive( triangles );
		Assert.That( triangles[999].P1, Is.EqualTo( new Point( 999, 0 ) ) );
		Assert.That( bytes, Is.LessThanOrEqualTo( 33024 ) );
	}

	private static Triangle[] CreateTriangles( int count ) {
		Triangle[] result = new Triangle[count];
		for( int i = 0; i < count; i++ ) {
			result[i] = new Triangle( new Point( i, 0 ), new Point( i + 1, 0 ), new Point( i, 1 ) );
		}
		return result;
	}

	[Test]
	public void Query_PopulatedQuadtree_DoesNotAllocatePerCandidateOrRecursionLevel() {
		SimpleQuadTreeNode<Rect> tree = new SimpleQuadTreeNode<Rect>( new Rect( 0, 0, 1920, 1080 ) );
		Random random = new Random( 0xBADF00D );
		IRect area = new Rect( 250, 250, 20, 20 );
		int expected = 0;
		for( int i = 0; i < 10000; i++ ) {
			Rect item = new Rect( random.Next( 0, 980 ), random.Next( 0, 980 ), random.Next( 5, 20 ), random.Next( 5, 20 ) );
			tree.Insert( item );
			if( item.X1 <= area.X2 && item.X2 >= area.X1 && item.Y1 <= area.Y2 && item.Y2 >= area.Y1 ) {
				expected++;
			}
		}
		for( int i = 0; i < 100; i++ ) {
			GC.KeepAlive( tree.Query( area ) );
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		int count = 0;
		for( int i = 0; i < 100; i++ ) {
			count += tree.Query( area ).Count;
		}
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( count, Is.EqualTo( 100 * expected ) );
		Assert.That( bytes, Is.LessThanOrEqualTo( 4096 * 100 ) );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Rasterize_PixelFallback_DoesNotBoxPerScanline( bool useDelegate ) {
		IRasterizer rasterizer = new IntegerRasterizer();
		Point[] points = [new Point( 0, 0 ), new Point( 20, 0 ), new Point( 20, 20 ), new Point( 0, 20 )];
		int[] calls = [0];
		Counter sink = new Counter( calls );
		Action<int, int> callback = ( _, _ ) => calls[0]++;
		for( int i = 0; i < 100; i++ ) {
			Draw();
		}
		calls[0] = 0;
		long before = GC.GetAllocatedBytesForCurrentThread();
		for( int i = 0; i < 100; i++ ) {
			Draw();
		}
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( calls[0], Is.EqualTo( 44100 ) );
		Assert.That( bytes, Is.Zero );

		void Draw() {
			if( useDelegate ) {
				rasterizer.Rasterize( points.AsSpan(), callback );
			} else {
				rasterizer.Rasterize( points.AsSpan(), sink );
			}
		}
	}

	[Test]
	public void Search_EqualBounds_PreservesDistinctCellsAndMultiplicity() {
		Rect bounds = new Rect( 0, 0, 3, 3 );
		Cell a = new Cell( new Point( 0, 0 ),
			new Polygon( [new Point( 0, 0 ), new Point( 2, 0 ), new Point( 0, 2 )] ), false, bounds );
		Cell b = new Cell( new Point( 2, 2 ),
			new Polygon( [new Point( 2, 2 ), new Point( 0, 2 ), new Point( 2, 0 )] ), false, bounds );
		IVoronoi diagram = new Voronoi( [a.Center, b.Center], [a, b], [0, 0, 0], [], [] );
		ISearchableVoronoiFactory factory = new QuadTreeSearchableVoronoiFactory( new SimpleQuadTreeFactory() );
		ISearchableVoronoi searchable = factory.Create( diagram, new Point( 3, 3 ) );
		Assert.That( searchable.Search( bounds ), Is.EquivalentTo( new[] { a, b } ) );
	}

	private readonly struct Counter( int[] calls ) : IPixelOperation {
		public void Pixel( int x, int y ) => calls[0]++;
		public void PixelSpan( int xMin, int xMax, int y ) => IPixelOperation.EmitSpan( this, xMin, xMax, y );
	}

	[Test]
	public void EqualsAndGetHashCode_ValueTriangles_PreserveOrderedEqualityAndHashing() {
		Triangle a = new Triangle( new Point( 0, 0 ), new Point( 2, 0 ), new Point( 0, 2 ) );
		Triangle b = new Triangle( a.P1, a.P2, a.P3 );
		Assert.That( a == b, Is.True );
		Assert.That( a.Equals( b ), Is.True );
		Assert.That( a.GetHashCode(), Is.EqualTo( b.GetHashCode() ) );
		Assert.That( new HashSet<Triangle> { a, b }, Has.Count.EqualTo( 1 ) );
		Assert.That( a, Is.Not.EqualTo( new Triangle( a.P3, a.P2, a.P1 ) ) );
	}

	[Test]
	public void Rasterize_DefaultTriangle_RejectsFillBeforeEmission() {
		IRasterizer rasterizer = new IntegerRasterizer();
		int[] calls = [0];
		Assert.Throws<ArgumentException>( () => rasterizer.Rasterize( default( Triangle ), new Counter( calls ) ) );
		Assert.That( calls[0], Is.Zero );
	}

	[Test]
	public void EmitSpan_MaximumEndpoint_EmitsInclusiveRunWithoutOverflow() {
		int[] calls = [0];
		IPixelOperation.EmitSpan( new Counter( calls ), int.MaxValue, int.MaxValue, 0 );
		Assert.That( calls[0], Is.EqualTo( 1 ) );
	}
}
