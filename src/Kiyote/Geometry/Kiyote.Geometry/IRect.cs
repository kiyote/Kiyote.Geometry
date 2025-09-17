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
