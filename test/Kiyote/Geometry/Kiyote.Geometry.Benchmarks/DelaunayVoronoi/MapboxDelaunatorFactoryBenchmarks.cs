using Kiyote.Geometry.DelaunayVoronoi;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( true )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
[MarkdownExporterAttribute.GitHub]
public class MapboxDelaunatorFactoryBenchmarks {

	public const int DistanceApart = 5;
	private readonly double[] _100points;
	private readonly double[] _500points;
	private readonly double[] _1000points;

	public MapboxDelaunatorFactoryBenchmarks() {
		_1000points = Fill( new Point( 1000, 1000 ) );
		_500points = Fill( new Point( 500, 500 ) );
		_100points = Fill( new Point( 100, 100 ) );
	}

	private static double[] Fill(
		ISize bounds
	) {
		int horizontalPoints = bounds.Width / DistanceApart;
		int verticalPoints = bounds.Height / DistanceApart;
		int offset = DistanceApart / 2;
		int pointCount = horizontalPoints * verticalPoints;
		double[] points = new double[pointCount * 2];
		int pointIndex = 0;
		for( int i = 0; i < horizontalPoints; i++ ) {
			for( int j = 0; j < verticalPoints; j++ ) {
				points[pointIndex + 0] = ( DistanceApart * i ) + offset;
				points[pointIndex + 1] = ( DistanceApart * j ) + offset;
				pointIndex += 2;
			}
		}
		return points;
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void Create_100x100() {
		MapboxDelaunatorFactory.Create( _100points );
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void Create_500x500() {
		MapboxDelaunatorFactory.Create( _500points );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void Create_1000x1000() {
		MapboxDelaunatorFactory.Create( _1000points );
	}
}
