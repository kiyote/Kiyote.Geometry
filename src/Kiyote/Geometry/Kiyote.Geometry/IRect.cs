namespace Kiyote.Geometry;

/// <summary>
/// Inclusive occupied-cell bounds in signed 32-bit coordinate space.
/// Empty rectangles contain no points or rectangles and never overlap or touch.
/// Nonempty implementations must expose ordered bounds and positive representable
/// dimensions consistent with those bounds.
/// </summary>
public interface IRect {

	/// <summary>True when either dimension is zero.</summary>
	bool IsEmpty { get; }

	int Height { get; }
	int Width { get; }
	int X1 { get; }
	int X2 { get; }
	int Y1 { get; }
	int Y2 { get; }

	/// <summary>Returns whether this nonempty rectangle contains the occupied cell.</summary>
	bool Contains(
		int x,
		int y
	);

	bool Contains(
		Point point
	);

	/// <summary>Returns whether every cell of a nonempty rectangle is contained.</summary>
	bool Contains(
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

	/// <summary>Returns whether occupied bounds agree; two empty rectangles are equivalent.</summary>
	bool IsEquivalentTo(
		IRect other
	);

	/// <summary>Compares with the nonempty inclusive bounds defined by two endpoints.</summary>
	bool IsEquivalentTo(
		Point topLeft,
		Point bottomRight
	);

	/// <summary>Compares with the nonempty inclusive bounds defined by two endpoints.</summary>
	bool IsEquivalentTo(
		int x1,
		int y1,
		int x2,
		int y2
	);

}
