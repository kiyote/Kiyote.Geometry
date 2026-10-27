namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class Voronoi : IVoronoi {
	private readonly int[] _neighbourOffsets;
	private readonly int[] _neighbourIndices;
	private readonly Cell[] _cells;
	private readonly Point[] _vertices;

	// Transfers factory-owned read-only storage; no mutable aliases may escape.
	public Voronoi(
		IReadOnlyList<Point> points,
		Cell[] cells,
		Point[] vertices,
		int[] neighbourOffsets,
		int[] neighbourIndices,
		IReadOnlyList<Edge> edges
	) {
		Points = points;
		_cells = cells;
		_vertices = vertices;
		_neighbourOffsets = neighbourOffsets;
		_neighbourIndices = neighbourIndices;
		Edges = edges;
	}

	public IReadOnlyList<Point> Points { get; }

	public ReadOnlySpan<Cell> Cells => _cells;

	public ReadOnlySpan<Point> GetCellVertices( int cellIndex ) {
		ValidateCellIndex( cellIndex );
		ref readonly Cell cell = ref _cells[cellIndex];
		return _vertices.AsSpan( cell.VertexOffset, cell.VertexCount );
	}

	public bool Contains( int cellIndex, Point point ) {
		ReadOnlySpan<Point> vertices = GetCellVertices( cellIndex );
		GeometryLimits.Validate( point, nameof( point ) );
		return Polygon.ContainsConvex( vertices, 1, point );
	}

	public Polygon ExportPolygon( int cellIndex ) => Polygon.FromNormalizedVertices( GetCellVertices( cellIndex ) );

	public ReadOnlySpan<int> GetNeighbourIndices( int cellIndex ) {
		ValidateCellIndex( cellIndex );
		int start = _neighbourOffsets[cellIndex];
		return _neighbourIndices.AsSpan( start, _neighbourOffsets[cellIndex + 1] - start );
	}

	public IReadOnlyList<Edge> Edges { get; }

	private void ValidateCellIndex( int cellIndex ) {
		ArgumentOutOfRangeException.ThrowIfNegative( cellIndex );
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual( cellIndex, _cells.Length );
	}
}
