namespace Kiyote.Geometry.Trees;

internal sealed class SimpleQuadTreeFactory : IQuadTreeFactory {

	IQuadTree<T> IQuadTreeFactory.Create<T>( Rect area ) {
		return new SimpleQuadTree<T>( area );
	}
}
