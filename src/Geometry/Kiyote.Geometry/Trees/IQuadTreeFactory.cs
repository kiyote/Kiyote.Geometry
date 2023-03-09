namespace Kiyote.Geometry.Trees;

public interface IQuadTreeFactory {

	IQuadTree<T> Create<T>( Rect area ) where T: Rect;
}
