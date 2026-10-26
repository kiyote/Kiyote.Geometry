namespace Kiyote.Geometry.DelaunayVoronoi.D3;

internal sealed class D3Delaunay {

	public D3Delaunay(
		int[] inedges,
		int[] hullIndex
	) {
		Inedges = inedges;
		HullIndex = hullIndex;
	}

	public int[] Inedges { get; }

	public int[] HullIndex { get; }
}

