namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
public class EdgeBenchmarks {

	private readonly Edge _edge;

	private readonly Edge _other;

	public EdgeBenchmarks() {
		_edge = new Edge(
			new Point( 200, 200 ),
			new Point( 400, 400 )
		);

		_other = new Edge(
			new Point( 400, 200 ),
			new Point( 200, 400 )
		);
	}

	[Benchmark]
	public void HasIntersection() {
		_ = _edge.HasIntersection( _other );
	}

	[Benchmark]
	public void TryFindIntersection() {
		_ = _edge.TryFindIntersection( _other, out _ );
	}
}
