namespace Kiyote.Geometry.Rasterizers;

/// <summary>
/// Receives the pixels produced by a rasterizer.  Implement this on a struct so
/// that the rasterizer can inline the callback rather than invoking a delegate
/// once per pixel.
/// </summary>
public interface IPixelOperation {

	void Pixel(
		int x,
		int y
	);
}

/// <summary>
/// Adapts a delegate to <see cref="IPixelOperation"/> so the delegate based
/// overloads can share the same implementation as the struct based ones.
/// </summary>
public readonly struct ActionPixelOperation(
	Action<int, int> pixelAction
) : IPixelOperation, IEquatable<ActionPixelOperation> {

	public void Pixel(
		int x,
		int y
	) {
		pixelAction( x, y );
	}

	public bool Equals(
		ActionPixelOperation other
	) {
		return ReferenceEquals( pixelAction, other.PixelAction );
	}

	public override bool Equals(
		object? obj
	) {
		return obj is ActionPixelOperation other && Equals( other );
	}

	public override int GetHashCode() {
		return pixelAction?.GetHashCode() ?? 0;
	}

	public static bool operator ==(
		ActionPixelOperation left,
		ActionPixelOperation right
	) {
		return left.Equals( right );
	}

	public static bool operator !=(
		ActionPixelOperation left,
		ActionPixelOperation right
	) {
		return !left.Equals( right );
	}

	private Action<int, int> PixelAction => pixelAction;
}
