namespace Kiyote.Geometry;

public readonly struct Edge: IEquatable<Edge> {

	public Edge(
		Point a,
		Point b
	) {
		A = a;
		B = b;
	}

	public readonly Point A { get; }

	public readonly Point B { get; }

	public bool Equals(
		Edge other
	) {
		return other.A == A
			&& other.B == B;
	}

	public override bool Equals( object? obj ) {
		return obj is Edge edge && Equals( edge );
	}

	public override int GetHashCode() {
		return HashCode.Combine( A, B );
	}

	public static bool operator ==( Edge left, Edge right ) {
		return left.Equals( right );
	}

	public static bool operator !=( Edge left, Edge right ) {
		return !( left == right );
	}
}
