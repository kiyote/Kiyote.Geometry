using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
public class PolygonBenchmarks {

	private readonly IReadOnlyList<Point> _points;
	private readonly Polygon _polygon;
	private readonly Polygon _other;
	private readonly Polygon _poly2;
	private readonly Edge _edge;

	public PolygonBenchmarks() {
		ISize size = new Point( 1000, 1000 );
		IPointFactory pointFactory = new FastPoissonDiscPointFactory(
			new FastRandom()
		);
		_points = pointFactory.Fill( size, 5 );


		_polygon = new Polygon(
			[
				new Point( 200, 200 ),
				new Point( size.Width - 200, 200 ),
				new Point( size.Width - 200, size.Height - 200 ),
				new Point( 200, size.Height - 200 )
			]
		);

		_poly2 = new Polygon(
			[
				new Point( size.Width - 200, 200 ),
				new Point( size.Width - 200, size.Height - 200 ),
				new Point( 200, size.Height - 200 ),
				new Point( 200, 200 )
			]
		);

		_other = new Polygon(
			[
				new Point( 250, 250 ),
				new Point( size.Width - 250, 350 ),
				new Point( size.Width - 350, size.Height - 150 ),
				new Point( 150, size.Height - 300 )
			]
		);

		_edge = new Edge(
			new Point( 200, 200 ),
			new Point( size.Width - 200, size.Height - 200 )
		);
	}

	[Benchmark]
	public void TryIntersect() {
		 _polygon.TryIntersect( _other, out Polygon _ );
	}

	[Benchmark]
	public void Contains() {
		 _polygon.Contains( _points[0] );
	}

	[Benchmark]
	public void Intersections() {
		 _polygon.Intersections( _other.Points );
	}

	[Benchmark]
	public void IsEquivalentTo() {
		_polygon.IsEquivalentTo( _poly2 );
	}


	[Benchmark, BenchmarkCategory("HasIntersection")]
	public void HasIntersection_Edge() {
		_ = _polygon.HasIntersection( _edge );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public void HasIntersection_Polygon() {
		_ = _polygon.HasIntersection( _other );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public void HasIntersection_EdgeList() {
		_ = _polygon.HasIntersection( _other.Edges );
	}

	[Benchmark]
	public void HasOverlap() {
		_ = _polygon.HasOverlap( _other );
	}

	[Benchmark]
	public void TryFindIntersections() {
		_polygon.TryFindIntersections( _edge, out _ );
	}
}
