namespace Kiyote.Geometry.DelaunayVoronoi;

internal static class DiagramSites {

	internal static int GetTriangleCapacity( int pointCount ) {
		long capacity = Math.Max( 2L * pointCount - 5, 0 ) * 3;
		if( pointCount < 0 || capacity > Array.MaxLength ) {
			throw new ArgumentOutOfRangeException( nameof( pointCount ), "Site count exceeds representable triangulation array capacities." );
		}
		return (int)capacity;
	}

	internal static Point[] SnapshotAndConvert(
		IReadOnlyList<Point> points,
		bool sanitize,
		out double[] coordinates,
		Rect? bounds = null
	) {
		ArgumentNullException.ThrowIfNull( points );
		_ = GetTriangleCapacity( points.Count );
		if( points.Count < 3 ) {
			throw new ArgumentException( "At least three distinct, noncollinear sites are required.", nameof( points ) );
		}

		Point[] snapshot = new Point[points.Count];
		coordinates = sanitize ? [] : new double[checked( points.Count * 2 )];
		HashSet<Point>? seen = sanitize ? [] : null;
		int count = 0;
		Point first = default;
		Point second = default;
		bool hasSecond = false;
		bool noncollinear = false;
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
			if( !noncollinear ) {
				if( count == 0 ) {
					first = point;
				} else if( !hasSecond ) {
					if( point != first ) {
						second = point;
						hasSecond = true;
					}
				} else {
					noncollinear = ( (long)second.X - first.X ) * ( point.Y - first.Y )
						!= ( (long)second.Y - first.Y ) * ( point.X - first.X );
				}
			}
			if( !sanitize ) {
				coordinates[count * 2] = point.X;
				coordinates[(count * 2) + 1] = point.Y;
			}
			snapshot[count++] = point;
		}
		if( !noncollinear ) {
			throw new ArgumentException( "At least three distinct, noncollinear sites are required.", nameof( points ) );
		}
		if( count != snapshot.Length ) {
			Array.Resize( ref snapshot, count );
		}
		if( sanitize ) {
			coordinates = snapshot.ToCoords();
		}
		return snapshot;
	}

	internal static void ValidateBounds( Rect bounds ) {
		if( bounds.Width < 2 || bounds.Height < 2 ) {
			throw new ArgumentOutOfRangeException( nameof( bounds ), "Voronoi clipping bounds must enclose positive geometric area." );
		}
		GeometryLimits.Validate( new Point( bounds.X1, bounds.Y1 ), nameof( bounds ) );
		GeometryLimits.Validate( new Point( bounds.X2, bounds.Y2 ), nameof( bounds ) );
	}
}
