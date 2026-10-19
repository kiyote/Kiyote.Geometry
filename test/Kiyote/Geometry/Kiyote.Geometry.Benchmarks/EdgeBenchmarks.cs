namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class EdgeBenchmarks {

	private readonly int _p1X;
	private readonly int _p1Y;
	private readonly int _p2X;
	private readonly int _p2Y;
	private readonly Point _p1;
	private readonly Point _p2;
	private readonly Edge _edge;
	private readonly Edge _other;

	public EdgeBenchmarks() {
		_p1X = 200;
		_p1Y = 200;
		_p2X = 400;
		_p2Y = 400;
		_p1 = new Point( _p1X, _p1Y );
		_p2 = new Point( _p2X, _p2Y );
		_edge = new Edge(
			_p1,
			_p2
		);

		_other = new Edge(
			new Point( 400, 200 ),
			new Point( 200, 400 )
		);
	}

	[Benchmark, BenchmarkCategory("Edge_Ctor")]
	public Edge Ctor_PointPoint() {
		return new Edge( _p1, _p2 );
	}

	[Benchmark, BenchmarkCategory( "Edge_Ctor" )]
	public Edge Ctor_IntIntIntInt() {
		return new Edge( _p1X, _p1Y, _p2X, _p2Y );
	}

	[Benchmark]
	public bool HasIntersection() {
		return _edge.HasIntersection( _other );
	}

	[Benchmark]
	public (bool Found, Point Intersection) TryFindIntersection() {
		bool found = _edge.TryFindIntersection( _other, out Point intersection );
		return (found, intersection);
	}

	[Benchmark]
	public Rect GetBoundingBox() {
		return _edge.GetBoundingBox();
	}

	[Benchmark]
	public Point GetMidpoint() {
		return _edge.GetMidpoint( 0.5f );
	}

	[GlobalSetup]
	public void Validate() {
		if( !HasIntersection()
			|| TryFindIntersection() != (true, new Point( 300, 300 ))
			|| GetMidpoint() != new Point( 300, 300 )
			|| GetBoundingBox() != new Rect( 200, 200, 201, 201 )
			|| Ctor_PointPoint() != _edge
			|| Ctor_IntIntIntInt() != _edge
		) {
			throw new InvalidOperationException( "Edge benchmark fixture produced an unexpected result." );
		}
	}
}
