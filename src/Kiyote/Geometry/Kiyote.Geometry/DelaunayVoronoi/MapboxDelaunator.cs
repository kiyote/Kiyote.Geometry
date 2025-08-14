namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class MapboxDelaunator {

	public MapboxDelaunator(
		int[] hull,
		int[] triangles,
		int[] halfEdges
	) {
		Hull = hull;
		Triangles = triangles;
		HalfEdges = halfEdges;
	}

	public int[] Hull { get; }

	public int[] Triangles { get; }

	public int[] HalfEdges { get; }
}
