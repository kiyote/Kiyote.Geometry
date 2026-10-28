using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Trees;

namespace Kiyote.Geometry.DelaunayVoronoi.Benchmarks;

[MemoryDiagnoser( false )]
public class VoronoiConsumptionBenchmarks {
	private readonly IVoronoiFactory _factory = new D3VoronoiFactory();
	private readonly ISearchableVoronoiFactory _searchFactory = new QuadTreeSearchableVoronoiFactory( new SimpleQuadTreeFactory() );
	private Point[] _sites = null!;
	private IVoronoi _diagram = null!;
	private Point _size;
	private Rect _bounds;

	[Params( 20, 64 )]
	public int Side { get; set; }

	[GlobalSetup]
	public void Setup() {
		_size = new Point( Side * 50, Side * 50 );
		_bounds = new Rect( 0, 0, _size );
		_sites = new Point[Side * Side];
		Random random = new Random( 7291 );
		for( int i = 0; i < _sites.Length; i++ ) {
			_sites[i] = new Point( 25 + (i % Side * 50) + random.Next( -10, 11 ),
				25 + (i / Side * 50) + random.Next( -10, 11 ) );
		}
		_diagram = _factory.Create( _bounds, _sites );
		if( ContainsSites() != _sites.Length || CreateAndConsume() != IterateVerticesAndNeighbours()
			|| BuildSearchIndex().Search( _bounds ).Count != _sites.Length ) {
			throw new InvalidOperationException( "Voronoi consumption fixtures did not preserve complete output." );
		}
	}

	[Benchmark]
	public long CreateAndConsume() => Consume( _factory.Create( _bounds, _sites ) );

	[Benchmark]
	public long IterateVerticesAndNeighbours() => Consume( _diagram );

	[Benchmark]
	public int ContainsSites() {
		int contained = 0;
		for( int i = 0; i < _diagram.Cells.Length; i++ ) {
			if( _diagram.Contains( i, _sites[i] ) ) {
				contained++;
			}
		}
		return contained;
	}

	[Benchmark]
	public ISearchableVoronoi BuildSearchIndex() => _searchFactory.Create( _diagram, _size );

	private static long Consume( IVoronoi diagram ) {
		long checksum = 0;
		ReadOnlySpan<Cell> cells = diagram.Cells;
		for( int i = 0; i < cells.Length; i++ ) {
			ref readonly Cell cell = ref cells[i];
			Point center = diagram.Points[i];
			checksum += center.X + center.Y + cell.BoundingBox.Width + cell.BoundingBox.Height;
			ReadOnlySpan<Point> vertices = diagram.GetCellVertices( i );
			for( int j = 0; j < vertices.Length; j++ ) {
				Point point = vertices[j];
				checksum += point.X + point.Y;
			}
			foreach( int neighbour in diagram.GetNeighbourIndices( i ) ) {
				checksum += neighbour;
			}
		}
		return checksum;
	}
}
