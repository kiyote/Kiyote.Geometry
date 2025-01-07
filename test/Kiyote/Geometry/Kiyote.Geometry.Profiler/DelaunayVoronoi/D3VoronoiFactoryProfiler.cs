using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.DelaunayVoronoi.Profiler;

public sealed class D3VoronoiFactoryProfiler {

	public const int Iterations = 1000;
	public const int Separation = 5;
	private readonly IReadOnlyList<Point> _points;
	private readonly IVoronoiFactory _voronoiFactory;
	private readonly Rect _bounds;

	public D3VoronoiFactoryProfiler() {
		ISize size = new Point( 1000, 1000 );
		_bounds = new Rect( 0, 0, size );

		IRandom random = new FastRandom();
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
		_points = pointFactory.Fill( size, Separation );

		_voronoiFactory = new D3VoronoiFactory();
	}

	public void Profile() {
		for( int i = 0; i < Iterations; i++ ) {
			 _voronoiFactory.Create( _bounds, _points );
		}
	}
}
