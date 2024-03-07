namespace Kiyote.Geometry.Rasterizers;

public interface IRasterizer {

	void Rasterize(
		IReadOnlyList<Point> polygon,
		Action<int, int> pixelAction
	);

	void Rasterize(
		Point p1,
		Point p2,
		Action<int, int> pixelAction
	);
}
