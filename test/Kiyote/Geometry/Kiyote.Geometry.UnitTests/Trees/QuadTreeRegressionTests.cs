using System.Diagnostics.CodeAnalysis;
using Moq;

namespace Kiyote.Geometry.Trees.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class QuadTreeRegressionTests {

	[Test]
	public void Query_SmallItemInPopulatedLeaf_ReturnsItExactlyOnce() {
		SimpleQuadTreeNode<Rect> tree = new SimpleQuadTreeNode<Rect>( new Rect( 0, 0, 100, 100 ) );
		Rect item = new Rect( 1, 1, 2, 2 );
		tree.Insert( item );
		Assert.That( tree.Count, Is.EqualTo( 1 ) );
		Assert.That( tree.Query( item ), Is.EqualTo( new[] { item } ) );
	}

	[Test]
	public void IsEmpty_PopulatedTerminalLeaf_IsFalse() {
		SimpleQuadTreeNode<Rect> tree = new SimpleQuadTreeNode<Rect>( new Rect( 0, 0, 2, 2 ) );
		tree.Insert( new Rect( 0, 0, 1, 1 ) );
		Assert.That( tree.IsEmpty, Is.False );
	}

	[TestCase( 1, 1 )]
	[TestCase( 1, 100 )]
	[TestCase( 100, 1 )]
	[TestCase( 2, 100 )]
	[TestCase( 100, 2 )]
	[TestCase( 3, 101 )]
	[TestCase( 101, 3 )]
	[TestCase( 7, 9 )]
	[TestCase( 100, 100 )]
	public void Query_EveryOccupiedCell_IsCoveredExactlyOnce( int width, int height ) {
		Rect bounds = new Rect( -7, -11, width, height );
		IQuadTree<Rect> tree = new SimpleQuadTree<Rect>( bounds );
		for( int x = bounds.X1; x <= bounds.X2; x++ ) {
			for( int y = bounds.Y1; y <= bounds.Y2; y++ ) {
				tree.Insert( new Rect( x, y, 1, 1 ) );
			}
		}
		Assert.That( tree.Count, Is.EqualTo( width * height ) );
		Assert.That( tree.Query( bounds ), Has.Count.EqualTo( width * height ) );
		for( int x = bounds.X1; x <= bounds.X2; x++ ) {
			for( int y = bounds.Y1; y <= bounds.Y2; y++ ) {
				Rect cell = new Rect( x, y, 1, 1 );
				Assert.That( tree.Query( cell ), Is.EqualTo( new[] { cell } ), $"Bounds {width}x{height}, query ({x},{y})" );
			}
		}
	}

	[Test]
	public void Query_SplitBoundaryAndSpanningItem_DoesNotPruneItsSibling() {
		SimpleQuadTreeNode<Rect> tree = new SimpleQuadTreeNode<Rect>( new Rect( 0, 0, 100, 100 ) );
		Rect spanning = new Rect( 49, 49, 2, 2 );
		tree.Insert( spanning );
		tree.Insert( new Rect( 1, 1, 2, 2 ) );
		Assert.That( tree.Query( new Rect( 49, 49, 1, 1 ) ), Is.EqualTo( new[] { spanning } ) );
		Assert.That( tree.Query( new Rect( 50, 50, 1, 1 ) ), Is.EqualTo( new[] { spanning } ) );
	}

	[TestCase( -1, 0, 1, 1 )]
	[TestCase( 99, 99, 2, 2 )]
	[TestCase( 100, 100, 1, 1 )]
	[TestCase( 0, 0, 0, 0 )]
	public void Insert_InvalidBounds_ThrowsWithoutChangingContents( int x, int y, int width, int height ) {
		Rect bounds = new Rect( 0, 0, 100, 100 );
		IQuadTree<Rect> tree = new SimpleQuadTree<Rect>( bounds );
		Rect existing = new Rect( 10, 10, 20, 20 );
		tree.Insert( existing );
		Assert.That( () => tree.Insert( new Rect( x, y, width, height ) ), Throws.ArgumentException );
		Assert.That( tree.Count, Is.EqualTo( 1 ) );
		Assert.That( tree.Query( bounds ), Is.EqualTo( new[] { existing } ) );
	}

