using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class QuadTreeSearchableVoronoiUnitTests {

	private Rect _area;
	private IVoronoi _voronoi;
	private ISearchableVoronoi _searchableVoronoi;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		Bounds size = new Bounds( 100, 100 );
		_area = new Rect( 0, 0, size );
		IReadOnlyList<Point> points = new List<Point>() {
			new Point( 25, 25 ),
			new Point( 75, 25 ),
			new Point( 25, 75 ),
			new Point( 75, 75 )
		};
		IVoronoiFactory voronoiFactory = new D3VoronoiFactory();
		_voronoi = voronoiFactory.Create( _area, points );
	}

	[SetUp]
	public void SetUp() {
		IQuadTree<Rect> quadTree = new SimpleQuadTree<Rect>( _area );
		_searchableVoronoi = new QuadTreeSearchableVoronoi( quadTree, _voronoi );
	}

	[TestCase( 1, 1, 48, 48, 25, 25 )]
	[TestCase( 51, 1, 48, 48, 75, 25 )]
	[TestCase( 1, 51, 48, 48, 25, 75 )]
	[TestCase( 51, 51, 48, 48, 75, 75 )]
	public void Search_SingleQuadrant_PointReturned(
		int x,
		int y,
		int w,
		int h,
		int cx,
		int cy
	) {
		IReadOnlyList<Cell> results = _searchableVoronoi.Search( x, y, w, h );
		Assert.AreEqual( 1, results.Count );
		Cell cell = results[0];
		Assert.AreEqual( cx, cell.Center.X );
		Assert.AreEqual( cy, cell.Center.Y );
	}

	[TestCase( 1, 1, 48, 98, 25, 25, 25, 75 )]
	[TestCase( 51, 1, 48, 98, 75, 25, 75, 75 )]
	[TestCase( 1, 1, 98, 48, 25, 25, 75, 25 )]
	[TestCase( 1, 51, 98, 98, 25, 75, 75, 75 )]
	public void Search_TwoQuadrants_TwoPointsReturned(
		int x,
		int y,
		int w,
		int h,
		int cx1,
		int cy1,
		int cx2,
		int cy2
	) {
		IReadOnlyList<Cell> results = _searchableVoronoi.Search( x, y, w, h );
		Assert.AreEqual( 2, results.Count );
		Cell cell = results[0];
		Assert.AreEqual( cx1, cell.Center.X );
		Assert.AreEqual( cy1, cell.Center.Y );
		cell = results[1];
		Assert.AreEqual( cx2, cell.Center.X );
		Assert.AreEqual( cy2, cell.Center.Y );
	}
}

