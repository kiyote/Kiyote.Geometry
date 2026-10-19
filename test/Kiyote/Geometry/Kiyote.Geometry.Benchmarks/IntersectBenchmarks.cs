namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class IntersectBenchmarks {

	private readonly Edge _first = new Edge( 200, 200, 400, 400 );
	private readonly Edge _second = new Edge( 200, 400, 400, 200 );

	[Benchmark]
	public bool HasIntersection() {
		return Intersect.HasIntersection(
			_first.A.X, _first.A.Y,
			_first.B.X, _first.B.Y,
			_second.A.X, _second.A.Y,
			_second.B.X, _second.B.Y
		);
	}

	[Benchmark]
	public (bool Found, int X, int Y) TryFindIntersection() {
		bool found = Intersect.TryFindIntersection(
			_first.A.X, _first.A.Y,
			_first.B.X, _first.B.Y,
			_second.A.X, _second.A.Y,
			_second.B.X, _second.B.Y,
			out int ix,
			out int iy
		);
		return (found, ix, iy);
	}

	[GlobalSetup]
	public void Validate() {
		if( !HasIntersection() || TryFindIntersection() != (true, 300, 300) ) {
			throw new InvalidOperationException( "Intersection benchmark fixture produced an unexpected result." );
		}
	}
}
