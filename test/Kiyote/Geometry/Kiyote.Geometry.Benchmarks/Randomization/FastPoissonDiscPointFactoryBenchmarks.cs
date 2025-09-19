namespace Kiyote.Geometry.Randomization.Benchmarks;

[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
[MarkdownExporterAttribute.GitHub]
public class FastPoissonDiscPointFactoryBenchmarks {

	private readonly IRandom _random;
	private readonly IPointFactory _poissonDisc;

	public FastPoissonDiscPointFactoryBenchmarks() {
		_random = new FastRandom();
		_poissonDisc = new FastPoissonDiscPointFactory(
			_random
		);
	}

	[BenchmarkCategory( "Fill" ), Benchmark]
	public void Fill_100x100() {
		_random.Reinitialise( 0xF00D );
		 _poissonDisc.Fill( new Point( 100, 100 ), 5 );
	}

	[BenchmarkCategory( "Fill" ), Benchmark]
	public void Fill_500x500() {
		_random.Reinitialise( 0xF00D );
		_poissonDisc.Fill( new Point( 500, 500 ), 5 );
	}


	[BenchmarkCategory( "Fill" ), Benchmark]
	public void Fill_1000x1000() {
		_random.Reinitialise( 0xF00D );
		_poissonDisc.Fill( new Point( 1000, 1000 ), 5 );
	}
}
