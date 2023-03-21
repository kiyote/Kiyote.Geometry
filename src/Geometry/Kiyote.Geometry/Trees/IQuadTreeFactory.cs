namespace Kiyote.Geometry.Trees;

public interface IQuadTreeFactory {

	IQuadTree<T> Create<T>( IRect area ) where T: IRect;
}
