using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( true )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class DelaunayFactoryBenchmarks {

	private readonly IDelaunayFactory _delaunayFactory;
	private readonly Delaunator _100points;
	private readonly Delaunator _500points;
	private readonly Delaunator _1000points;

	public DelaunayFactoryBenchmarks() {
		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_1000points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 1000, 1000 ), 5 ) );
		_500points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 500, 500 ), 5 ) );
		_100points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 100, 100 ), 5 ) );

		_delaunayFactory = new DelaunayFactory();
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void Create_100x100() {
		_delaunayFactory.Create( _100points );
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void Create_500x500() {
		_delaunayFactory.Create( _500points );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void Create_1000x1000() {
		_delaunayFactory.Create( _1000points );
	}
}

