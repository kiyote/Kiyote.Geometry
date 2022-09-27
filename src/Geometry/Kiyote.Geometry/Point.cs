namespace Kiyote.Geometry;

/// <summary>
/// Provides an interface for a integer-based coordinate point
/// </summary>
/// <remarks>
/// This was tested as a `record` and it was found to have worse performance.
/// </remarks>
public sealed class Point : IPoint {

	public Point(
		int x,
		int y
	) {
		X = x;
		Y = y;
	}

	public int X { get; }

	public int Y { get; }

	public bool Equals(
		int x,
		int y
	) {
		return X == x && Y == y;
	}

	public bool Equals(
		IPoint? other
	) {
		if( other is null ) {
			return false;
		}

		return Equals( other.X, other.Y );
	}

	public override bool Equals(
		object? obj
	) {
		if( obj is null ) {
			return false;
		}

		return Equals( obj as IPoint );
	}

	public override int GetHashCode() {
		return HashCode.Combine( X, Y );
	}
}


