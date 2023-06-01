using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class D3VoronoiFactoryBenchmarks {

	private readonly IVoronoiFactory _voronoiFactory;
	private readonly IRect _100bounds;
	private readonly IReadOnlyList<Point> _100points;
	private readonly IRect _500bounds;
	private readonly IReadOnlyList<Point> _500points;
	private readonly IRect _1000bounds;
	private readonly IReadOnlyList<Point> _1000points;

	public D3VoronoiFactoryBenchmarks() {
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );

		_1000bounds = new Rect( 0, 0, 1000, 1000 );
		_1000points = pointFactory.Fill( new Bounds( 1000, 1000 ), 5 );

		_500bounds = new Rect( 0, 0, 500, 500 );
		_500points = pointFactory.Fill( new Bounds( 500, 500 ), 5 );

		_100bounds = new Rect( 0, 0, 100, 100 );
		_100points = pointFactory.Fill( new Bounds( 100, 100 ), 5 );

		_voronoiFactory = new D3VoronoiFactory();
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void Create_100x100() {
		_ = _voronoiFactory.Create( _100bounds, _100points );
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void Create_500x500() {
		_ = _voronoiFactory.Create( _500bounds, _500points );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void Create_1000x1000() {
		_ = _voronoiFactory.Create( _1000bounds, _1000points );
	}
}
