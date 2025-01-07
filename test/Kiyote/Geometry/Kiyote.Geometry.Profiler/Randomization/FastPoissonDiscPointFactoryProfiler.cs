namespace Kiyote.Geometry.Randomization.Profiler;

public sealed class FastPoissonDiscPointFactoryProfiler {

	public const int Iterations = 100;
	public const int Separation = 5;

	private readonly IPointFactory _pointFactory;
	private readonly ISize _bounds;

	public FastPoissonDiscPointFactoryProfiler() {
		_bounds = new Point( 1000, 1000 );
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
	}

	public void Profile() {
		for (int i = 0; i < Iterations;i++ ) {
			 _pointFactory.Fill( _bounds, Separation );
		}
	}
}

