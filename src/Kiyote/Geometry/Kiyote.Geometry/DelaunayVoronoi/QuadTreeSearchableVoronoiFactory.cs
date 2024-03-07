using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class QuadTreeSearchableVoronoiFactory : ISearchableVoronoiFactory {

	private readonly IQuadTreeFactory _quadTreeFactory;

	public QuadTreeSearchableVoronoiFactory(
		IQuadTreeFactory quadTreeFactory
	) {
		_quadTreeFactory = quadTreeFactory;
	}

	ISearchableVoronoi ISearchableVoronoiFactory.Create(
		IVoronoi voronoi,
		ISize size
	) {
		return new QuadTreeSearchableVoronoi(
			_quadTreeFactory.Create<Rect>( new Rect( 0, 0, size.Width, size.Height ) ),
			voronoi
		);
	}
}
