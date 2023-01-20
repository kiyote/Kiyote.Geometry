namespace Kiyote.Geometry;

public interface IPoint : IEquatable<IPoint> {
	public int X { get; }

	public int Y { get; }

	bool Inside( IReadOnlyList<IPoint> polygon );
}
