
namespace Kiyote.Geometry.Trees.UnitTests;

[TestFixture]
internal sealed class SimpleQuadTreeNodeTests {

	private Rect _bounds;
	private SimpleQuadTreeNode<Rect> _node;

	[SetUp]
	public void SetUp() {
		_bounds = new Rect( 0, 0, 100, 100 );
		_node = new SimpleQuadTreeNode<Rect>( _bounds );
	}

	[Test]
	public void Bounds_DefaultTestNode_BoundsMatch() {
		Assert.That( _bounds, Is.EqualTo( _node.Bounds ) );
	}

	[Test]
	public void Contents_DefaultTestNode_ReturnsEmpty() {
		Assert.That( _node.Contents, Is.Empty );
	}

	[Test]
	public void Count_DefaultTestNode_ReturnsZero() {
		Assert.That( _node.Count, Is.EqualTo( 0 ) );
	}

	[Test]
	public void IsEmpty_DefaultTestNode_ReturnsTrue() {
		Assert.That( _node.IsEmpty, Is.True );
	}

	[Test]
	public void IsEmpty_ZeroAreaBounds_ThrowsException() {
		Rect bounds = new Rect( 0, 0, 0, 0 );
		 Assert.Throws<InvalidOperationException>( () => new SimpleQuadTreeNode<Rect>( bounds ) );
	}

	[Test]
	public void IsEmpty_ItemInserted_ReturnsFalse() {
		Rect item = SmallerBy( _bounds, 10 );
		_node.Insert( item );

		Assert.That( _node.IsEmpty, Is.False );
	}

	[Test]
	public void Insert_ItemSmaller_ItemIsContained() {
		Rect item = SmallerBy( _bounds, 10 );

		_node.Insert( item );

		Assert.That( _node.Count, Is.EqualTo( 1 ) );
		Assert.That( _node.Contents, Is.EquivalentTo( new List<Rect> { item } ) );
	}

	[Test]
	public void Insert_ItemLarger_ItemIsRejected() {
		Rect item = LargerBy( _bounds, 10 );

		_node.Insert( item );

		Assert.That( _node.Count, Is.EqualTo( 0 ) );
	}

	[Test]
	public void Insert_SubdivisionTooSmall_StillInserts() {
		Rect bounds = new Rect( 0, 0, 2, 2 );
		SimpleQuadTreeNode<Rect> node = new SimpleQuadTreeNode<Rect>( bounds );
		Rect item = SmallerBy( bounds, 1 );

		node.Insert( item );

		Assert.That( node.Count, Is.EqualTo( 1 ) );
		Assert.That( node.GetSubTreeContents, Is.EquivalentTo( new List<Rect> { item } ) );
	}

	[Test]
	public void Insert_ItemTooBigForQuadButContainsSubItem_ItemIsStoredInContents() {
		Rect item = new Rect( 0, 0, 55, 55 );
		Rect subItem = new Rect( 0, 0, 25, 25 );

		_node.Insert( item );
		_node.Insert( subItem );

		Assert.That( _node.Count, Is.EqualTo( 2 ), "Count does not match" );
		Assert.That( _node.Contents, Is.EquivalentTo( new List<Rect> { item } ), "Contents do not match" );
		Assert.That( _node.GetSubTreeContents, Is.EquivalentTo( new List<Rect> { item, subItem } ), "SubTreeContents do not match" );
	}

	[Test]
	public void Query_OverlappingQuery_ItemReturned() {
		Rect item = SmallerBy( _bounds, 10 );
		_node.Insert( item );

		IReadOnlyList<Rect> result = _node.Query( item );

		Assert.That( result, Is.EquivalentTo( new List<Rect> { item } ) );
	}

	[Test]
	public void Query_NonOverlappingQuery_NothingReturned() {
		Rect item = new Rect( 10, 10, 30, 80 );
		_node.Insert( item );

		Rect query = new Rect( 50, 10, 30, 80 );

		IReadOnlyList<Rect> result = _node.Query( query );

		Assert.That( result, Is.Empty );
	}

