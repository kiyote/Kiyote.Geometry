using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Kiyote.Geometry;

/// <summary>
/// An immutable convex polygon of bounded integer coordinates, or an empty value.
/// Construction preserves boundary order while removing harmless redundancy;
/// concave, self-intersecting, backtracking, and degenerate nonempty input is rejected.
/// </summary>
public sealed class Polygon {

	public const int MaximumVertices = 128;
	public static readonly Polygon None = new Polygon( ImmutableArray<Point>.Empty );

	private readonly ImmutableArray<Point> _points;
	private readonly int _winding;

	public Polygon(
		IReadOnlyList<Point> points
	) : this( Normalize( points ) ) {
	}

	private Polygon(
		ImmutableArray<Point> ownedPoints
	) {
		_points = ownedPoints;
		Points = ownedPoints;
		_winding = ownedPoints.IsEmpty ? 0 : Math.Sign( Orientation( ownedPoints[0], ownedPoints[1], ownedPoints[2] ) );
		Edge[] edges = ownedPoints.IsEmpty ? [] : new Edge[ownedPoints.Length];
		for( int i = 0; i < edges.Length; i++ ) {
			edges[i] = new Edge( ownedPoints[i], ownedPoints[( i + 1 ) % edges.Length] );
		}
		Edges = ImmutableCollectionsMarshal.AsImmutableArray( edges );
	}

	public IReadOnlyList<Point> Points { get; }

	public ImmutableArray<Edge> Edges { get; }

	public bool IsEmpty => _points.IsEmpty;

