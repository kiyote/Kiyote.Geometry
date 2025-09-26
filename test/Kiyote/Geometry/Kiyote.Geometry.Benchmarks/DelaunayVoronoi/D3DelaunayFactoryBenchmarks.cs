namespace Kiyote.Geometry.DelaunayVoronoi.Benchmarks;

[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
[MarkdownExporterAttribute.GitHub]
public class D3DelaunayFactoryBenchmarks {

	public const int DistanceApart = 5;
	private readonly IDelaunayFactory _delaunayFactory;
	private readonly IReadOnlyList<Point> _100points;
	private readonly IReadOnlyList<Point> _500points;
	private readonly IReadOnlyList<Point> _1000points;

	public D3DelaunayFactoryBenchmarks() {
		_1000points = Fill( new Point( 1000, 1000 ) );
		_500points = Fill( new Point( 500, 500 ) );
		_100points = Fill( new Point( 100, 100 ) );

		_delaunayFactory = new D3DelaunayFactory();
	}

	private static IReadOnlyList<Point> Fill(
		ISize bounds
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

	[BenchmarkCategory( "Create" ), Benchmark]
	public void Create_100x100() {
		 _delaunayFactory.Create( _100points );
	}

	[BenchmarkCategory( "Create" ), Benchmark]
	public void Create_500x500() {
		 _delaunayFactory.Create( _500points );
	}

	[BenchmarkCategory( "Create" ), Benchmark]
	public void Create_1000x1000() {
		 _delaunayFactory.Create( _1000points );
	}
}
