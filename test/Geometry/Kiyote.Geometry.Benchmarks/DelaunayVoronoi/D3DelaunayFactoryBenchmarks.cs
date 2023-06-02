using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class D3DelaunayFactoryBenchmarks {

	public const int DistanceApart = 5;
	private readonly IDelaunayFactory _delaunayFactory;
	private readonly IReadOnlyList<Point> _100points;
	private readonly IReadOnlyList<Point> _500points;
	private readonly IReadOnlyList<Point> _1000points;

	public D3DelaunayFactoryBenchmarks() {
		_1000points = Fill( new Bounds( 1000, 1000 ) );
		_500points = Fill( new Bounds( 500, 500 ) );
		_100points = Fill( new Bounds( 100, 100 ) );

		_delaunayFactory = new D3DelaunayFactory();
	}

	private static IReadOnlyList<Point> Fill(
		Bounds bounds
	) {
		int horizontalPoints = bounds.Width / DistanceApart;
		int verticalPoints = bounds.Height / DistanceApart;
		int offset = DistanceApart / 2;
		List<Point> points = new List<Point>( bounds.Width / horizontalPoints * ( bounds.Height / verticalPoints ) );
		for( int i = 0; i < horizontalPoints; i++ ) {
			for( int j = 0; j < verticalPoints; j++ ) {
				points.Add(
					new Point(
						( DistanceApart * i ) + offset,
						( DistanceApart * j ) + offset
					)
				);
			}
		}
		return points;
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