	[TestCase( int.MinValue, int.MinValue, 100, 100 )]
	[TestCase( int.MaxValue - 99, int.MaxValue - 99, 100, 100 )]
	[TestCase( 0, 0, int.MaxValue, int.MaxValue )]
	[TestCase( int.MinValue, int.MinValue, int.MaxValue, int.MaxValue )]
	public void Query_FullIntBounds_PartitionsWithoutOverflow( int x, int y, int width, int height ) {
		Rect bounds = new Rect( x, y, width, height );
		SimpleQuadTreeNode<Rect> tree = new SimpleQuadTreeNode<Rect>( bounds );
		Rect[] items = [
			new Rect( bounds.X1, bounds.Y1, 1, 1 ),
			new Rect( bounds.X2, bounds.Y1, 1, 1 ),
			new Rect( bounds.X1, bounds.Y2, 1, 1 ),
			new Rect( bounds.X2, bounds.Y2, 1, 1 )
		];
		foreach( Rect item in items ) {
			tree.Insert( item );
		}
		Assert.That( tree.Contents, Is.Empty, "Large bounds must actually subdivide, not wrap the area test." );
		Assert.That( tree.Count, Is.EqualTo( items.Length ) );
		Assert.That( tree.Query( bounds ), Is.EquivalentTo( items ) );
		foreach( Rect item in items ) {
			Assert.That( tree.Query( item ), Is.EqualTo( new[] { item } ) );
		}
	}

	[Test]
	public void Query_SeededItemsAndQueries_MatchesIndependentInclusiveReference() {
		Random random = new Random( 61723 );
		Rect bounds = new Rect( -50, -60, 101, 123 );
		IQuadTree<Rect> tree = new SimpleQuadTree<Rect>( bounds );
		List<Rect> items = [];
		for( int i = 0; i < 600; i++ ) {
			int x = random.Next( bounds.X1, bounds.X2 + 1 ), y = random.Next( bounds.Y1, bounds.Y2 + 1 );
			Rect item = new Rect( x, y, random.Next( 1, Math.Min( 20, bounds.X2 - x + 1 ) + 1 ),
				random.Next( 1, Math.Min( 20, bounds.Y2 - y + 1 ) + 1 ) );
			items.Add( item );
			tree.Insert( item );
		}
		for( int i = 0; i < 1000; i++ ) {
			Rect query = new Rect( random.Next( -100, 100 ), random.Next( -100, 100 ),
				random.Next( 0, 150 ), random.Next( 0, 150 ) );
			Rect[] expected = items.Where( item => !query.IsEmpty
				&& item.X1 <= query.X2 && item.X2 >= query.X1
				&& item.Y1 <= query.Y2 && item.Y2 >= query.Y1 ).ToArray();
			Assert.That( tree.Query( query ), Is.EquivalentTo( expected ),
				$"Query {i}: ({query.X1},{query.Y1}), {query.Width}x{query.Height}" );
		}
		Assert.That( tree.Count, Is.EqualTo( items.Count ) );
	}

	[Test]
	public void Query_EmptyOrOutsideQuery_ReturnsNoItems() {
		Rect bounds = new Rect( 0, 0, 100, 100 );
		IQuadTree<Rect> tree = new SimpleQuadTree<Rect>( bounds );
		tree.Insert( bounds );
		Assert.That( tree.Query( default( Rect ) ), Is.Empty );
		Assert.That( tree.Query( new Rect( 100, 100, 1, 1 ) ), Is.Empty );
	}

