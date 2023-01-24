using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.DelaunayVoronoi;

[MemoryDiagnoser( true )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class VoronoiBenchmarks {

	private readonly IVoronoi _1000points;
	private readonly IVoronoi _500points;
	private readonly IVoronoi _100points;

	public VoronoiBenchmarks() {
		IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		Delaunator d1000points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 1000, 1000 ), 5 ) );
		Delaunator d500points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 500, 500 ), 5 ) );
		Delaunator d100points = delaunatorFactory.Create( pointFactory.Fill( new Bounds( 100, 100 ), 5 ) );

		IVoronoiFactory voronoiFactory = new VoronoiFactory();

		_1000points = voronoiFactory.Create( d1000points, 1000, 1000 );
		_500points = voronoiFactory.Create( d500points, 500, 500 );
		_100points = voronoiFactory.Create( d100points, 100, 100 );
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void IterateCells_1000x1000() {
		foreach (Cell cell in _1000points.Cells) {
		}
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void IterateCells_500x500() {
		foreach( Cell cell in _500points.Cells ) {
		}
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void IterateCells_100x100() {
		foreach( Cell cell in _100points.Cells ) {
		}
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void IterateEdges_1000x1000() {
		foreach( Edge edge in _1000points.Edges ) {
		}
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void IterateEdges_500x500() {
		foreach( Edge edge in _500points.Edges ) {
		}
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void IterateEdges_100x100() {
		foreach( Edge edge in _100points.Edges ) {
		}
	}

	[BenchmarkCategory( "1000x1000" ), Benchmark]
	public void IterateNeighbours_1000x1000() {
		foreach( KeyValuePair<Cell, IReadOnlyList<Cell>> entry in _1000points.Neighbours ) {
		}
	}

	[BenchmarkCategory( "500x500" ), Benchmark]
	public void IterateNeighbours_500x500() {
		foreach( KeyValuePair<Cell, IReadOnlyList<Cell>> entry in _1000points.Neighbours ) {
		}
	}

	[BenchmarkCategory( "100x100" ), Benchmark]
	public void IterateNeighbours_100x100() {
		foreach( KeyValuePair<Cell, IReadOnlyList<Cell>> entry in _1000points.Neighbours ) {
		}
	}
}

