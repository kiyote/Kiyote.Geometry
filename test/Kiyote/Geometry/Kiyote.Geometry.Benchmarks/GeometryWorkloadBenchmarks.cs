namespace Kiyote.Geometry.Benchmarks;

public enum GeometryWorkload {
	Small,
	Screen,
	Boundary,
	Irregular
}

[MemoryDiagnoser( false )]
public class GeometryWorkloadBenchmarks {

	private const int OperationCount = 256;
	private readonly Edge[] _edges = new Edge[OperationCount];
	private readonly Edge[] _crossings = new Edge[OperationCount];
	private readonly Point[] _expected = new Point[OperationCount];

	[ParamsAllValues]
	public GeometryWorkload Workload { get; set; }

	[GlobalSetup]
	public void Setup() {
		Random random = new Random( 7291 );
		for( int i = 0; i < OperationCount; i++ ) {
			int x = Workload == GeometryWorkload.Irregular ? random.Next( -900, 900 ) : i % 16;
			int y = Workload == GeometryWorkload.Irregular ? random.Next( -500, 500 ) : i / 16;
			int left;
			int right;
			switch( Workload ) {
				case GeometryWorkload.Small:
					left = x - 2;
					right = x + 2;
					break;
				case GeometryWorkload.Screen:
					left = 0;
					right = 1919;
					break;
				case GeometryWorkload.Boundary:
					left = -32768;
					right = 32767;
					break;
				case GeometryWorkload.Irregular:
					left = x - random.Next( 1, 100 );
					right = x + random.Next( 1, 100 );
					break;
				default:
					throw new InvalidOperationException( "Unknown geometry workload." );
			}
			_edges[i] = new Edge( left, y, right, y );
			_crossings[i] = new Edge( x, y - 1, x, y + 1 );
			_expected[i] = new Point( x, y );
			if( !_edges[i].HasIntersection( _crossings[i] )
				|| !_edges[i].TryFindIntersection( _crossings[i], out Point intersection )
				|| intersection != _expected[i]
			) {
				throw new InvalidOperationException( $"Geometry fixture {Workload}, case {i} has an incorrect intersection." );
			}
		}
	}

	[Benchmark( OperationsPerInvoke = OperationCount )]
	public int HasIntersection() {
		int found = 0;
		for( int i = 0; i < OperationCount; i++ ) {
			if( _edges[i].HasIntersection( _crossings[i] ) ) {
				found++;
			}
		}
		return found;
	}

	[Benchmark( OperationsPerInvoke = OperationCount )]
	public (int Found, int X, int Y) TryFindIntersection() {
		int found = 0;
		int x = 0;
		int y = 0;
		for( int i = 0; i < OperationCount; i++ ) {
			if( _edges[i].TryFindIntersection( _crossings[i], out Point intersection ) ) {
				found++;
				x += intersection.X;
				y += intersection.Y;
			}
		}
		return (found, x, y);
	}
}