	/// <summary>Returns whether any point is shared, including contact and containment.</summary>
	public bool HasIntersection(
		Polygon polygon
	) {
		ArgumentNullException.ThrowIfNull( polygon );
		if( IsEmpty || polygon.IsEmpty ) {
			return false;
		}
		if( ContainsUnchecked( polygon._points[0] ) || polygon.ContainsUnchecked( _points[0] ) ) {
			return true;
		}
		foreach( Edge a in Edges ) {
			foreach( Edge b in polygon.Edges ) {
				if( Intersect.HasIntersectionUnchecked( a, b ) ) {
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Returns whether any supplied segment shares a point with this polygon.</summary>
	public bool HasIntersection(
		ImmutableArray<Edge> edges
	) {
		if( edges.IsDefault ) {
			throw new ArgumentException( "The edge collection must be initialized.", nameof( edges ) );
		}
		foreach( Edge edge in edges ) {
			Validate( edge );
		}
		foreach( Edge edge in edges ) {
			if( HasIntersectionUnchecked( edge ) ) {
				return true;
			}
		}
		return false;
	}

	/// <summary>Includes boundary contact and segments or points wholly inside the polygon.</summary>
	public bool HasIntersection(
		Edge edge
	) {
		Validate( edge );
		return HasIntersectionUnchecked( edge );
	}

	private bool HasIntersectionUnchecked(
		Edge edge
	) {
		if( ContainsUnchecked( edge.A ) || ContainsUnchecked( edge.B ) ) {
			return true;
		}
		foreach( Edge boundary in Edges ) {
			if( Intersect.HasIntersectionUnchecked( boundary, edge ) ) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Collects distinct nearest-even rounded point intersections with boundary edges.
	/// A collinear shared length contributes no arbitrary point; contacts with other
	/// edges can still contribute its endpoints. Interior containment is not a boundary intersection.
	/// </summary>
	public bool TryFindBoundaryIntersections(
		Edge edge,
		out IReadOnlyList<Point> intersections
	) {
		Validate( edge );
		Span<Point> points = stackalloc Point[2];
		int count = 0;
		foreach( Edge boundary in Edges ) {
			if( Intersect.TryFindIntersectionUnchecked( boundary, edge, out Point point ) ) {
				AddUnique( points, ref count, point );
			}
		}
		return PublishIntersections( points[..count], out intersections );
	}

	/// <summary>
	/// Collects distinct rounded point intersections between boundary-edge pairs,
	/// not contained vertices or collinear shared lengths.
	/// </summary>
	public bool TryFindBoundaryIntersections(
		Polygon polygon,
		out IReadOnlyList<Point> intersections
	) {
		ArgumentNullException.ThrowIfNull( polygon );
		Span<Point> points = stackalloc Point[_points.Length + polygon._points.Length];
		int count = 0;
		foreach( Edge a in Edges ) {
			foreach( Edge b in polygon.Edges ) {
				if( Intersect.TryFindIntersectionUnchecked( a, b, out Point point ) ) {
					AddUnique( points, ref count, point );
				}
			}
		}
		return PublishIntersections( points[..count], out intersections );
	}

	/// <summary>Tests inclusive containment of a bounded position.</summary>
	public bool Contains(
		Point target
	) {
		GeometryLimits.Validate( target, nameof( target ) );
		return ContainsUnchecked( target );
	}

	public bool Contains(
		int x,
		int y
	) {
		GeometryLimits.ValidateCoordinate( x );
		GeometryLimits.ValidateCoordinate( y );
		return ContainsUnchecked( new Point( x, y ) );
	}

	private bool ContainsUnchecked(
		Point point
	) {
		if( IsEmpty ) {
			return false;
		}
		foreach( Edge edge in Edges ) {
			if( Orientation( edge.A, edge.B, point ) * _winding < 0 ) {
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns whether the entire nonempty target lies inside or on the boundary.</summary>
	public bool Contains(
		Polygon polygon
	) {
		ArgumentNullException.ThrowIfNull( polygon );
		if( polygon.IsEmpty ) {
			return false;
		}
		foreach( Point point in polygon._points ) {
			if( !ContainsUnchecked( point ) ) {
				return false;
			}
		}
		return true;
	}

	/// <summary>Tests every supplied point; an empty sequence is not contained.</summary>
	public bool Contains(
		IEnumerable<Point> points
	) {
		ArgumentNullException.ThrowIfNull( points );
		bool any = false;
		foreach( Point point in points ) {
			if( !Contains( point ) ) {
				return false;
			}
			any = true;
		}
		return any;
	}

	/// <summary>Alias for the shared-point predicate <see cref="HasIntersection(Polygon)"/>.</summary>
	public bool HasOverlap(
		Polygon polygon
	) {
		return HasIntersection( polygon );
	}

	/// <summary>
	/// Returns the convex hull of nearest-even rounded mathematical intersection
	/// vertices. Snapping can move boundaries slightly. Point/edge contact and
	/// quantization collapse return false and <see cref="None"/>.
	/// A result exceeding <see cref="MaximumVertices"/> throws <see cref="InvalidOperationException"/>.
	/// </summary>
	public bool TryIntersect(
		Polygon polygon,
		out Polygon intersectedPolygon
	) {
		ArgumentNullException.ThrowIfNull( polygon );
		intersectedPolygon = None;
		if( IsEmpty || polygon.IsEmpty ) {
			return false;
		}
		if( Contains( polygon ) ) {
			intersectedPolygon = polygon;
			return true;
		}
		if( polygon.Contains( this ) ) {
			intersectedPolygon = this;
			return true;
		}

		// Convex intersection vertices are contained input vertices or original
		// edge crossings. Rounding never feeds back into those exact decisions.
		Span<Point> candidates = stackalloc Point[_points.Length + polygon._points.Length];
		int count = 0;
		foreach( Point point in _points ) {
			if( polygon.ContainsUnchecked( point ) ) {
				AddUnique( candidates, ref count, point );
			}
		}
		foreach( Point point in polygon._points ) {
			if( ContainsUnchecked( point ) ) {
				AddUnique( candidates, ref count, point );
			}
		}
		foreach( Edge a in Edges ) {
			foreach( Edge b in polygon.Edges ) {
				if( Intersect.TryFindIntersectionUnchecked( a, b, out Point point ) ) {
					AddUnique( candidates, ref count, point );
				}
			}
		}
		intersectedPolygon = CreateRoundedHull( candidates[..count] );
		return !intersectedPolygon.IsEmpty;
	}

	// Only generated convex geometry may repair quantization with a hull.
	// Public construction must reject, not repair, incorrectly ordered input.
	internal static Polygon FromRoundedConvexVertices(
		Span<Point> points
	) {
		if( points.Length > 2 * MaximumVertices ) {
			throw new ArgumentException( "Generated vertices exceed the bounded convex-hull scratch capacity.", nameof( points ) );
		}
		foreach( Point point in points ) {
			GeometryLimits.Validate( point, nameof( points ) );
		}
		return CreateRoundedHull( points );
	}

	private static Polygon CreateRoundedHull(
		Span<Point> candidates
	) {
		int count = candidates.Length;
		if( count < 3 ) {
			return None;
		}
		candidates.Sort( ComparePoints );
		Span<Point> hull = stackalloc Point[2 * count];
		int hullCount = 0;
		foreach( Point point in candidates ) {
			while( hullCount >= 2 && Orientation( hull[hullCount - 2], hull[hullCount - 1], point ) <= 0 ) {
				hullCount--;
			}
			hull[hullCount++] = point;
		}
		int upperStart = hullCount + 1;
		for( int i = candidates.Length - 2; i >= 0; i-- ) {
			Point point = candidates[i];
			while( hullCount >= upperStart && Orientation( hull[hullCount - 2], hull[hullCount - 1], point ) <= 0 ) {
				hullCount--;
			}
			hull[hullCount++] = point;
		}
		hullCount--;
		if( hullCount < 3 ) {
			return None;
		}
		if( hullCount > MaximumVertices ) {
			throw new InvalidOperationException( $"The generated polygon exceeds {MaximumVertices} normalized vertices." );
		}
		return new Polygon( Own( hull[..hullCount] ) );
	}

	/// <summary>Compares geometric boundaries independently of start, winding, and input redundancy.</summary>
	public bool IsEquivalentTo(
		Polygon other
	) {
		ArgumentNullException.ThrowIfNull( other );
		int count = _points.Length;
		if( count != other._points.Length ) {
			return false;
		}
		if( count == 0 ) {
			return true;
		}
		int start = other._points.IndexOf( _points[0] );
		if( start < 0 ) {
			return false;
		}
		int direction = _winding == other._winding ? 1 : -1;
		int index = start;
		for( int i = 1; i < count; i++ ) {
			index = ( index + direction + count ) % count;
			if( _points[i] != other._points[index] ) {
				return false;
			}
		}
		return true;
	}

	private static ImmutableArray<Point> Normalize(
		IReadOnlyList<Point> points
	) {
		ArgumentNullException.ThrowIfNull( points );
		if( points.Count == 0 ) {
			return ImmutableArray<Point>.Empty;
		}
		// A valid boundary can retain two extra vertices across the closing seam.
		Span<Point> normalized = stackalloc Point[Math.Min( points.Count, MaximumVertices + 2 )];
		int count = 0;
		for( int i = 0; i < points.Count; i++ ) {
			Point point = points[i];
			GeometryLimits.Validate( point, nameof( points ) );
			if( count > 0 && normalized[count - 1] == point ) {
				continue;
			}
			while( count >= 2 && Orientation( normalized[count - 2], normalized[count - 1], point ) == 0 ) {
				RequireBetween( normalized[count - 2], normalized[count - 1], point );
				count--;
			}
			if( count == normalized.Length ) {
				throw new ArgumentException( $"Polygon exceeds {MaximumVertices} normalized vertices.", nameof( points ) );
			}
			normalized[count++] = point;
		}
		if( count > 1 && normalized[count - 1] == normalized[0] ) {
			count--;
		}
		while( count >= 3 ) {
			if( Orientation( normalized[count - 2], normalized[count - 1], normalized[0] ) == 0 ) {
				RequireBetween( normalized[count - 2], normalized[count - 1], normalized[0] );
				count--;
			} else if( Orientation( normalized[count - 1], normalized[0], normalized[1] ) == 0 ) {
				RequireBetween( normalized[count - 1], normalized[0], normalized[1] );
				normalized.Slice( 1, --count ).CopyTo( normalized );
			} else {
				break;
			}
		}
		if( count < 3 ) {
			throw new ArgumentException( "A nonempty polygon must have positive area and at least three distinct corners.", nameof( points ) );
		}
		if( count > MaximumVertices ) {
			throw new ArgumentException( $"Polygon exceeds {MaximumVertices} normalized vertices.", nameof( points ) );
		}
		int winding = Math.Sign( Orientation( normalized[0], normalized[1], normalized[2] ) );
		for( int i = 0; i < count; i++ ) {
			int next = ( i + 1 ) % count;
			for( int j = 0; j < count; j++ ) {
				if( j != i && j != next
					&& Orientation( normalized[i], normalized[next], normalized[j] ) * winding <= 0
				) {
					throw new ArgumentException( "Vertices must trace a convex boundary without self-intersection or repeated corners.", nameof( points ) );
				}
			}
		}
		return Own( normalized[..count] );
	}

	private static void RequireBetween(
		Point start, Point middle, Point end
	) {
		if( middle.X < Math.Min( start.X, end.X ) || middle.X > Math.Max( start.X, end.X )
			|| middle.Y < Math.Min( start.Y, end.Y ) || middle.Y > Math.Max( start.Y, end.Y )
		) {
			throw new ArgumentException( "A polygon boundary must not backtrack along an edge." );
		}
	}

	private static void Validate(
		Edge edge
	) {
		GeometryLimits.Validate( edge.A, nameof( edge ) );
		GeometryLimits.Validate( edge.B, nameof( edge ) );
	}

	private static long Orientation(
		Point a, Point b, Point c
	) {
		return ( (long)b.X - a.X ) * ( c.Y - a.Y ) - ( (long)b.Y - a.Y ) * ( c.X - a.X );
	}

	private static void AddUnique(
		Span<Point> points, ref int count, Point point
	) {
		if( points[..count].Contains( point ) ) {
			return;
		}
		if( count == points.Length ) {
			throw new InvalidOperationException( "Convex intersection scratch capacity was exceeded." );
		}
		points[count++] = point;
	}

	private static bool PublishIntersections(
		ReadOnlySpan<Point> points, out IReadOnlyList<Point> intersections
	) {
		if( points.IsEmpty ) {
			intersections = [];
			return false;
		}
		intersections = Own( points );
		return true;
	}

	private static ImmutableArray<Point> Own(
		ReadOnlySpan<Point> points
	) {
		return ImmutableCollectionsMarshal.AsImmutableArray( points.ToArray() );
	}

	private static int ComparePoints( Point x, Point y ) {
		int result = x.X.CompareTo( y.X );
		return result == 0 ? x.Y.CompareTo( y.Y ) : result;
	}
}
