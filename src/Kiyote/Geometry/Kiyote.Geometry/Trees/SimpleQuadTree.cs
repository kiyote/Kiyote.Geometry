namespace Kiyote.Geometry.Trees;

internal sealed class SimpleQuadTree<T> : IQuadTree<T> where T : IRect {
	/// <summary>
	/// The root QuadTreeNode
	/// </summary>
	private readonly SimpleQuadTreeNode<T> _root;

	/// <summary>
	/// The bounds of this QuadTree
	/// </summary>
	private readonly IRect _bounds;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="bounds"></param>
	public SimpleQuadTree( IRect bounds ) {
		_bounds = bounds;
		_root = new SimpleQuadTreeNode<T>( _bounds );
	}

	/// <summary>
	/// Get the count of items in the QuadTree
	/// </summary>
	public int Count => _root.Count;

	/// <summary>
	/// Insert the feature into the QuadTree
	/// </summary>
	/// <param name="item"></param>
	public void Insert( T item ) {
		_root.Insert( item );
	}

	/// <summary>
	/// Query the QuadTree, returning the items that are in the given area
	/// </summary>
	/// <param name="bounds"></param>
	/// <returns></returns>
	public IReadOnlyList<T> Query( IRect bounds ) {
		return _root.Query( bounds );
	}
}
