using System.Linq;
using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class QuadTreeSearchableVoronoiUnitTests {

	private Rect _area;
	private IVoronoi _voronoi;
	private ISearchableVoronoi _searchableVoronoi;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		ISize size = new Point( 100, 100 );
		_area = new Rect( 0, 0, size );
		IReadOnlyList<Point> points = [
			new Point( 25, 25 ),
			new Point( 75, 25 ),
			new Point( 25, 75 ),
			new Point( 75, 75 )
		];
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
		Assert.That( results.Count, Is.EqualTo( 1 ) );
		Cell cell = results[0];
		Assert.That( cell.Center.X, Is.EqualTo( cx ) );
		Assert.That( cell.Center.Y, Is.EqualTo( cy ) );
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
		Assert.That( results.Count, Is.EqualTo( 2 ) );
		Cell cell = results[0];
		Assert.That( cell.Center.X, Is.EqualTo( cx1 ) );
		Assert.That( cell.Center.Y, Is.EqualTo( cy1 ) );
		cell = results[1];
		Assert.That( cell.Center.X, Is.EqualTo( cx2 ) );
		Assert.That( cell.Center.Y, Is.EqualTo( cy2 ) );
	}
}