	[Test]
	public void Query_SubContainedItem_ItemReturned() {
		Rect item1 = SmallerBy( _bounds, 10 );
		_node.Insert( item1 );
		Rect item2 = SmallerBy( item1, 20 );
		_node.Insert( item2 );

		Rect query = SmallerBy( item1, 10 );
		IReadOnlyList<Rect> result = _node.Query( query );

		Assert.That( result, Is.EquivalentTo( new List<Rect> { item1, item2 } ), $"Querying: {query}" );
	}

	[TestCase( 10, 10, 20, 20, 0, 0, 49, 49 )]
	[TestCase( 60, 10, 20, 20, 50, 0, 99, 49 )]
	[TestCase( 10, 60, 20, 20, 0, 50, 49, 99 )]
	[TestCase( 60, 60, 20, 20, 50, 50, 99, 99 )]
	public void Query_NonOverlappingContainedItems_CorrectItemReturned(
		int x,
		int y,
		int width,
		int height,
		int ex1,
		int ey1,
		int ex2,
		int ey2
	) {
		Rect item1 = new Rect( 0, 0, 50, 50 );   //Top-Left
		Rect item2 = new Rect( 0, 50, 50, 50 );  //Bottom-Left
		Rect item3 = new Rect( 50, 0, 50, 50 );  //Top-Right
		Rect item4 = new Rect( 50, 50, 50, 50 ); // Bottom-Right

		_node.Insert( item1 );
		_node.Insert( item2 );
		_node.Insert( item3 );
		_node.Insert( item4 );

		Rect query = new Rect( x, y, width, height );
		IReadOnlyList<Rect> result = _node.Query( query );

		Assert.That( result.Count, Is.EqualTo( 1 ), $"Querying: {x},{y} {width}x{height}" );
		Rect r = result[0];
		Assert.That( r.X1, Is.EqualTo( ex1 ), $"Querying: {r.X1},{r.Y1},{r.X2},{r.Y2}" );
		Assert.That( r.Y1, Is.EqualTo( ey1 ), $"Querying: {r.X1},{r.Y1},{r.X2},{r.Y2}" );
		Assert.That( r.X2, Is.EqualTo( ex2 ), $"Querying: {r.X1},{r.Y1},{r.X2},{r.Y2}" );
		Assert.That( r.Y2, Is.EqualTo( ey2 ), $"Querying: {r.X1},{r.Y1},{r.X2},{r.Y2}" );
	}

	[Test]
	public void Query_QueryContainsAllNodes_AllContainedNodesReturned() {
		Rect item1 = new Rect( 5, 5, 40, 40 );
		_node.Insert( item1 );
		Rect item2 = new Rect( 10, 10, 30, 30 );
		_node.Insert( item2 );

		Rect query = new Rect( -1, -1, 52, 52 );

		IReadOnlyList<Rect> items = _node.Query( query );
		Assert.That( items, Is.EquivalentTo( new List<Rect> { item1, item2 } ) );
	}

	[Test]
	public void Query_QueryIntersectsContainedNode_AllContainedNodesReturned() {
		Rect item1 = new Rect( 5, 5, 40, 40 );
		_node.Insert( item1 );
		Rect item2 = new Rect( 10, 10, 30, 30 );
		_node.Insert( item2 );

		Rect query = new Rect( -5, -5, 15, 15 );

		IReadOnlyList<Rect> items = _node.Query( query );
		Assert.That( items, Is.EquivalentTo( new List<Rect> { item1, item2 } ) );
	}

	private static Rect SmallerBy(
		Rect rect,
		int amount
	) {
		return new Rect(
			rect.X1 + amount,
			rect.Y1 + amount,
			rect.Width - ( amount * 2 ),
			rect.Height - ( amount * 2 )
		);
	}

	private static Rect LargerBy(
		Rect rect,
		int amount
	) {
		return new Rect(
			rect.X1 - amount,
			rect.Y1 - amount,
			rect.Width + ( amount * 2 ),
			rect.Height + ( amount * 2 )
		);
	}
}
