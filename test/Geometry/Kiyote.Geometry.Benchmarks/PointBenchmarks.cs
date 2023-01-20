using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class PointBenchmarks {

	private readonly IReadOnlyList<IPoint> _points;
	private readonly IReadOnlyList<IPoint> _polygon;

	public PointBenchmarks() {
		Bounds bounds = new Bounds( 1000, 1000 );
		IPointFactory pointFactory = new FastPoissonDiscPointFactory(
			new FastRandom()
		);
		_points = pointFactory.Fill( bounds, 5 );


		_polygon = new List<IPoint>() {
			new Point( 200, 200 ),
			new Point( bounds.Width - 200, 200 ),
			new Point( bounds.Width - 200, bounds.Height - 200 ),
			new Point( 200, bounds.Height - 200 )
		};
	}

	[Benchmark]
	public void Inside() {
		_points[0].Inside( _polygon );
	}

	[Benchmark]
	public void Inside_1000x1000() {
		for (int i = 0; i < _points.Count; i++) {
			_points[i].Inside( _polygon );
		}
	}
}
