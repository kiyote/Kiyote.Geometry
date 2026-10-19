namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class MapboxDelaunator {
	private readonly int[] _triangles;
	private readonly int[] _halfEdges;
	private readonly int _length;

	public MapboxDelaunator(
		int[] hull,
		int[] triangles,
		int[] halfEdges,
		int length
	) {
		Hull = hull;
		_triangles = triangles;
		_halfEdges = halfEdges;
		_length = length;
	}

	public int[] Hull { get; }

	public ReadOnlySpan<int> Triangles => _triangles.AsSpan( 0, _length );

	public ReadOnlySpan<int> HalfEdges => _halfEdges.AsSpan( 0, _length );
}
