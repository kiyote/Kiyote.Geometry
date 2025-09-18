namespace Kiyote.Geometry.Profiler;

public sealed class RectProfiler {

	public const int Iterations = 10000;

	private readonly Rect _rect1;
	private readonly IRect _rect2;

	public RectProfiler() {
		_rect1 = new Rect( 0, 0, 50, 50 );
		_rect2 = new Rect( 10, 10, 50, 50 );
	}

	public void Profile() {
		for (int i = 0; i < Iterations; i++) {
			_ = _rect1.Contains( _rect2 );
		}
	}
}
