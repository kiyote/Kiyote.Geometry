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

	/// <summary>
	/// Receives a contiguous horizontal run of pixels from <paramref name="xMin"/> to
	/// <paramref name="xMax"/> inclusive, all on row <paramref name="y"/>.  Rasterizers
	/// call this instead of <see cref="Pixel"/> whenever they already know a whole run,
	/// which lets a sink backed by contiguous storage fill the run in one vectorized
	/// write rather than one call per pixel. Implement this explicitly to avoid
	/// default-interface boxing. Per-pixel sinks can delegate to <see cref="EmitSpan{TPixelOperation}"/>.
	/// </summary>
	void PixelSpan(
		int xMin,
		int xMax,
		int y
	);

	/// <summary>
	/// Emits a bounded rasterizer run through a value copy of the sink, without
	/// boxing. Keep observable state in referenced or span-backed storage, or
	/// implement the span loop directly when the sink needs mutable value state.
	/// </summary>
	public static void EmitSpan<TPixelOperation>(
		TPixelOperation operation,
		int xMin,
		int xMax,
		int y
	) where TPixelOperation : struct, IPixelOperation, allows ref struct {
		for( long x = xMin; x <= xMax; x++ ) {
			operation.Pixel( (int)x, y );
		}
	}
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

	public void PixelSpan( int xMin, int xMax, int y ) =>
		IPixelOperation.EmitSpan( this, xMin, xMax, y );

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
