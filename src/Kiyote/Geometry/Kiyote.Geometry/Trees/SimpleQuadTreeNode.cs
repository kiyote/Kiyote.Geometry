namespace Kiyote.Geometry.Trees;

public class SimpleQuadTreeNode<T> where T : IRect {

	public const int SmallestAllowedArea = 10;

	private readonly int _width;
	private readonly int _height;
	private readonly List<T> _contents;
	private readonly List<SimpleQuadTreeNode<T>> _nodes;

	/// <summary>
	/// Construct a quadtree node with the given bounds 
	/// </summary>
	/// <param name="area"></param>
	public SimpleQuadTreeNode(
		IRect bounds
	) {
		if (bounds.X2 - bounds.X1 == 0
			|| bounds.Y2 - bounds.Y1 == 0
		) {
			throw new InvalidOperationException( "Cannot define quad tree with zero area." );
		}
		Bounds = bounds;
		_width = bounds.X2 - bounds.X1;
		_height = bounds.Y2 - bounds.Y1;
		_contents = [];
		_nodes = new List<SimpleQuadTreeNode<T>>( 4 );
	}

	/// <summary>
	/// Is the node empty
	/// </summary>
	public bool IsEmpty => _nodes.Count == 0;

	/// <summary>
	/// Area of the quadtree node
	/// </summary>
	public IRect Bounds { get; }

	/// <summary>
	/// Total number of nodes in the this node and all SubNodes
	/// </summary>
	public int Count {
		get {
			int count = 0;

			foreach( SimpleQuadTreeNode<T> node in _nodes ) {
				count += node.Count;
			}

			count += this.Contents.Count;

			return count;
		}
	}

	/// <summary>
	/// Return the contents of this node and all subnodes in the true below this one.
	/// </summary>
	public IReadOnlyList<T> SubTreeContents {
		get {
			List<T> results = [];

			foreach( SimpleQuadTreeNode<T> node in _nodes ) {
				results.AddRange( node.SubTreeContents );
			}

			results.AddRange( this.Contents );
			return results;
		}
	}

	public IReadOnlyList<T> Contents => _contents;

	/// <summary>
	/// Query the QuadTree for items that are in the given area
	/// </summary>
	/// <param name="queryArea"></pasram>
	/// <returns></returns>
	public IReadOnlyList<T> Query( IRect queryArea ) {
		// create a list of the items that are found
		List<T> results = [];

		// this quad contains items that are not entirely contained by
		// its four sub-quads. Iterate through the items in this quad 
		// to see if they intersect.
		foreach( T item in Contents ) {
			if( queryArea.Intersects( item ) ) {
				results.Add( item );
			}
		}

		foreach( SimpleQuadTreeNode<T> node in _nodes ) {
			if( node.IsEmpty ) {
				continue;
			}

			// Case 1: search area completely contained by sub-quad
			// if a node completely contains the query area, go down that branch
			// and skip the remaining nodes (break this loop)
			if( node.Bounds.Contains( queryArea ) ) {
				results.AddRange( node.Query( queryArea ) );
				break;
			}

			// Case 2: Sub-quad completely contained by search area 
			// if the query area completely contains a sub-quad,
			// just add all the contents of that quad and it's children 
			// to the result set. You need to continue the loop to test 
			// the other quads
			if( queryArea.Contains( node.Bounds ) ) {
				results.AddRange( node.SubTreeContents );
				continue;
			}

			// Case 3: search area intersects with sub-quad
			// traverse into this quad, continue the loop to search other
			// quads
			if( node.Bounds.Intersects( queryArea ) ) {
				results.AddRange( node.Query( queryArea ) );
			}
		}

		return results;
	}

	/// <summary>
	/// Insert an item to this node
	/// </summary>
	/// <param name="item"></param>
	public void Insert( T item ) {
		// if the item is not contained in this quad, there's a problem
		if( !Bounds.Contains( item ) ) {
			//throw new InvalidOperationException( "feature is out of the bounds of this quadtree node" );
			//Trace.TraceWarning( "feature is out of the bounds of this quadtree node" );
			return;
		}

		// if the subnodes are null create them. may not be sucessful: see below
		// we may be at the smallest allowed size in which case the subnodes will not be created
		if( _nodes.Count == 0 ) {
			CreateSubNodes();
		}

		// for each subnode:
		// if the node contains the item, add the item to that node and return
		// this recurses into the node that is just large enough to fit this item
		foreach( SimpleQuadTreeNode<T> node in _nodes ) {
			if( node.Bounds.Contains( item ) ) {
				node.Insert( item );
				return;
			}
		}

		// if we make it to here, either
		// 1) none of the subnodes completely contained the item. or
		// 2) we're at the smallest subnode size allowed 
		// add the item to this node's contents.
		_contents.Add( item );
	}

	/// <summary>
	/// Internal method to create the subnodes (partitions space)
	/// </summary>
	private void CreateSubNodes() {
		// the smallest subnode has an area
		if( ( _height * _width ) <= SmallestAllowedArea ) {
			return;
		}

		int halfWidth = ( _width / 2 );
		int halfHeight = ( _height / 2 );

		_nodes.Add( new SimpleQuadTreeNode<T>(
			new Rect(
				new Point( Bounds.X1, Bounds.Y1 ),
				new Point( Bounds.X1 + halfWidth, Bounds.Y1 + halfHeight )
			)
		) );
		_nodes.Add( new SimpleQuadTreeNode<T>(
			new Rect(
				new Point( Bounds.X1, Bounds.Y1 + halfHeight ),
				new Point( Bounds.X1 + halfWidth, Bounds.Y1 + halfHeight + halfHeight )
			)
		) );
		_nodes.Add( new SimpleQuadTreeNode<T>(
			new Rect(
				new Point( Bounds.X1 + halfWidth, Bounds.Y1 ),
				new Point( Bounds.X1 + halfWidth + halfWidth, Bounds.Y1 + halfHeight )
			)
		) );
		_nodes.Add( new SimpleQuadTreeNode<T>(
			new Rect(
				new Point( Bounds.X1 + halfWidth, Bounds.Y1 + halfHeight ),
				new Point( Bounds.X1 + halfWidth + halfWidth, Bounds.Y1 + halfHeight + halfHeight )
			)
		) );
	}
}
