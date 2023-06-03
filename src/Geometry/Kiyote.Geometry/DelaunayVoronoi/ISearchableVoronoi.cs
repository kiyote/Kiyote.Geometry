namespace Kiyote.Geometry.DelaunayVoronoi;

public interface ISearchableVoronoi : IVoronoi {
	IReadOnlyList<Cell> Search( IRect area );

	IReadOnlyList<Cell> Search( int x, int y, int w, int h );
}

