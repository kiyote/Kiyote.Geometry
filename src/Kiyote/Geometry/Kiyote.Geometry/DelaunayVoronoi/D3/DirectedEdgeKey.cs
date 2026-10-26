namespace Kiyote.Geometry.DelaunayVoronoi.D3;

internal readonly record struct DirectedEdgeKey( uint Start, uint End ) {
	internal DirectedEdgeKey( Edge edge ) : this( Pack( edge.A ), Pack( edge.B ) ) {
	}

	internal Edge ToEdge() {
		return new Edge( Unpack( Start ), Unpack( End ) );
	}

	public override int GetHashCode() {
		// XOR hashing a packed ulong collapses many axis-aligned grid edges into the same bucket.
		return HashCode.Combine( Start, End );
	}

	private static uint Pack( Point point ) {
		return ((uint)(point.X - GeometryLimits.MinimumCoordinate) << 16)
			| (uint)(point.Y - GeometryLimits.MinimumCoordinate);
	}

	private static Point Unpack( uint key ) {
		return new Point( (int)(key >> 16) + GeometryLimits.MinimumCoordinate,
			(int)(key & 0xFFFF) + GeometryLimits.MinimumCoordinate );
	}
}
