namespace Kiyote.Geometry;

public static class Intersect {

	/// <summary>
	/// Returns whether two closed segments share any point, including endpoints,
	/// point-segments, and collinear overlap. Coordinates must satisfy
	/// <see cref="GeometryLimits"/>.
	/// </summary>
	public static bool HasIntersection(
		int aX1, int aY1, int bX1, int bY1,
		int aX2, int aY2, int bX2, int bY2
	) {
		ValidateEndpoints( aX1, aY1, bX1, bY1, aX2, aY2, bX2, bY2 );
		return HasIntersectionUnchecked(
			new Edge( aX1, aY1, bX1, bY1 ),
			new Edge( aX2, aY2, bX2, bY2 )
		);
	}

	/// <summary>
	/// Finds a unique shared point of two closed segments. Coordinates must satisfy
	/// <see cref="GeometryLimits"/>. Returns false for disjoint segments or a shared
	/// length, setting both outputs to zero. Determines existence before rounding
	/// coordinates to nearest, with exact halves to even; the rounded approximation
	/// need not lie exactly on the mathematical segments.
	/// </summary>
	public static bool TryFindIntersection(
		int aX1, int aY1, int bX1, int bY1,
		int aX2, int aY2, int bX2, int bY2,
		out int intersectionX, out int intersectionY
	) {
		ValidateEndpoints( aX1, aY1, bX1, bY1, aX2, aY2, bX2, bY2 );
		bool found = TryFindIntersectionUnchecked(
			new Edge( aX1, aY1, bX1, bY1 ),
			new Edge( aX2, aY2, bX2, bY2 ),
			out Point point
		);
		intersectionX = found ? point.X : 0;
		intersectionY = found ? point.Y : 0;
		return found;
	}

	internal static bool HasIntersectionUnchecked( Edge a, Edge b ) {
		long denominator = GetParameters( a, b, out long numeratorA, out long numeratorB );
		if( denominator == 0 ) {
			return numeratorA == 0 && numeratorB == 0
				&& TryGetSharedBounds( a, b, out _, out _ );
		}
		return numeratorA >= 0 && numeratorA <= denominator
			&& numeratorB >= 0 && numeratorB <= denominator;
	}

	internal static bool TryFindIntersectionUnchecked( Edge a, Edge b, out Point point ) {
		point = Point.None;
		long denominator = GetParameters( a, b, out long numeratorA, out long numeratorB );
		if( denominator == 0 ) {
			if( numeratorA != 0 || numeratorB != 0
				|| !TryGetSharedBounds( a, b, out Point low, out Point high )
				|| low != high
			) {
				return false;
			}
			point = low;
			return true;
		}
		if( numeratorA < 0 || numeratorA > denominator
			|| numeratorB < 0 || numeratorB > denominator
		) {
			return false;
		}

		// Exact numerators and bounded rational spacing preserve half ties in double.
		long x = (a.A.X * denominator) + (numeratorA * ( a.B.X - a.A.X ));
		long y = (a.A.Y * denominator) + (numeratorA * ( a.B.Y - a.A.Y ));
		point = new Point(
			(int)Math.Round( (double)x / denominator ),
			(int)Math.Round( (double)y / denominator )
		);
		return true;
	}

	private static long GetParameters( Edge a, Edge b, out long numeratorA, out long numeratorB ) {
		long rx = (long)a.B.X - a.A.X;
		long ry = (long)a.B.Y - a.A.Y;
		long sx = (long)b.B.X - b.A.X;
		long sy = (long)b.B.Y - b.A.Y;
		long qx = (long)b.A.X - a.A.X;
		long qy = (long)b.A.Y - a.A.Y;
		long denominator = (rx * sy) - (ry * sx);
		numeratorA = (qx * sy) - (qy * sx);
		numeratorB = (qx * ry) - (qy * rx);
		if( denominator < 0 ) {
			numeratorA = -numeratorA;
			numeratorB = -numeratorB;
			return -denominator;
		}
		return denominator;
	}

	private static bool TryGetSharedBounds( Edge a, Edge b, out Point low, out Point high ) {
		low = Point.Max( Point.Min( a.A, a.B ), Point.Min( b.A, b.B ) );
		high = Point.Min( Point.Max( a.A, a.B ), Point.Max( b.A, b.B ) );
		return low.X <= high.X && low.Y <= high.Y;
	}

	private static void ValidateEndpoints(
		int aX1, int aY1, int bX1, int bY1,
		int aX2, int aY2, int bX2, int bY2
	) {
		GeometryLimits.ValidateCoordinate( aX1 );
		GeometryLimits.ValidateCoordinate( aY1 );
		GeometryLimits.ValidateCoordinate( bX1 );
		GeometryLimits.ValidateCoordinate( bY1 );
		GeometryLimits.ValidateCoordinate( aX2 );
		GeometryLimits.ValidateCoordinate( aY2 );
		GeometryLimits.ValidateCoordinate( bX2 );
		GeometryLimits.ValidateCoordinate( bY2 );
	}
}
