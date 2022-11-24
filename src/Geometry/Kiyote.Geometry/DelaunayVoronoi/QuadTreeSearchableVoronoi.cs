using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class QuadTreeSearchableVoronoi : ISearchableVoronoi {

	private readonly IQuadTree<IRect> _quadTree;
	private readonly Dictionary<IRect, Cell> _bounds;
	private readonly IVoronoi _voronoi;

	public QuadTreeSearchableVoronoi(
		IQuadTree<IRect> quadTree,
		IVoronoi voronoi
	) {
		_voronoi = voronoi;
		_quadTree = quadTree;
		_bounds = new Dictionary<IRect, Cell>();
		foreach( Cell cell in voronoi.Cells ) {
			_quadTree.Insert( cell.BoundingBox );
			_bounds[cell.BoundingBox] = cell;
		}
	}

	IReadOnlyList<Cell> IVoronoi.Cells => _voronoi.Cells;

	IReadOnlyList<Cell> IVoronoi.OpenCells => _voronoi.OpenCells;

	IReadOnlyList<Cell> IVoronoi.ClosedCells => _voronoi.ClosedCells;

	IReadOnlyList<IEdge> IVoronoi.Edges => _voronoi.Edges;

	IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> IVoronoi.Neighbours => _voronoi.Neighbours;

	IReadOnlyList<Cell> ISearchableVoronoi.Search(
		IRect area
	) {
		IReadOnlyList<IRect> result = _quadTree.Query( area );
		if( result.Any() ) {
			return result.Select( r => _bounds[r] ).ToList();
		}

		return Array.Empty<Cell>();
	}

	IReadOnlyList<Cell> ISearchableVoronoi.Search( int x, int y, int w, int h ) {
		IRect area = new Rect( x, y, x + w, y + h );
		return ( this as ISearchableVoronoi ).Search( area );
	}
}
