using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class QueryVisitorTests {
	[TestCase( 0, 0, 128, 128 )]
	[TestCase( 60, 60, 10, 10 )]
	[TestCase( 1000, 1000, 10, 10 )]
	[TestCase( 0, 0, 0, 10 )]
	public void Query_SpanVisitor_MatchesReferenceAndMaterializedOrderWithoutAllocation( int x, int y, int width, int height ) {
		IQuadTree<Rect> tree = CreateTree( out Rect[] items );
		Rect area = new Rect( x, y, width, height );
		Rect[] destination = new Rect[items.Length];
		IReadOnlyList<Rect> expected = tree.Query( area );
		Assert.That( expected, Is.EquivalentTo( items.Where( item => !area.IsEmpty
			&& item.X1 <= area.X2 && item.X2 >= area.X1 && item.Y1 <= area.Y2 && item.Y2 >= area.Y1 ) ) );
		for( int i = 0; i < 100; i++ ) {
			BufferVisitor<Rect> warmup = new BufferVisitor<Rect>( destination );
			tree.Query( area, ref warmup );
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		int count = 0;
		for( int i = 0; i < 100; i++ ) {
			BufferVisitor<Rect> visitor = new BufferVisitor<Rect>( destination );
			tree.Query( area, ref visitor );
			count += visitor.Count;
		}
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( count, Is.EqualTo( expected.Count * 100 ) );
		Assert.That( destination.Take( expected.Count ), Is.EqualTo( expected ) );
		Assert.That( bytes, Is.Zero );
	}

	[Test]
	public void Query_StackBackedVisitor_WritesIntoCallerStorage() {
		Rect bounds = new Rect( 0, 0, 2, 2 );
		IQuadTree<Rect> tree = new SimpleQuadTree<Rect>( bounds );
		Rect item = new Rect( 1, 1, 1, 1 );
		tree.Insert( item );
		Span<Rect> destination = stackalloc Rect[1];
		BufferVisitor<Rect> visitor = new BufferVisitor<Rect>( destination );
		tree.Query( bounds, ref visitor );
		Assert.That( visitor.Count, Is.EqualTo( 1 ) );
		Assert.That( destination[0], Is.EqualTo( item ) );
	}

	[Test]
	public void Query_VisitorThrows_PreservesStateAndSubsequentQueries() {
		IQuadTree<Rect> tree = CreateTree( out Rect[] items );
		Rect area = new Rect( 0, 0, 128, 128 );
		ThrowingVisitor<Rect> visitor = default;
		Assert.Throws<InvalidOperationException>( () => tree.Query( area, ref visitor ) );
		Assert.That( visitor.Count, Is.EqualTo( 2 ) );
		Assert.That( tree.Query( area ), Has.Count.EqualTo( items.Length ) );
	}

	[Test]
	public void Search_SpanVisitor_PreservesCellOrderWithoutAllocation() {
		ISearchableVoronoi searchable = CreateSearchable();
		Rect area = new Rect( 0, 0, 100, 100 );
		IReadOnlyList<int> expected = searchable.Search( area );
		Span<int> destination = stackalloc int[expected.Count];
		for( int i = 0; i < 100; i++ ) {
			BufferVisitor<int> warmup = new BufferVisitor<int>( destination );
			searchable.Search( area, ref warmup );
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		int count = 0;
		for( int i = 0; i < 100; i++ ) {
			BufferVisitor<int> visitor = new BufferVisitor<int>( destination );
			searchable.Search( area, ref visitor );
			count += visitor.Count;
		}
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( count, Is.EqualTo( 100 * expected.Count ) );
		Assert.That( destination.ToArray(), Is.EqualTo( expected ) );
		Assert.That( bytes, Is.Zero );
		Assert.That( searchable.Search( new Rect( 200, 200, 10, 10 ) ), Is.Empty );
		Assert.That( searchable.Search( area ), Is.EqualTo( expected ) );
	}

	[Test]
	public void Search_RefStructVisitorThrows_PreservesPartialStateAndSubsequentQueries() {
		ISearchableVoronoi searchable = CreateSearchable();
		Rect area = new Rect( 0, 0, 100, 100 );
		IReadOnlyList<int> expected = searchable.Search( area );
		Span<int> destination = stackalloc int[1];
		BufferVisitor<int> visitor = new BufferVisitor<int>( destination );
		bool threw = false;
		try {
			searchable.Search( area, ref visitor );
		} catch( IndexOutOfRangeException ) {
			threw = true;
		}
		Assert.That( threw, Is.True );
		Assert.That( visitor.Count, Is.EqualTo( 1 ) );
		Assert.That( destination[0], Is.EqualTo( expected[0] ) );
		Assert.That( searchable.Search( area ), Is.EqualTo( expected ) );
	}

	private static IQuadTree<Rect> CreateTree( out Rect[] items ) {
		IQuadTree<Rect> tree = new SimpleQuadTree<Rect>( new Rect( 0, 0, 128, 128 ) );
		items = new Rect[1026];
		for( int i = 0; i < 1024; i++ ) {
			items[i] = new Rect(  i % 32  * 4,  i / 32  * 4, 2, 2 );
		}
		items[1024] = new Rect( 60, 60, 10, 10 );
		items[1025] = items[0];
		foreach( Rect item in items ) {
			tree.Insert( item );
		}
		return tree;
	}

	private static ISearchableVoronoi CreateSearchable() {
		Point[] sites = new Point[100];
		for( int i = 0; i < sites.Length; i++ ) {
			sites[i] = new Point( ( i % 10  * 10) + 5, ( i / 10  * 10) + 5 );
		}
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 100, 100 ), sites );
		ISearchableVoronoiFactory searchableFactory = new QuadTreeSearchableVoronoiFactory( new SimpleQuadTreeFactory() );
		return searchableFactory.Create( diagram, new Point( 100, 100 ) );
	}

	private ref struct BufferVisitor<T>( Span<T> destination ) : IQueryVisitor<T> {
		private readonly Span<T> _destination = destination;
		public int Count;

		public void Visit( scoped in T item ) {
			_destination[Count] = item;
			Count++;
		}
	}

	private struct ThrowingVisitor<T> : IQueryVisitor<T> {
		public int Count;

		public void Visit( scoped in T item ) {
			if( ++Count == 2 ) {
				throw new InvalidOperationException( "Expected visitor failure." );
			}
		}
	}
}
