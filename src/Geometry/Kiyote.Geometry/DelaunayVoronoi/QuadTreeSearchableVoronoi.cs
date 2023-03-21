using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi;

internal sealed class QuadTreeSearchableVoronoi : ISearchableVoronoi {

	private readonly IQuadTree<Rect> _quadTree;
	private readonly Dictionary<Rect, Cell> _bounds;
	private readonly IVoronoi _voronoi;

	public QuadTreeSearchableVoronoi(
		IQuadTree<Rect> quadTree,
		IVoronoi voronoi
	) {
		_voronoi = voronoi;
		_quadTree = quadTree;
		_bounds = new Dictionary<Rect, Cell>();
		foreach( Cell cell in voronoi.Cells ) {
			_quadTree.Insert( cell.BoundingBox );
			_bounds[cell.BoundingBox] = cell;
		}
	}

	IReadOnlyList<Cell> IVoronoi.Cells => _voronoi.Cells;

	IReadOnlyList<Cell> IVoronoi.OpenCells => _voronoi.OpenCells;

	IReadOnlyList<Cell> IVoronoi.ClosedCells => _voronoi.ClosedCells;

	IReadOnlyList<Edge> IVoronoi.Edges => _voronoi.Edges;

	IReadOnlyDictionary<Cell, IReadOnlyList<Cell>> IVoronoi.Neighbours => _voronoi.Neighbours;

	IReadOnlyList<Cell> ISearchableVoronoi.Search(
		Rect area
	) {
		IReadOnlyList<Rect> result = _quadTree.Query( area );
		if( result.Any() ) {
			return result.Select( r => _bounds[r] ).ToList();
		}

		return Array.Empty<Cell>();
	}

	IReadOnlyList<Cell> ISearchableVoronoi.Search( int x, int y, int w, int h ) {
		Rect area = new Rect( x, y, w, h );
		return ( this as ISearchableVoronoi ).Search( area );
	}
}
