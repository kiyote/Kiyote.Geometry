namespace Kiyote.Geometry.DelaunayVoronoi;

public interface ISearchableVoronoiFactory {

	ISearchableVoronoi Create( IVoronoi voronoi, ISize size );
}
