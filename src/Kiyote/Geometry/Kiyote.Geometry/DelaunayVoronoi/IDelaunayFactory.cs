namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IDelaunayFactory {

	IDelaunay Create(
		IReadOnlyList<Point> points
	);

	IDelaunay Create(
		IReadOnlyList<Point> points,
		bool sanitizePoints
	);

}
