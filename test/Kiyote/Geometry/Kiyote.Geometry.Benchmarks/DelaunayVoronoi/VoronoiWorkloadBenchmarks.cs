namespace Kiyote.Geometry.DelaunayVoronoi.Benchmarks;

public enum VoronoiWorkload {
	Grid,
	JitteredGrid,
	HighDegree
}

[MemoryDiagnoser( false )]
public class VoronoiWorkloadBenchmarks {

	private readonly IVoronoiFactory _factory = new D3VoronoiFactory();
	private Point[] _points = null!;
	private Rect _bounds;

	[ParamsAllValues]
	public VoronoiWorkload Workload { get; set; }

	[GlobalSetup]
	public void Setup() {
		if( Workload == VoronoiWorkload.HighDegree ) {
			_bounds = new Rect( -20000, -20000, 40001, 40001 );
			_points = new Point[41];
			for( int i = 0; i < 40; i++ ) {
				double angle = 2 * Math.PI * i / 40;
				_points[i + 1] = new Point(
					(int)Math.Round( 10000 * Math.Cos( angle ) ),
					(int)Math.Round( 10000 * Math.Sin( angle ) )
				);
			}
		} else {
			_bounds = new Rect( -500, -500, 1001, 1001 );
			_points = new Point[400];
			Random random = new Random( 7291 );
			for( int row = 0; row < 20; row++ ) {
				for( int column = 0; column < 20; column++ ) {
					int x = -475 + ( column * 50 );
					int y = -475 + ( row * 50 );
					if( Workload == VoronoiWorkload.JitteredGrid ) {
						x += random.Next( -10, 11 );
						y += random.Next( -10, 11 );
					}
					_points[( row * 20 ) + column] = new Point( x, y );
				}
			}
		}

		IVoronoi result = Create();
		if( result.Cells.Count != _points.Length ) {
			throw new InvalidOperationException( "Voronoi benchmark did not produce one cell per site." );
		}
		for( int i = 0; i < _points.Length; i++ ) {
			Cell cell = result.Cells[i];
			if( cell.Center != _points[i] || !cell.Polygon.Contains( _points[i] ) ) {
				throw new InvalidOperationException( $"Voronoi fixture {Workload}, cell {i} does not contain its site." );
			}
		}
	}

	[Benchmark]
	public IVoronoi Create() {
		return _factory.Create( _bounds, _points );
	}
}
