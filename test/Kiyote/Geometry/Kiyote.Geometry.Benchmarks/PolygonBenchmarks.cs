using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
public class PolygonBenchmarks {

	private readonly Point _p1;
	private readonly Point _p2;
	private readonly Point _p3;
	private readonly Point _p4;
	private readonly Point[] _polyPoints;

	private readonly IReadOnlyList<Point> _points;
	private readonly Polygon _polygon;
	private readonly Polygon _other;
	private readonly Polygon _farOther;
	private readonly Polygon _poly2;
	private readonly Edge _edge;
	private readonly Edge _farEdge;
	private readonly Polygon _i1;
	private readonly Polygon _i2;

	public PolygonBenchmarks() {
		ISize size = new Point( 1000, 1000 );
		IPointFactory pointFactory = new FastPoissonDiscPointFactory(
			new FastRandom()
		);
		_points = pointFactory.Fill( size, 5 );

		_p1 = new Point( 200, 200 );
		_p2 = new Point( size.Width - 200, 200 );
		_p3 = new Point( size.Width - 200, size.Height - 200 );
		_p4 = new Point( 200, size.Height - 200 );
		_polyPoints = [_p1, _p2, _p3, _p4];

		_polygon = new Polygon( _polyPoints );

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
		_farEdge = new Edge( -200, -200, -200, -400 );
		_farOther = new Polygon( [
			new Point( -200, -200 ),
			new Point( -10, -200 ),
			new Point (-10, -10 ),
			new Point( -200, -10 )
		] );

		_i1 = new Polygon(
			[
				new Point( 200, 200 ),
				new Point( 800, 200 ),
				new Point( 800, 800 ),
				new Point( 200, 800 )
			] );

		_i2 = new Polygon(
			[
				new Point( 250, 250 ),
				new Point( 750, 350 ),
				new Point( 650, 850 ),
				new Point( 150, 700 )
			] );

	}

	[Benchmark]
	public void Ctor() {
		_ = new Polygon( _polyPoints );
	}

	[Benchmark]
	public void Contains_Point() {
		_polygon.Contains( _points[0] );
	}

	[Benchmark]
	public void Contains_Polygon() {
		_polygon.Contains( _other );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public void HasIntersection_Edge() {
		_ = _polygon.HasIntersection( _edge );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public void HasIntersection_Polygon() {
		_ = _polygon.HasIntersection( _other );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public void HasIntersection_EdgeArray() {
		_ = _polygon.HasIntersection( _other.Edges );
	}

	[Benchmark]
	public void HasOverlap() {
		_ = _polygon.HasOverlap( _other );
	}

	[Benchmark]
	public void IsEquivalentTo() {
		_polygon.IsEquivalentTo( _poly2 );
	}

	[Benchmark]
	public void TryIntersect_WithIntersection() {
		_ = _i1.TryIntersect( _i2, out Polygon _ );
	}

	[Benchmark]
	public void TryIntersect_WithoutIntersection() {
		_ = _i1.TryIntersect( _farOther, out Polygon _ );
	}

	[Benchmark]
	public void TryFindIntersections_Edge_WithIntersections() {
		_polygon.TryFindIntersections( _edge, out _ );
	}

	[Benchmark]
	public void TryFindIntersections_Edge_WithoutIntersections() {
		_polygon.TryFindIntersections( _farEdge, out _ );
	}

	[Benchmark]
	public void TryFindIntersections_Polygon_WithIntersections() {
		_polygon.TryFindIntersections( _other, out _ );
	}

	[Benchmark]
	public void TryFindIntersections_Polygon_WithoutIntersections() {
		_polygon.TryFindIntersections( _farOther, out _ );
	}
}
