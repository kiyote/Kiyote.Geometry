namespace Kiyote.Geometry;

public static class Intersect {

	/// <summary>
	/// Determines if the edge defined by (aX1,aY1)->(bX1,bY1) intersects
	/// the edge defined by (aX2,aY2)->(bX2,bY2).
	/// </summary>
	/// <param name="aX1">The X coordinate of the first point of the first edge.</param>
	/// <param name="aY1">The Y coordinate of the first point of the first edge.</param>
	/// <param name="bX1">The X coordinate of the second point of the first edge.</param>
	/// <param name="bY1">The Y coordinate of the second point of the first edge.</param>
	/// <param name="aX2">The X coordinate of the first point of the second edge.</param>
	/// <param name="aY2">The Y coordinate of the first point of the second edge.</param>
	/// <param name="bX2">The X coordinate of the second point of the second edge.</param>
	/// <param name="bY2">The Y coordinate of the second point of the second edge.</param>
	/// <returns>
	/// Returns <c>True</c> if an intersection occurs, otherwise returns <c>False</c>.
	/// </returns>
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

	/// <summary>
	/// Determines if the edge defined by (aX1,aY1)->(bX1,bY1) intersects
	/// the edge defined by (aX2,aY2)->(bX2,bY2).
	/// </summary>
	/// <param name="aX1">The X coordinate of the first point of the first edge.</param>
	/// <param name="aY1">The Y coordinate of the first point of the first edge.</param>
	/// <param name="bX1">The X coordinate of the second point of the first edge.</param>
	/// <param name="bY1">The Y coordinate of the second point of the first edge.</param>
	/// <param name="aX2">The X coordinate of the first point of the second edge.</param>
	/// <param name="aY2">The Y coordinate of the first point of the second edge.</param>
	/// <param name="bX2">The X coordinate of the second point of the second edge.</param>
	/// <param name="bY2">The Y coordinate of the second point of the second edge.</param>
	/// <param name="intersection">
	/// The point at which the two edges intersect, if at all.  Otherwise will
	/// contain <c>Point.None</c> if there is no intersection.
	/// </param>
	/// <returns>
	/// Returns <c>True</c> if an intersection occurs, otherwise returns <c>False</c>.
	/// </returns>
	public static bool TryFindIntersection(
		int aX1,
		int aY1,
		int bX1,
		int bY1,
		int aX2,
		int aY2,
		int bX2,
		int bY2,
		out int intersectionX,
		out int intersectionY
	) {
		// Make sure none of the lines are zero length
		if( ( aX1 == bX1 && aY1 == bY1 )
			|| ( aX2 == bX2 && aY2 == bY2 )
		) {
			intersectionX = 0;
			intersectionY = 0;
			return false;
		}

		double denominator = ( ( ( bY2 - aY2 ) * ( bX1 - aX1 ) )
			- ( ( bX2 - aX2 ) * ( bY1 - aY1 ) ) );

		// If this is zero then the lines are parallel
		if( denominator == 0.0 ) {
			intersectionX = 0;
			intersectionY = 0;
			return false;
		}

		double ua = ( ( ( bX2 - aX2 ) * ( aY1 - aY2 ) )
			- ( ( bY2 - aY2 ) * ( aX1 - aX2 ) ) ) / denominator;

		double ub = ( ( ( bX1 - aX1 ) * ( aY1 - aY2 ) )
			- ( ( bY1 - aY1 ) * ( aX1 - aX2 ) ) ) / denominator;

		// Is the intersection somewhere along actual line segments?
		if( ua < 0 || ua > 1 || ub < 0 || ub > 1 ) {
			intersectionX = 0;
			intersectionY = 0;
			return false;
		}

		double x = aX1 + ( ua * ( bX1 - aX1 ) );
		double y = aY1 + ( ua * ( bY1 - aY1 ) );

		intersectionX = (int)Math.Round( x );
		intersectionY = (int)Math.Round( y );

		return true;
	}
}
