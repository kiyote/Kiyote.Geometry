using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( true )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class DelaunatorFactoryBenchmarks {

	private readonly IDelaunatorFactory _delaunatorFactory;
	private readonly IReadOnlyList<IPoint> _100points;
	private readonly IReadOnlyList<IPoint> _500points;
	private readonly IReadOnlyList<IPoint> _1000points;

	public DelaunatorFactoryBenchmarks() {
		_delaunatorFactory = new DelaunatorFactory();
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_1000points = pointFactory.Fill( new Bounds( 1000, 1000 ), 5 );
		_500points = pointFactory.Fill( new Bounds( 500, 500 ), 5 );
		_100points = pointFactory.Fill( new Bounds( 100, 100 ), 5 );
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void Create_100x100() {
		_delaunatorFactory.Create( _100points );
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void Create_500x500() {
		_delaunatorFactory.Create( _500points );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void Create_1000x1000() {
		_delaunatorFactory.Create( _1000points );
	}
}

