using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Profiler;

public sealed class MapboxDelaunatorFactoryProfiler {

	public const int Iterations = 100;
	public const int Separation = 5;
	private readonly IReadOnlyList<Point> _points;
	private readonly double[] _coords;

	public MapboxDelaunatorFactoryProfiler() {
		ISize bounds = new Point( 1000, 1000 );
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_points = pointFactory.Fill( bounds, Separation );
		_coords = _points.ToCoords();
	}

	public void Profile() {
		for (int i = 0; i < Iterations; i++) {
			_ = MapboxDelaunatorFactory.Create( _coords );
		}
	}
}
