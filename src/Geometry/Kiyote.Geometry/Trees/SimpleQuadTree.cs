namespace Kiyote.Geometry.Trees;

internal sealed class SimpleQuadTree<T> : IQuadTree<T> where T : Rect {
	/// <summary>
	/// The root QuadTreeNode
	/// </summary>
	private readonly SimpleQuadTreeNode<T> _root;

	/// <summary>
	/// The bounds of this QuadTree
	/// </summary>
	private readonly Rect _rectangle;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="rectangle"></param>
	public SimpleQuadTree( Rect rectangle ) {
		_rectangle = rectangle;
		_root = new SimpleQuadTreeNode<T>( _rectangle );
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
	/// <param name="area"></param>
	/// <returns></returns>
	public IReadOnlyList<T> Query( Rect area ) {
		return _root.Query( area );
	}
}