	[Test]
	public void Query_InterfaceItemsAndCustomQuery_ReturnsOriginalEntry() {
		IQuadTree<IRect> tree = new SimpleQuadTree<IRect>( new Rect( 0, 0, 10, 10 ) );
		IRect item = new Rect( 1, 1, 2, 2 );
		tree.Insert( item );
		Mock<IRect> query = new Mock<IRect>( MockBehavior.Strict );
		query.SetupGet( q => q.IsEmpty ).Returns( false );
		query.SetupGet( q => q.X1 ).Returns( 0 );
		query.SetupGet( q => q.Y1 ).Returns( 0 );
		query.SetupGet( q => q.Width ).Returns( 2 );
		query.SetupGet( q => q.Height ).Returns( 2 );
		IReadOnlyList<IRect> result = tree.Query( query.Object );
		Assert.That( result, Has.Count.EqualTo( 1 ) );
		Assert.That( result[0], Is.SameAs( item ) );
	}

	[Test]
	public void CtorAndInsertAndQuery_NullInputs_ThrowArgumentNullException() {
		Assert.That( () => new SimpleQuadTreeNode<IRect>( null! ), Throws.ArgumentNullException );
		IQuadTree<IRect> tree = new SimpleQuadTree<IRect>( new Rect( 0, 0, 10, 10 ) );
		Assert.That( () => tree.Insert( null! ), Throws.ArgumentNullException );
		Assert.That( () => tree.Query( null! ), Throws.ArgumentNullException );
		Assert.That( tree.Count, Is.Zero );
	}

	[Test]
	public void Insert_RepeatedValue_PreservesInsertionMultiplicity() {
		IQuadTree<Rect> tree = new SimpleQuadTree<Rect>( new Rect( 0, 0, 100, 100 ) );
		Rect item = new Rect( 1, 1, 1, 1 );
		tree.Insert( item );
		tree.Insert( item );
		Assert.That( tree.Count, Is.EqualTo( 2 ) );
		Assert.That( tree.Query( item ), Is.EqualTo( new[] { item, item } ) );
	}

	[Test]
	public void Contents_ReadOnlyView_CannotInvalidateSubtreeCount() {
		SimpleQuadTreeNode<Rect> tree = new SimpleQuadTreeNode<Rect>( new Rect( 0, 0, 2, 2 ) );
		tree.Insert( new Rect( 0, 0, 1, 1 ) );
		if( tree.Contents is ICollection<Rect> collection ) {
			Assert.That( () => collection.Clear(), Throws.TypeOf<NotSupportedException>() );
		}
		Assert.That( tree.Count, Is.EqualTo( 1 ) );
		Assert.That( tree.IsEmpty, Is.False );
	}

	[Test]
	public void Ctor_MutableBounds_OwnsStableSnapshot() {
		Rect current = new Rect( 0, 0, 100, 100 );
		Mock<IRect> bounds = new Mock<IRect>( MockBehavior.Strict );
		bounds.SetupGet( b => b.X1 ).Returns( () => current.X1 );
		bounds.SetupGet( b => b.Y1 ).Returns( () => current.Y1 );
		bounds.SetupGet( b => b.X2 ).Returns( () => current.X2 );
		bounds.SetupGet( b => b.Y2 ).Returns( () => current.Y2 );
		bounds.SetupGet( b => b.Width ).Returns( () => current.Width );
		bounds.SetupGet( b => b.Height ).Returns( () => current.Height );
		bounds.SetupGet( b => b.IsEmpty ).Returns( () => current.IsEmpty );
		SimpleQuadTreeNode<Rect> tree = new SimpleQuadTreeNode<Rect>( bounds.Object );
		current = new Rect( 100, 100, 10, 10 );
		Assert.That( tree.Bounds.X1, Is.Zero );
		Assert.That( tree.Bounds.Y1, Is.Zero );
		Assert.That( tree.Bounds.Width, Is.EqualTo( 100 ) );
	}
}
