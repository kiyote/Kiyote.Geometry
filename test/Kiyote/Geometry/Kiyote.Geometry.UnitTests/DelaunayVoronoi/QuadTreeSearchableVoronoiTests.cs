using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.Trees;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class QuadTreeSearchableVoronoiTests {

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
		IQuadTree<IndexedCell> quadTree = new SimpleQuadTree<IndexedCell>( _area );
		_searchableVoronoi = new QuadTreeSearchableVoronoi( quadTree, _voronoi );
	}

	[Test]
	public void Ctor_ValidParameter_PropertiesSet() {
		IVoronoi voronoi = _searchableVoronoi;

		Assert.That( voronoi.Points, Is.SameAs( _voronoi.Points ) );
		Assert.That( voronoi.Cells.SequenceEqual( _voronoi.Cells ), Is.True );
		Assert.That( voronoi.Edges, Is.SameAs( _voronoi.Edges ) );
		for( int i = 0; i < _voronoi.Cells.Length; i++ ) {
			Assert.That( _searchableVoronoi.GetNeighbourIndices( i ).SequenceEqual( _voronoi.GetNeighbourIndices( i ) ), Is.True );
			Assert.That( _searchableVoronoi.GetCellVertices( i ).SequenceEqual( _voronoi.GetCellVertices( i ) ), Is.True );
			Assert.That( _searchableVoronoi.Contains( i, _voronoi.Points[i] ), Is.True );
			Assert.That( _searchableVoronoi.ExportPolygon( i ).Points, Is.EqualTo( _voronoi.GetCellVertices( i ).ToArray() ) );
		}
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
		IReadOnlyList<int> results = _searchableVoronoi.Search( x, y, w, h );
		Assert.That( results.Count, Is.EqualTo( 1 ) );
		Point center = _searchableVoronoi.Points[results[0]];
		Assert.That( center.X, Is.EqualTo( cx ) );
		Assert.That( center.Y, Is.EqualTo( cy ) );
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
		IReadOnlyList<int> results = _searchableVoronoi.Search( x, y, w, h );
		Assert.That( results.Count, Is.EqualTo( 2 ) );
		Point center = _searchableVoronoi.Points[results[0]];
		Assert.That( center.X, Is.EqualTo( cx1 ) );
		Assert.That( center.Y, Is.EqualTo( cy1 ) );
		center = _searchableVoronoi.Points[results[1]];
		Assert.That( center.X, Is.EqualTo( cx2 ) );
		Assert.That( center.Y, Is.EqualTo( cy2 ) );
	}
}
