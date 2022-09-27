namespace Kiyote.Geometry.Trees;

internal sealed class SimpleQuadTree<T> : IQuadTree<T> where T : IRect {
	/// <summary>
	/// The root QuadTreeNode
	/// </summary>
	private readonly QuadTreeNode<T> m_root;

	/// <summary>
	/// The bounds of this QuadTree
	/// </summary>
	private readonly IRect m_rectangle;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="rectangle"></param>
	public SimpleQuadTree( IRect rectangle ) {
		m_rectangle = rectangle;
		m_root = new QuadTreeNode<T>( m_rectangle );
	}

	/// <summary>
	/// Get the count of items in the QuadTree
	/// </summary>
	public int Count => m_root.Count;

	/// <summary>
	/// Insert the feature into the QuadTree
	/// </summary>
	/// <param name="item"></param>
	public void Insert( T item ) {
		m_root.Insert( item );
	}

	/// <summary>
	/// Query the QuadTree, returning the items that are in the given area
	/// </summary>
	/// <param name="area"></param>
	/// <returns></returns>
	public IReadOnlyList<T> Query( IRect area ) {
		return m_root.Query( area );
	}

	/*
	public void ForEach( Action<QuadTreeNode<T>> action ) {
		m_root.ForEach( action );
	}
	*/

}
