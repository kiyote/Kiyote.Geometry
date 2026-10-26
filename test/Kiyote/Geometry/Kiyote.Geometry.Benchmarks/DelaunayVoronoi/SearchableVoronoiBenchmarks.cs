using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi.Benchmarks;

[MemoryDiagnoser( false )]
public class SearchableVoronoiBenchmarks {
	private ISearchableVoronoi _searchable = null!;
	private Rect _query;

	[Params( 20, 200 )]
	public int QuerySize { get; set; }

	[GlobalSetup]
	public void Setup() {
		Point[] sites = new Point[400];
		for( int i = 0; i < sites.Length; i++ ) {
			sites[i] = new Point( ( i % 20  * 10) + 5, ( i / 20  * 10) + 5 );
		}
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 200, 200 ), sites );
		ISearchableVoronoiFactory searchableFactory = new QuadTreeSearchableVoronoiFactory( new SimpleQuadTreeFactory() );
		_searchable = searchableFactory.Create( diagram, new Point( 200, 200 ) );
		_query = new Rect( 0, 0, QuerySize, QuerySize );
		Cell[] expected = diagram.Cells.Where( cell => cell.BoundingBox.Overlaps( _query ) ).ToArray();
		if( !_searchable.Search( _query ).ToHashSet().SetEquals( expected )
			|| SearchVisitor() != expected.Length ) {
			throw new InvalidOperationException( "Search differs from the bounding-box reference." );
		}
	}

	[Benchmark]
	public IReadOnlyList<Cell> Search() => _searchable.Search( _query );

	[Benchmark]
	public int SearchVisitor() {
		CountingVisitor visitor = default;
		_searchable.Search( _query, ref visitor );
		return visitor.Count;
	}

	private struct CountingVisitor : IQueryVisitor<Cell> {
		public int Count;
		public void Visit( scoped in Cell item ) => Count++;
	}
}
