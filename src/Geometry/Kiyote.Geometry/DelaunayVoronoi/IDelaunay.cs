namespace Kiyote.Geometry.DelaunayVoronoi; 

public interface IDelaunay {
	IReadOnlyList<Point> Points { get; }

	IReadOnlyList<Triangle> Triangles { get; }

	IReadOnlyList<Point> Hull { get; }
}
