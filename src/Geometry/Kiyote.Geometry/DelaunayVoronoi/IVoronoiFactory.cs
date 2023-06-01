namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IVoronoiFactory {

	IVoronoi Create(
		IRect bounds,
		IReadOnlyList<Point> points
	);

	IVoronoi Create(
		IRect bounds,
		IReadOnlyList<Point> points,
		bool sanitizePoints
	);

}
