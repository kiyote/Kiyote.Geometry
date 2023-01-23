using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class PolygonBenchmarks {

	private readonly IReadOnlyList<IPoint> _points;
	private readonly IPolygon _polygon;
	private readonly IPolygon _other;

	public PolygonBenchmarks() {
		Bounds bounds = new Bounds( 1000, 1000 );
		IPointFactory pointFactory = new FastPoissonDiscPointFactory(
			new FastRandom()
		);
		_points = pointFactory.Fill( bounds, 5 );


		_polygon = new Polygon(
			new List<IPoint>() {
				new Point( 200, 200 ),
				new Point( bounds.Width - 200, 200 ),
				new Point( bounds.Width - 200, bounds.Height - 200 ),
				new Point( 200, bounds.Height - 200 )
			}
		);

		_other = new Polygon(
			new List<IPoint>() {
				new Point( 250, 250 ),
				new Point( bounds.Width - 250, 350 ),
				new Point( bounds.Width - 350, bounds.Height - 150 ),
				new Point( 150, bounds.Height - 300 )
			}
		);
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
