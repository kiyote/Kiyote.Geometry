namespace Kiyote.Geometry.Trees;

internal sealed class SimpleQuadTreeFactory : IQuadTreeFactory {
	IQuadTree<T> IQuadTreeFactory.Create<T>( IRect area ) {
		return new SimpleQuadTree<T>( area );
	}
}
