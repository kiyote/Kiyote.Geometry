using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class D3DelaunayFactoryBenchmarks {

	private readonly IDelaunayFactory _delaunayFactory;
	private readonly IReadOnlyList<Point> _100points;
	private readonly IReadOnlyList<Point> _500points;
	private readonly IReadOnlyList<Point> _1000points;

	public D3DelaunayFactoryBenchmarks() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_1000points = pointFactory.Fill( new Bounds( 1000, 1000 ), 5 );
		_500points = pointFactory.Fill( new Bounds( 500, 500 ), 5 );
		_100points = pointFactory.Fill( new Bounds( 100, 100 ), 5 );

		_delaunayFactory = new D3DelaunayFactory();
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void Create_100x100() {
		_ = _delaunayFactory.Create( _100points );
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void Create_500x500() {
		_ = _delaunayFactory.Create( _500points );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void Create_1000x1000() {
		_ = _delaunayFactory.Create( _1000points );
	}
}
