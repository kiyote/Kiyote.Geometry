using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Profiler;

public sealed class DelaunatorFactoryProfiler {

	public const int Iterations = 100;
	public const int Separation = 5;
	private readonly IDelaunatorFactory _delaunatorFactory;
	private readonly IReadOnlyList<IPoint> _points;	

	public DelaunatorFactoryProfiler() {
		Bounds bounds = new Bounds( 1000, 1000 );
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_points = pointFactory.Fill( bounds, Separation );
		_delaunatorFactory = new DelaunatorFactory();
	}

	public void Profile() {
		for (int i = 0; i < Iterations; i++) {
			_ = _delaunatorFactory.Create( _points );
		}
	}
}

