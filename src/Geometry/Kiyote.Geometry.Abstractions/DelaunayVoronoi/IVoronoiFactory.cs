namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IVoronoiFactory {

	IVoronoi Create(
		Rect bounds,
		IReadOnlyList<Point> points
	);

	IVoronoi Create(
		Rect bounds,
		IReadOnlyList<Point> points,
		bool sanitizePoints
	);

}
