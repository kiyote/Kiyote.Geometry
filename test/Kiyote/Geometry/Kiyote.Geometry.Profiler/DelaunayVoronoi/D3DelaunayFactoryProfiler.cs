using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Profiler;

public sealed class D3DelaunayFactoryProfiler {

	public const int Iterations = 10000;
	public const int Separation = 5;
	private readonly IReadOnlyList<Point> _points;
	private readonly double[] _coords;
	private readonly MapboxDelaunator _delaunator;

	public D3DelaunayFactoryProfiler() {
		ISize size = new Point( 1000, 1000 );
		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_points = pointFactory.Fill( size, Separation );
		_coords = _points.ToCoords();
		_delaunator = MapboxDelaunatorFactory.Create( _coords );
	}

	public void Profile() {
		for( int i = 0; i < Iterations; i++ ) {
			 D3DelaunayFactory.Create( _coords, _delaunator );
		}
	}
}
