namespace Kiyote.Geometry.DelaunayVoronoi.D3;

internal static class DiagramSites {

	internal static IReadOnlyList<Point> Snapshot(
		IReadOnlyList<Point> points,
		bool sanitize,
		Rect? bounds = null
	) {
		ArgumentNullException.ThrowIfNull( points );
		_ = MapboxDelaunatorFactory.GetTriangleCapacity( points.Count );
		if( points.Count < 3 ) {
			throw new ArgumentException( "At least three distinct, noncollinear sites are required.", nameof( points ) );
		}

		Point[] snapshot = new Point[points.Count];
		HashSet<Point>? seen = sanitize ? [] : null;
		int count = 0;
		for( int i = 0; i < points.Count; i++ ) {
			Point point = points[i];
			GeometryLimits.Validate( point, nameof( points ) );
			if( bounds is Rect rectangle && !rectangle.Contains( point ) ) {
				if( sanitize ) {
					continue;
				}
				throw new ArgumentException( "Voronoi sites must lie within the clipping bounds.", nameof( points ) );
			}
			if( seen is not null && !seen.Add( point ) ) {
				continue;
			}
			snapshot[count++] = point;
		}

		bool noncollinear = false;
		if( count >= 3 ) {
			Point a = snapshot[0];
			int second = 1;
			while( second < count && snapshot[second] == a ) {
				second++;
			}
			if( second < count ) {
				Point b = snapshot[second];
				for( int i = second + 1; i < count; i++ ) {
					Point c = snapshot[i];
					if( ( (long)b.X - a.X ) * ( c.Y - a.Y ) != ( (long)b.Y - a.Y ) * ( c.X - a.X ) ) {
						noncollinear = true;
						break;
					}
				}
			}
		}
		if( !noncollinear ) {
			throw new ArgumentException( "At least three distinct, noncollinear sites are required.", nameof( points ) );
		}
		if( count != snapshot.Length ) {
			Array.Resize( ref snapshot, count );
		}
		return Array.AsReadOnly( snapshot );
	}

	internal static void ValidateBounds( Rect bounds ) {
		if( bounds.Width < 2 || bounds.Height < 2 ) {
			throw new ArgumentOutOfRangeException( nameof( bounds ), "Voronoi clipping bounds must enclose positive geometric area." );
		}
		GeometryLimits.Validate( new Point( bounds.X1, bounds.Y1 ), nameof( bounds ) );
		GeometryLimits.Validate( new Point( bounds.X2, bounds.Y2 ), nameof( bounds ) );
	}
}
