namespace Kiyote.Geometry; 

public interface IRect {

	int Height { get; }
	int Width { get; }
	int X1 { get; }
	int X2 { get; }
	int Y1 { get; }
	int Y2 { get; }

	bool Contains(
		int x,
		int y
	);

	bool Contains(
		Point point
	);

	bool Contains(
		IRect rect
	);

	bool HasOverlap(
		IRect rect
	);

	/// <summary>
	/// Returns true when the two rectangles share at least one cell.
	/// </summary>
	bool Overlaps(
		IRect rect
	);

	/// <summary>
	/// Returns true when the two rectangles share at least one cell or are
	/// adjacent, including diagonally.
	/// </summary>
	bool Touches(
		IRect rect
	);

	bool IsEquivalentTo(
		IRect other
	);

	bool IsEquivalentTo(
		Point topLeft,
		Point bottomRight
	);

	bool IsEquivalentTo(
		int x1,
		int y1,
		int x2,
		int y2
	);

}
