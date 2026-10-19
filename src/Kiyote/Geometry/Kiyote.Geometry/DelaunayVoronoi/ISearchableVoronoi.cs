namespace Kiyote.Geometry.DelaunayVoronoi;

/// <summary>Searches inclusive cell bounding boxes, not exact polygon intersections.</summary>
public interface ISearchableVoronoi : IVoronoi {

	/// <summary>Returns cells whose bounding boxes share an occupied cell with the area; empty areas return no cells.</summary>
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
