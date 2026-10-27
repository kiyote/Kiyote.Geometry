using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class QuadTreeSearchableVoronoi : ISearchableVoronoi {

	private readonly IQuadTree<IndexedCell> _quadTree;
	private readonly IVoronoi _voronoi;

	public QuadTreeSearchableVoronoi(
		IQuadTree<IndexedCell> quadTree,
		IVoronoi voronoi
	) {
		_voronoi = voronoi;
		_quadTree = quadTree;
		ReadOnlySpan<Cell> cells = voronoi.Cells;
		for( int i = 0; i < cells.Length; i++ ) {
			_quadTree.Insert( new IndexedCell( i, cells[i].BoundingBox ) );
		}
	}

	IReadOnlyList<Point> IVoronoi.Points => _voronoi.Points;

	ReadOnlySpan<Cell> IVoronoi.Cells => _voronoi.Cells;

	IReadOnlyList<Edge> IVoronoi.Edges => _voronoi.Edges;

	ReadOnlySpan<int> IVoronoi.GetNeighbourIndices( int cellIndex ) => _voronoi.GetNeighbourIndices( cellIndex );

	ReadOnlySpan<Point> IVoronoi.GetCellVertices( int cellIndex ) => _voronoi.GetCellVertices( cellIndex );

	bool IVoronoi.Contains( int cellIndex, Point point ) => _voronoi.Contains( cellIndex, point );

	Polygon IVoronoi.ExportPolygon( int cellIndex ) => _voronoi.ExportPolygon( cellIndex );

	IReadOnlyList<int> ISearchableVoronoi.Search(
		Rect area
	) {
		ListQueryVisitor<int> visitor = default;
		Search( area, ref visitor );
		return visitor.Results;
	}

	public void Search<TVisitor>( Rect area, ref TVisitor visitor )
		where TVisitor : struct, IQueryVisitor<int>, allows ref struct {
		CellVisitor<TVisitor> adapter = new CellVisitor<TVisitor>( visitor );
		try {
			_quadTree.Query( area, ref adapter );
		} finally {
			visitor = adapter.Visitor;
		}
	}

	IReadOnlyList<int> ISearchableVoronoi.Search( int x, int y, int w, int h ) {
		Rect area = new Rect( x, y, w, h );
		return ( this as ISearchableVoronoi ).Search( area );
	}

	private ref struct CellVisitor<TVisitor>( TVisitor visitor ) : IQueryVisitor<IndexedCell>
		where TVisitor : struct, IQueryVisitor<int>, allows ref struct {
		internal TVisitor Visitor = visitor;

		public void Visit( scoped in IndexedCell item ) {
			int cellIndex = item.CellIndex;
			Visitor.Visit( in cellIndex );
		}
	}
}
