namespace Kiyote.Geometry.Trees;

public interface IQuadTree<T> where T : Rect {

	int Count { get; }

	void Insert( T item );

	IReadOnlyList<T> Query( Rect area );
}
