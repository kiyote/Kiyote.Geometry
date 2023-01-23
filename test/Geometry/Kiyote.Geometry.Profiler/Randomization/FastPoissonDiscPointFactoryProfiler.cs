namespace Kiyote.Geometry.Randomization.Profiler;

public sealed class FastPoissonDiscPointFactoryProfiler {

	public const int Iterations = 100;
	public const int Separation = 5;

	private readonly IPointFactory _pointFactory;
	private readonly Bounds _bounds;

	public FastPoissonDiscPointFactoryProfiler() {
		_bounds = new Bounds( 1000, 1000 );
		IRandom random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( random );
	}

	public void Profile() {
		for (int i = 0; i < Iterations;i++ ) {
			_ = _pointFactory.Fill( _bounds, Separation );
		}
	}
}

