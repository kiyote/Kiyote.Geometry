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

	bool Intersects(
		IRect rect
	);
}
