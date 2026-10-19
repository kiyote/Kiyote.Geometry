
namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class PolygonBenchmarks {

	private readonly Point _p1;
	private readonly Point _p2;
	private readonly Point _p3;
	private readonly Point _p4;
	private readonly Point[] _polyPoints;

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
	public Polygon Ctor() {
		return new Polygon( _polyPoints );
	}

	[GlobalSetup]
	public void Validate() {
		(bool clipped, Polygon result) = TryIntersect_WithIntersection();
		Point[] expected = [
			new( 660, 800 ), new( 483, 800 ), new( 200, 715 ),
			new( 200, 475 ), new( 250, 250 ), new( 750, 350 )
		];
		if( Ctor().Points.Count != 4
			|| Contains_Polygon()
			|| !HasIntersection_Edge()
			|| !HasIntersection_Polygon()
			|| !HasIntersection_EdgeArray()
			|| !HasOverlap()
			|| !IsEquivalentTo()
			|| !clipped
			|| result.Points.Count != expected.Length
			|| !expected.All( result.Points.Contains )
			|| TryIntersect_WithoutIntersection() != (false, Polygon.None)
			|| TryFindBoundaryIntersections_Edge_WithIntersections().Intersections.Count != 2
			|| TryFindBoundaryIntersections_Polygon_WithIntersections().Intersections.Count != 4
			|| TryFindBoundaryIntersections_Edge_WithoutIntersections().Found
			|| TryFindBoundaryIntersections_Polygon_WithoutIntersections().Found
		) {
			throw new InvalidOperationException( "Polygon benchmark fixture produced an unexpected result." );
		}
	}

	[Benchmark]
	public bool Contains_Polygon() {
		return _polygon.Contains( _other );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public bool HasIntersection_Edge() {
		return _polygon.HasIntersection( _edge );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public bool HasIntersection_Polygon() {
		return _polygon.HasIntersection( _other );
	}

	[Benchmark, BenchmarkCategory( "HasIntersection" )]
	public bool HasIntersection_EdgeArray() {
		return _polygon.HasIntersection( _other.Edges );
	}

	[Benchmark]
	public bool HasOverlap() {
		return _polygon.HasOverlap( _other );
	}

	[Benchmark]
	public bool IsEquivalentTo() {
		return _polygon.IsEquivalentTo( _poly2 );
	}

	[Benchmark]
	public (bool Found, Polygon Intersection) TryIntersect_WithIntersection() {
		bool found = _i1.TryIntersect( _i2, out Polygon intersection );
		return (found, intersection);
	}

	[Benchmark]
	public (bool Found, Polygon Intersection) TryIntersect_WithoutIntersection() {
		bool found = _i1.TryIntersect( _farOther, out Polygon intersection );
		return (found, intersection);
	}

	[Benchmark]
	public (bool Found, IReadOnlyList<Point> Intersections) TryFindBoundaryIntersections_Edge_WithIntersections() {
		bool found = _polygon.TryFindBoundaryIntersections( _edge, out IReadOnlyList<Point> intersections );
		return (found, intersections);
	}

	[Benchmark]
	public (bool Found, IReadOnlyList<Point> Intersections) TryFindBoundaryIntersections_Edge_WithoutIntersections() {
		bool found = _polygon.TryFindBoundaryIntersections( _farEdge, out IReadOnlyList<Point> intersections );
		return (found, intersections);
	}

	[Benchmark]
	public (bool Found, IReadOnlyList<Point> Intersections) TryFindBoundaryIntersections_Polygon_WithIntersections() {
		bool found = _polygon.TryFindBoundaryIntersections( _other, out IReadOnlyList<Point> intersections );
		return (found, intersections);
	}

	[Benchmark]
	public (bool Found, IReadOnlyList<Point> Intersections) TryFindBoundaryIntersections_Polygon_WithoutIntersections() {
		bool found = _polygon.TryFindBoundaryIntersections( _farOther, out IReadOnlyList<Point> intersections );
		return (found, intersections);
	}
}
