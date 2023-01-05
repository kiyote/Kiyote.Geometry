using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( true )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class VoronoiFactoryBenchmarks {

	private readonly IVoronoiFactory _voronoiFactory;
	private readonly Delaunator _100points;
	private readonly Delaunator _500points;
	private readonly Delaunator _1000points;

	public VoronoiFactoryBenchmarks() {
		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_1000points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 1000, 1000 ), 5 ) );
		_500points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 500, 500 ), 5 ) );
		_100points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 100, 100 ), 5 ) );

		_voronoiFactory = new VoronoiFactory();
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void Create_100x100() {
		_voronoiFactory.Create( _100points, 100, 100 );
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void Create_500x500() {
		_voronoiFactory.Create( _500points, 500, 500 );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void Create_1000x1000() {
		_voronoiFactory.Create( _1000points, 1000, 1000 );
	}
}

