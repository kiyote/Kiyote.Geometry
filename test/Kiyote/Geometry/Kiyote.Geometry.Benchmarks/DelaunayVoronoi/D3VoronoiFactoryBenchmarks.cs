namespace Kiyote.Geometry.DelaunayVoronoi.Benchmarks;

[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
[MarkdownExporterAttribute.GitHub]
public class D3VoronoiFactoryBenchmarks {

	public const int DistanceApart = 5;

	private readonly IVoronoiFactory _voronoiFactory;
	private readonly Rect _100bounds;
	private readonly IReadOnlyList<Point> _100points;
	private readonly Rect _500bounds;
	private readonly IReadOnlyList<Point> _500points;
	private readonly Rect _1000bounds;
	private readonly IReadOnlyList<Point> _1000points;

	public D3VoronoiFactoryBenchmarks() {

		_1000points = Fill( 1000, 1000 );
		_1000bounds = new Rect( 0, 0, 1000, 1000 );
		_500points = Fill( 500, 500 );
		_500bounds = new Rect( 0, 0, 500, 500 );
		_100points = Fill( 100, 100 );
		_100bounds = new Rect( 0, 0, 100, 100 );

		_voronoiFactory = new D3VoronoiFactory();
	}

	private static IReadOnlyList<Point> Fill(
		int width, int height
	) {
		int horizontalPoints = width / DistanceApart;
		int verticalPoints = height / DistanceApart;
		int offset = DistanceApart / 2;
		// Produce a grid of points to reduce positioning of the points
		// affecting performance
		List<Point> points = new List<Point>( width / horizontalPoints * (height / verticalPoints) );
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

	[BenchmarkCategory( "Create" ), Benchmark]
	public void Create_100x100() {
		 _voronoiFactory.Create( _100bounds, _100points );
	}

	[BenchmarkCategory( "Create" ), Benchmark]
	public void Create_500x500() {
		 _voronoiFactory.Create( _500bounds, _500points );
	}

	[BenchmarkCategory( "Create" ), Benchmark]
	public void Create_1000x1000() {
		 _voronoiFactory.Create( _1000bounds, _1000points );
	}
}
