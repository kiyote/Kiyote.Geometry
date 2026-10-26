namespace Kiyote.Geometry.DelaunayVoronoi.D3;

internal readonly ref struct D3Delaunay {

	public D3Delaunay(
		ReadOnlySpan<int> inedges,
		ReadOnlySpan<int> hullIndex
	) {
		Inedges = inedges;
		HullIndex = hullIndex;
	}

	public ReadOnlySpan<int> Inedges { get; }

	public ReadOnlySpan<int> HullIndex { get; }
}
