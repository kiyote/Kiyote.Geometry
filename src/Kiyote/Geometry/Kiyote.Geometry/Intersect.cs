using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry;

public static class Intersect {

	public static bool HasIntersection(
		int aX1,
		int aY1,
		int bX1,
		int bY1,
		int aX2,
		int aY2,
		int bX2,
		int bY2
	) {
		// Make sure none of the lines are zero length
		if( ( aX1 == bX1 && aY1 == bY1 )
			|| ( aX2 == bX2 && aY2 == bY2 )
		) {
			return false;
		}

		double denominator = ( ( ( bY2 - aY2 ) * ( bX1 - aX1 ) )
			- ( ( bX2 - aX2 ) * ( bY1 - aY1 ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			return false;
		}

		double ua = ( ( ( bX2 - aX2 ) * ( aY1 - aY2 ) )
			- ( ( bY2 - aY2 ) * ( aX1 - aX2 ) ) ) / denominator;

		double ub = ( ( ( bX1 - aX1 ) * ( aY1 - aY2 ) )
			- ( ( bY1 - aY1 ) * ( aX1 - aX2 ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			return false;
		}

		return true;
	}

	public static bool TryFindIntersection(
		int aX1,
		int aY1,
		int bX1,
		int bY1,
		int aX2,
		int aY2,
		int bX2,
		int bY2,
		[NotNullWhen(true)]
		out Point? intersection
	) {
		// Make sure none of the lines are zero length
		if( ( aX1 == bX1 && aY1 == bY1 )
			|| ( aX2 == bX2 && aY2 == bY2 )
		) {
			intersection = null;
			return false;
		}

		double denominator = ( ( ( bY2 - aY2 ) * ( bX1 - aX1 ) )
			- ( ( bX2 - aX2 ) * ( bY1 - aY1 ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			intersection = null;
			return false;
		}

		double ua = ( ( ( bX2 - aX2 ) * ( aY1 - aY2 ) )
			- ( ( bY2 - aY2 ) * ( aX1 - aX2 ) ) ) / denominator;

		double ub = ( ( ( bX1 - aX1 ) * ( aY1 - aY2 ) )
			- ( ( bY1 - aY1 ) * ( aX1 - aX2 ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			intersection = null;
			return false;
		}

		double x = aX1 + ( ua * ( bX1 - aX1 ) );
		double y = aY1 + ( ua * ( bY1 - aY1 ) );

		intersection = new Point( (int)Math.Round( x ), (int)Math.Round( y ) );
		return true;
	}
}
