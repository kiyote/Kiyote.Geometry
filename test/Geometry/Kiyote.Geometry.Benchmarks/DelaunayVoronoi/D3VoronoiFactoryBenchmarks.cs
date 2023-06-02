using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class D3VoronoiFactoryBenchmarks {

	public const int DistanceApart = 5;

	private readonly IVoronoiFactory _voronoiFactory;
	private readonly IRect _100bounds;
	private readonly IReadOnlyList<Point> _100points;
	private readonly IRect _500bounds;
	private readonly IReadOnlyList<Point> _500points;
	private readonly IRect _1000bounds;
	private readonly IReadOnlyList<Point> _1000points;

	public D3VoronoiFactoryBenchmarks() {

		_1000bounds = new Rect( 0, 0, 1000, 1000 );
		_1000points = Fill( new Bounds( 1000, 1000 ) );

		_500bounds = new Rect( 0, 0, 500, 500 );
		_500points = Fill( new Bounds( 500, 500 ) );

		_100bounds = new Rect( 0, 0, 100, 100 );
		_100points = Fill( new Bounds( 100, 100 ) );

		_voronoiFactory = new D3VoronoiFactory();
	}

	private static IReadOnlyList<Point> Fill(
		Bounds bounds
	) {
		int horizontalPoints = bounds.Width / DistanceApart;
		int verticalPoints = bounds.Height / DistanceApart;
		int offset = DistanceApart / 2;
		List<Point> points = new List<Point>( bounds.Width / horizontalPoints * (bounds.Height / verticalPoints) );
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
