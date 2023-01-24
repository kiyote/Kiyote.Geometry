namespace Kiyote.Geometry;

public readonly struct Triangle : IEquatable<Triangle> {

	public Triangle(
		Point p1,
		Point p2,
		Point p3
	) {
		P1 = p1;
		P2 = p2;
		P3 = p3;
	}

	public readonly Point P1;

	public readonly Point P2;

	public readonly Point P3;

	public bool Equals(
		Triangle other
	) {
		return P1.Equals( other.P1 )
			&& P2.Equals( other.P2 )
			&& P3.Equals( other.P3 );
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Triangle triangle && Equals( triangle );
	}

	public override int GetHashCode() {
		return HashCode.Combine( P1, P2, P3 );
	}

	public static bool operator ==( Triangle left, Triangle right ) {
		return left.Equals( right );
	}

	public static bool operator !=( Triangle left, Triangle right ) {
		return !( left == right );
	}
}
