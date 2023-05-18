using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class QuadTreeSearchableVoronoiUnitTests {

	private Rect _area;
	private IVoronoi _voronoi;
	private ISearchableVoronoi _searchableVoronoi;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		Bounds bounds = new Bounds( 100, 100 );
		_area = new Rect( 0, 0, bounds );
		IReadOnlyList<Point> points = new List<Point>() {
			new Point( 25, 25 ),
			new Point( 75, 25 ),
			//new Point( 50, 50 ),
			new Point( 25, 75 ),
			new Point( 75, 75 )
		};
		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		Delaunator delaunator = delaunatorFactory.Create( points );
		IVoronoiFactory voronoiFactory = new VoronoiFactory();
		_voronoi = voronoiFactory.Create( delaunator, bounds );
	}

	[SetUp]
	public void SetUp() {
		IQuadTree<Rect> quadTree = new SimpleQuadTree<Rect>( _area );
		_searchableVoronoi = new QuadTreeSearchableVoronoi( quadTree, _voronoi );
	}

	/*
	[TestCase( -1, -1, 52, 52, 25, 25 )]
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
	*/
}
