namespace Kiyote.Geometry.Randomization.Profiler;

public sealed class FastPoissonDiscPointFactoryProfiler {

	public const int Iterations = 100;
	public const int Separation = 5;

	private readonly IPointFactory _pointFactory;
	private readonly IRandom _random;

	public FastPoissonDiscPointFactoryProfiler() {
		_random = new FastRandom();
		_pointFactory = new FastPoissonDiscPointFactory( _random );
	}

	public void Profile() {
		for (int i = 0; i < Iterations;i++ ) {
			_random.Reinitialise( 0xF00D );
			_pointFactory.Fill( 1000, 1000, Separation );
		}
	}
}

