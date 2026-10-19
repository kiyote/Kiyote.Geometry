namespace Kiyote.Geometry.Trees;

public interface IQuadTreeFactory {

	/// <summary>Creates a tree with a snapshot of nonempty signed-32-bit bounds, including one-cell-wide/high areas.</summary>
	IQuadTree<T> Create<T>(
		IRect area
	) where T: IRect;

}
