using Kiyote.Geometry.Noises;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.Noises;

[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
public class MidpointDisplacementNoisyEdgeFactoryBenchmarks {
	private readonly INoisyEdgeFactory _factory;
	private readonly Edge _toSplit;
	private readonly Edge _control;

	public MidpointDisplacementNoisyEdgeFactoryBenchmarks() {
		IRandom random = new FastRandom( 0xF00D );
		_factory = new MidpointDisplacementNoisyEdgeFactory( random );
		_toSplit = new Edge( new Point( 0, 0 ), new Point( 100, 100 ) );
		_control = new Edge( new Point( 0, 50 ), new Point( 100, 50 ) );
	}

	[Benchmark, BenchmarkCategory( "Create" )]
	public void Create_Amplitude05_Levels3() {
		_ = _factory.Create( _toSplit, _control, 0.5f, 3 );
	}

	[Benchmark, BenchmarkCategory( "Create" )]
	public void Create_Amplitude05_Levels4() {
		_ = _factory.Create( _toSplit, _control, 0.5f, 4 );
	}
}
