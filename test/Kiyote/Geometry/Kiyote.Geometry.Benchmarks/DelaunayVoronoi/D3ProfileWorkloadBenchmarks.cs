using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Testing;

namespace Kiyote.Geometry.DelaunayVoronoi.Benchmarks;

[MemoryDiagnoser( false )]
public class D3ProfileWorkloadBenchmarks {
	private readonly IVoronoiFactory _factory = new D3VoronoiFactory();
	private D3WorkloadFixture _fixture = null!;

	[Params( "Irregular", "Thin", "MostlyHull", "Sanitized" )]
	public string Workload { get; set; } = null!;

	[GlobalSetup]
	public void Setup() {
		_fixture = D3WorkloadFixture.Create( Workload );
		_fixture.Validate( Create() );
	}

	[Benchmark]
	public IVoronoi Create() {
		return _factory.Create( _fixture.Bounds, _fixture.Input, _fixture.Sanitize );
	}
}
