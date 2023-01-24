namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IDelaunatorFactory {
	Delaunator Create( IEnumerable<Point> points );
}
