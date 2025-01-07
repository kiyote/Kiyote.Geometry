namespace Kiyote.Geometry.DelaunayVoronoi;

public interface ISearchableVoronoi : IVoronoi {

	IReadOnlyList<Cell> Search(
		Rect area
	);

	IReadOnlyList<Cell> Search(
		int x,
		int y,
		int w,
		int h
	);
}

