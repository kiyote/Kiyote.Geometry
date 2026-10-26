namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class Voronoi : IVoronoi {
	private readonly int[] _neighbourOffsets;
	private readonly int[] _neighbourIndices;

	// Transfers factory-owned read-only storage; no mutable aliases may escape.
	public Voronoi(
		IReadOnlyList<Point> points,
		IReadOnlyList<Cell> cells,
		int[] neighbourOffsets,
		int[] neighbourIndices,
		IReadOnlyList<Edge> edges
	) {
		Points = points;
		Cells = cells;
		_neighbourOffsets = neighbourOffsets;
		_neighbourIndices = neighbourIndices;
		Edges = edges;
	}

	public IReadOnlyList<Point> Points { get; }

	public IReadOnlyList<Cell> Cells { get; }

	public ReadOnlySpan<int> GetNeighbourIndices( int cellIndex ) {
		ArgumentOutOfRangeException.ThrowIfNegative( cellIndex );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( cellIndex, Cells.Count );
		int start = _neighbourOffsets[cellIndex];
		return _neighbourIndices.AsSpan( start, _neighbourOffsets[cellIndex + 1] - start );
	}

	public IReadOnlyList<Edge> Edges { get; }
}
