namespace Kiyote.Geometry;

public sealed class Edge : IEdge {

	public Edge(
		IPoint a,
		IPoint b
	) {
		A = a;
		B = b;
	}

	public IPoint A { get; }

	public IPoint B { get; }

	public bool Equals(
		IPoint a,
		IPoint b
	) {
		return ( A.Equals( a ) && B.Equals( b ) )
			|| ( A.Equals( b ) && B.Equals( a ) );
	}

	public bool Equals(
		IEdge? other
	) {
		if( other is null ) {
			return false;
		}

		return Equals( other.A, other.B );
	}

	public override bool Equals(
		object? obj
	) {
		return Equals( obj as Edge );
	}

	public override int GetHashCode() {
		return HashCode.Combine( A, B );
	}
}
