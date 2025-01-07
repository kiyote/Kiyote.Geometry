using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class PolygonBenchmarks {

	private readonly IReadOnlyList<Point> _points;
	private readonly Polygon _polygon;
	private readonly Polygon _other;

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

		_other = new Polygon(
			[
				new Point( 250, 250 ),
				new Point( size.Width - 250, 350 ),
				new Point( size.Width - 350, size.Height - 150 ),
				new Point( 150, size.Height - 300 )
			]
		);
	}

	[Benchmark]
	public void TryFindIntersection() {
		 _polygon.TryFindIntersection( _other, out Polygon _ );
	}

	[Benchmark]
	public void Contains() {
		 _polygon.Contains( _points[0] );
	}

	[Benchmark]
	public void Contains_1000x1000() {
		for (int i = 0; i < _points.Count; i++) {
			 _polygon.Contains( _points[i] );
		}
	}

	[Benchmark]
	public void Intersections() {
		 _polygon.Intersections( _other.Points );
	}
}
