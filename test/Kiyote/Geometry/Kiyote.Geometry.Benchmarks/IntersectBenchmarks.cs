namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
public class IntersectBenchmarks {

	[Benchmark]
	public void HasIntersection() {
		_ = Intersect.HasIntersection(
			200, 200,
			400, 400,
			200, 400,
			400, 200
		);
	}

	[Benchmark]
	public void TryFindIntersection() {
		_ = Intersect.TryFindIntersection(
			200, 200,
			400, 400,
			200, 400,
			400, 200,
			out int ix,
			out int iy
		);
	}
}
