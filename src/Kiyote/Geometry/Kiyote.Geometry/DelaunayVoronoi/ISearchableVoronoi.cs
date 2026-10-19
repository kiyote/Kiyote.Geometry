using Kiyote.Geometry.Trees;

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

	/// <summary>
	/// Visits matching cells directly without allocating result collections.
	/// Caller-owned visitor state, including changes before an exception, is retained.
	/// </summary>
	void Search<TVisitor>( Rect area, ref TVisitor visitor )
		where TVisitor : struct, IQueryVisitor<Cell>, allows ref struct;
}
