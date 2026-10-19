namespace Kiyote.Geometry.Trees;

/// <summary>Indexes nonempty inclusive rectangles. Item bounds must remain unchanged while indexed.</summary>
public interface IQuadTree<T> where T : IRect {

	/// <summary>Number of inserted entries, including repeated values.</summary>
	int Count { get; }

	/// <summary>Inserts an entry entirely contained by the tree's bounds.</summary>
	/// <exception cref="ArgumentException">The item is empty or extends outside the bounds; the tree is unchanged.</exception>
	/// <exception cref="OverflowException">The total entry count cannot fit a signed 32-bit integer.</exception>
	void Insert(
		T item
	);

	/// <summary>Returns entries sharing an occupied cell with the area. Empty queries return no entries.</summary>
	IReadOnlyList<T> Query(
		IRect area
	);
}
