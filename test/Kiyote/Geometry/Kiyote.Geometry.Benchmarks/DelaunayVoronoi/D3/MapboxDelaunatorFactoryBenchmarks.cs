using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi.D3;

[MemoryDiagnoser( false )]
public class MapboxDelaunatorFactoryBenchmarks {

	public const int DistanceApart = 5;
	private readonly double[] _100points;
	private readonly double[] _500points;
	private readonly double[] _1000points;

	[Params( false, true )]
	public bool RejectDuplicates { get; set; }

	public MapboxDelaunatorFactoryBenchmarks() {
		_1000points = Fill( new Point( 1000, 1000 ) );
		_500points = Fill( new Point( 500, 500 ) );
		_100points = Fill( new Point( 100, 100 ) );
	}

	[GlobalSetup]
	public void Setup() {
		foreach( double[] points in new[] { _100points, _500points, _1000points } ) {
			MapboxDelaunator uncheckedResult = MapboxDelaunatorFactory.Create( points );
			MapboxDelaunator checkedResult = MapboxDelaunatorFactory.Create( points, rejectDuplicates: true );
			if( !checkedResult.Triangles.SequenceEqual( uncheckedResult.Triangles )
				|| !checkedResult.HalfEdges.SequenceEqual( uncheckedResult.HalfEdges )
				|| !checkedResult.Hull.SequenceEqual( uncheckedResult.Hull ) ) {
				throw new InvalidOperationException( "Duplicate validation changed the triangulation fixture." );
			}
		}
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

	[BenchmarkCategory( "Create" ), Benchmark]
	public object Create_100x100() {
		return MapboxDelaunatorFactory.Create( _100points, RejectDuplicates );
	}

	[BenchmarkCategory( "Create" ), Benchmark]
	public object Create_500x500() {
		return MapboxDelaunatorFactory.Create( _500points, RejectDuplicates );
	}

	[BenchmarkCategory( "Create" ), Benchmark]
	public object Create_1000x1000() {
		return MapboxDelaunatorFactory.Create( _1000points, RejectDuplicates );
	}
}
