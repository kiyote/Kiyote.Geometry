namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class EdgeBenchmarks {

	private readonly IEdge _edge1;
	private readonly IEdge _edge2;

	public EdgeBenchmarks() {
		Bounds bounds = new Bounds( 1000, 1000 );
		_edge1 = new Edge( new Point( 0, 0 ), new Point( bounds.Width, bounds.Height ) );
		_edge2 = new Edge( new Point( 0, bounds.Height ), new Point( bounds.Width, 0 ) );
	}

	[Benchmark]
	public void Intersection() {
		_edge1.Intersection( _edge2 );
	}
}

