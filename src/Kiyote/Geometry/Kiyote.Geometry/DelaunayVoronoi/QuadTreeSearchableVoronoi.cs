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
		foreach( Cell cell in voronoi.Cells ) {
			_quadTree.Insert( new IndexedCell( cell ) );
		}
	}

	IReadOnlyList<Point> IVoronoi.Points => _voronoi.Points;

	IReadOnlyList<Cell> IVoronoi.Cells => _voronoi.Cells;

	IReadOnlyList<Edge> IVoronoi.Edges => _voronoi.Edges;

	IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> IVoronoi.Neighbours => _voronoi.Neighbours;

	IReadOnlyList<Cell> ISearchableVoronoi.Search(
		Rect area
	) {
		ListQueryVisitor<Cell> visitor = default;
		Search( area, ref visitor );
		return visitor.Results;
	}

	public void Search<TVisitor>( Rect area, ref TVisitor visitor )
		where TVisitor : struct, IQueryVisitor<Cell>, allows ref struct {
		CellVisitor<TVisitor> adapter = new CellVisitor<TVisitor>( visitor );
		try {
			_quadTree.Query( area, ref adapter );
		} finally {
			visitor = adapter.Visitor;
		}
	}

	IReadOnlyList<Cell> ISearchableVoronoi.Search( int x, int y, int w, int h ) {
		Rect area = new Rect( x, y, w, h );
		return ( this as ISearchableVoronoi ).Search( area );
	}

	private ref struct CellVisitor<TVisitor>( TVisitor visitor ) : IQueryVisitor<IndexedCell>
		where TVisitor : struct, IQueryVisitor<Cell>, allows ref struct {
		internal TVisitor Visitor = visitor;

		public void Visit( scoped in IndexedCell item ) {
			Cell cell = item.Cell;
			Visitor.Visit( in cell );
		}
	}
}
