using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.Trees;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class SearchableVoronoiRegressionTests {

	[Test]
	public void Search_ManySmallCells_MatchesBoundingBoxReference() {
		Rect bounds = new Rect( 0, 0, 41, 41 );
		Point[] sites = new Point[400];
		for( int i = 0; i < sites.Length; i++ ) {
			sites[i] = new Point( 1 + (i % 20 * 2), 1 + (i / 20 * 2) );
		}
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( bounds, sites );
		ISearchableVoronoiFactory searchableFactory = new QuadTreeSearchableVoronoiFactory( new SimpleQuadTreeFactory() );
		ISearchableVoronoi searchable = searchableFactory.Create( diagram, new Point( bounds.Width, bounds.Height ) );
		List<Rect> queries = [bounds, default, new Rect( 19, 0, 2, 41 ), new Rect( 41, 0, 1, 41 )];
		foreach( Point site in sites ) {
			queries.Add( new Rect( site.X, site.Y, 1, 1 ) );
		}
		Random random = new Random( 8129 );
		for( int i = 0; i < 250; i++ ) {
			queries.Add( new Rect( random.Next( -10, 60 ), random.Next( -10, 60 ),
				random.Next( 0, 20 ), random.Next( 0, 20 ) ) );
		}
		foreach( Rect query in queries ) {
			int[] expected = Enumerable.Range( 0, diagram.Cells.Length ).Where( i => !query.IsEmpty
				&& diagram.Cells[i].BoundingBox.X1 <= query.X2 && diagram.Cells[i].BoundingBox.X2 >= query.X1
				&& diagram.Cells[i].BoundingBox.Y1 <= query.Y2 && diagram.Cells[i].BoundingBox.Y2 >= query.Y1 ).ToArray();
			string context = $"Query ({query.X1},{query.Y1}), {query.Width}x{query.Height}";
			Assert.That( searchable.Search( query ), Is.EquivalentTo( expected ), context );
			Assert.That( searchable.Search( query.X1, query.Y1, query.Width, query.Height ), Is.EquivalentTo( expected ), context );
		}
	}

	[Test]
	public void Create_IndexBoundsExcludeCells_ThrowsRatherThanDroppingThem() {
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 101, 101 ),
			[new Point( 25, 25 ), new Point( 75, 25 ), new Point( 25, 75 ), new Point( 75, 75 )] );
		ISearchableVoronoiFactory searchableFactory = new QuadTreeSearchableVoronoiFactory( new SimpleQuadTreeFactory() );
		Assert.That( () => searchableFactory.Create( diagram, new Point( 100, 100 ) ), Throws.ArgumentException );
	}
}
