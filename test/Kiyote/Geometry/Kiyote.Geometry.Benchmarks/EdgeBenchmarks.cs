namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
public class EdgeBenchmarks {

	private readonly int _p1X;
	private readonly int _p1Y;
	private readonly int _p2X;
	private readonly int _p2Y;
	private readonly Point _p1;
	private readonly Point _p2;
	private readonly Edge _edge;
	private readonly Edge _other;
	private Point _i;

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
	public void Ctor_PointPoint() {
		_ = new Edge( _p1, _p2 );
	}

	[Benchmark, BenchmarkCategory( "Edge_Ctor" )]
	public void Ctor_IntIntIntInt() {
		_ = new Edge( _p1X, _p1Y, _p2X, _p2Y );
	}

	[Benchmark]
	public void HasIntersection() {
		_ = _edge.HasIntersection( _other );
	}

	[Benchmark]
	public void TryFindIntersection() {
		_ = _edge.TryFindIntersection( _other, out _i );
	}

	[Benchmark]
	public void GetBoundingBox() {
		_ = _edge.GetBoundingBox();
	}

	[Benchmark]
	public void GetMidpoint() {
		_ = _edge.GetMidpoint( 0.5f );
	}
}
