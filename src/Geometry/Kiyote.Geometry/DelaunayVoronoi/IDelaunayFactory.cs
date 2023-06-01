namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IDelaunayFactory {

	public IDelaunay Create( IReadOnlyList<Point> points );

	public IDelaunay Create( IReadOnlyList<Point> points, bool sanitizePoints );

}
