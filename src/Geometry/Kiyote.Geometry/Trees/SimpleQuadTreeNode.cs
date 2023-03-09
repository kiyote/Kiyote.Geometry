namespace Kiyote.Geometry.Trees;

public class QuadTreeNode<T> where T : Rect {
	/// <summary>
	/// Construct a quadtree node with the given bounds 
	/// </summary>
	/// <param name="area"></param>
	public QuadTreeNode( Rect bounds ) {
		_bounds = bounds;
	}

	/// <summary>
	/// The area of this node
	/// </summary>
	private readonly Rect _bounds;

	/// <summary>
	/// The contents of this node.
	/// Note that the contents have no limit: this is not the standard way to impement a QuadTree
	/// </summary>
	private readonly List<T> _contents = new List<T>();

	/// <summary>
	/// The child nodes of the QuadTree
	/// </summary>
	private readonly List<QuadTreeNode<T>> _nodes = new List<QuadTreeNode<T>>( 4 );

	/// <summary>
	/// Is the node empty
	/// </summary>
	public bool IsEmpty {
		get {
			return _bounds.Width == 0
				|| _bounds.Height == 0
				|| _nodes.Count == 0;
		}
	}

	/// <summary>
	/// Area of the quadtree node
	/// </summary>
	public Rect Bounds => _bounds;

	/// <summary>
	/// Total number of nodes in the this node and all SubNodes
	/// </summary>
	public int Count {
		get {
			int count = 0;

			foreach( QuadTreeNode<T> node in _nodes ) {
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
			List<T> results = new List<T>();

			foreach( QuadTreeNode<T> node in _nodes ) {
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
	public IReadOnlyList<T> Query( Rect queryArea ) {
		// create a list of the items that are found
		List<T> results = new List<T>();

		// this quad contains items that are not entirely contained by
		// it's four sub-quads. Iterate through the items in this quad 
		// to see if they intersect.
		foreach( T item in this.Contents ) {
			if( queryArea.Intersects( item ) ) {
				results.Add( item );
			}
		}

		foreach( QuadTreeNode<T> node in _nodes ) {
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
		if( !_bounds.Contains( item ) ) {
			//throw new InvalidOperationException( "feature is out of the bounds of this quadtree node" );
			//Trace.TraceWarning( "feature is out of the bounds of this quadtree node" );
			return;
		}

		// if the subnodes are null create them. may not be sucessfull: see below
		// we may be at the smallest allowed size in which case the subnodes will not be created
		if( _nodes.Count == 0 ) {
			CreateSubNodes();
		}

		// for each subnode:
		// if the node contains the item, add the item to that node and return
		// this recurses into the node that is just large enough to fit this item
		foreach( QuadTreeNode<T> node in _nodes ) {
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
		if( ( _bounds.Height * _bounds.Width ) <= 10 ) {
			return;
		}

		int halfWidth = ( _bounds.Width / 2 );
		int halfHeight = ( _bounds.Height / 2 );

		_nodes.Add( new QuadTreeNode<T>( new Rect( _bounds.TopLeft.X, _bounds.TopLeft.Y, _bounds.TopLeft.X + halfWidth, _bounds.TopLeft.Y + halfHeight ) ) );
		_nodes.Add( new QuadTreeNode<T>( new Rect( _bounds.TopLeft.X, _bounds.TopLeft.Y + halfHeight, _bounds.TopLeft.X + halfWidth, _bounds.TopLeft.Y + halfHeight + halfHeight ) ) );
		_nodes.Add( new QuadTreeNode<T>( new Rect( _bounds.TopLeft.X + halfWidth, _bounds.TopLeft.Y, _bounds.TopLeft.X + halfWidth + halfWidth, _bounds.TopLeft.Y + halfHeight ) ) );
		_nodes.Add( new QuadTreeNode<T>( new Rect( _bounds.TopLeft.X + halfWidth, _bounds.TopLeft.Y + halfHeight, _bounds.TopLeft.X + halfWidth + halfWidth, _bounds.TopLeft.Y + halfHeight + halfHeight ) ) );
	}
}
