namespace Kiyote.Geometry.Trees;

public interface IQuadTree<T> where T : IRect {

	int Count { get; }

	void Insert( T item );

	IReadOnlyList<T> Query( Rect area );
}
