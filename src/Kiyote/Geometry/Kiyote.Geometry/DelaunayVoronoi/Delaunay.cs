namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class Delaunay : IDelaunay {

	public Delaunay(
		IReadOnlyList<Point> points,
		IReadOnlyList<Triangle> triangles,
		IReadOnlyList<Point> hull
	) {
		Points = points;
		Triangles = triangles;
		Hull = hull;
	}

	public IReadOnlyList<Point> Points { get; }

	public IReadOnlyList<Triangle> Triangles { get; }

	public IReadOnlyList<Point> Hull { get; }
}
