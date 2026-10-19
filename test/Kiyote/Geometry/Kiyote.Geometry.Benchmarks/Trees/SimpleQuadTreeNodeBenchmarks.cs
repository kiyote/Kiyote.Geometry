
namespace Kiyote.Geometry.Trees.Benchmarks;

[MemoryDiagnoser( false )]
public class SimpleQuadTreeNodeBenchmarks {

	private readonly IRect _bounds;
	private readonly IRect _query;
	private readonly Rect _visitorQuery = new Rect( 250, 250, 20, 20 );
	private readonly SimpleQuadTreeNode<Rect> _node;
	private readonly Rect[] _items = new Rect[10000];
	private readonly Rect _insert;
	private SimpleQuadTreeNode<Rect> _insertionNode = null!;

	public SimpleQuadTreeNodeBenchmarks() {
		_bounds = new Rect( 0, 0, 1920, 1080 );
		_node = new SimpleQuadTreeNode<Rect>( _bounds );

		Random random = new Random( 0xBADF00D );
		for( int i = 0; i < _items.Length; i++ ) {
			int x = random.Next( 0, 980 );
			int y = random.Next( 0, 980 );
			int w = random.Next( 5, 20 );
			int h = random.Next( 5, 20 );
			_items[i] = new Rect( x, y, w, h );
			_node.Insert( _items[i] );
		}

		_query = new Rect( 250, 250, 20, 20 );
		_insert = new Rect( 500, 500, 500, 500 );
	}

	[GlobalSetup]
	public void Setup() {
		IEnumerable<Rect> expected = _items.Where( item =>
			item.X1 <= _query.X2 && item.X2 >= _query.X1
			&& item.Y1 <= _query.Y2 && item.Y2 >= _query.Y1 );
		if( _node.Count != _items.Length
			|| !Ordered( _node.GetSubTreeContents() ).SequenceEqual( Ordered( _items ) )
			|| !Ordered( _node.Query( _query ) ).SequenceEqual( Ordered( expected ) )
			|| QueryVisitor() != expected.Count() ) {
			throw new InvalidOperationException( "Quadtree benchmark differs from the seeded rectangle reference." );
		}
	}

	private static IOrderedEnumerable<Rect> Ordered( IEnumerable<Rect> values ) {
		return values.OrderBy( item => (item.X1, item.Y1, item.Width, item.Height) );
	}

	[Benchmark]
	public int Count() {
		return _node.Count;
	}

	[Benchmark]
	public IReadOnlyList<Rect> GetSubTreeContents() {
		return _node.GetSubTreeContents();
	}

	[IterationSetup( Target = nameof( Insert ) )]
	public void ResetInsertionTree() {
		_insertionNode = new SimpleQuadTreeNode<Rect>( _bounds );
		foreach( Rect item in _node.GetSubTreeContents() ) {
			_insertionNode.Insert( item );
		}
	}

	[Benchmark]
	public void Insert() {
		_insertionNode.Insert( _insert );
	}

	[Benchmark]
	public IReadOnlyList<Rect> Query() {
		return _node.Query( _query );
	}

	[Benchmark]
	public int QueryVisitor() {
		CountingVisitor visitor = default;
		_node.Query( _visitorQuery, ref visitor );
		return visitor.Count;
	}

	private struct CountingVisitor : IQueryVisitor<Rect> {
		public int Count;
		public void Visit( scoped in Rect item ) => Count++;
	}
}
