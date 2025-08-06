using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.Randomization;

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

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void Fill_100x100() {
		 _poissonDisc.Fill( new Point( 100, 100 ), 5 );
	}

	[BenchmarkCategory( "100x100" ), Benchmark( Baseline = true )]
	public void Fill_100x100_Base() {
		ISize bounds = new Point( 100, 100 );
		List<Point> points = [];
		for( int i = 0; i < 270; i++ ) {
			points.Add( new Point( _random.NextInt( bounds.Width ), _random.NextInt( bounds.Height ) ) );
		}
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void Fill_500x500() {
		 _poissonDisc.Fill( new Point( 500, 500 ), 5 );
	}

	[BenchmarkCategory( "500x500" ), Benchmark( Baseline = true )]
	public void Fill_500x500_Base() {
		ISize bounds = new Point( 500, 500 );
		List<Point> points = [];
		for( int i = 0; i < 6350; i++ ) {
			points.Add( new Point( _random.NextInt( bounds.Width ), _random.NextInt( bounds.Height ) ) );
		}
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void Fill_1000x1000() {
		 _poissonDisc.Fill( new Point( 1000, 1000 ), 5 );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark( Baseline = true )]
	public void Fill_1000x1000_Base() {
		ISize bounds = new Point( 1000, 1000 );
		List<Point> points = [];
		for( int i = 0; i < 25300; i++ ) {
			points.Add( new Point( _random.NextInt( bounds.Width ), _random.NextInt( bounds.Height ) ) );
		}
	}
}
