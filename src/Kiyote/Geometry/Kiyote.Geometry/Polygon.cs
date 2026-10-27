using System.Collections.Immutable;
using System.Runtime.CompilerServices;
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

	internal ReadOnlySpan<Point> AsSpan() => _points.AsSpan();

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
		return ContainsConvex( _points.AsSpan(), _winding, point );
	}

	internal static bool ContainsConvex(
		ReadOnlySpan<Point> vertices, int winding, Point point
	) {
		if( vertices.IsEmpty ) {
			return false;
		}
		Point previous = vertices[^1];
		foreach( Point current in vertices ) {
			if( Orientation( previous, current, point ) * winding < 0 ) {
				return false;
			}
			previous = current;
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
		ValidateRoundedCandidates( points );
		return CreateRoundedHull( points );
	}

	internal static int WriteRoundedConvexHull(
		Span<Point> points, Span<Point> normalized
	) {
		ValidateRoundedCandidates( points );
		ArgumentOutOfRangeException.ThrowIfLessThan( normalized.Length, Math.Min( points.Length, MaximumVertices ), nameof( normalized ) );
		if( points.Length < 3 ) {
			return 0;
		}
		if( points.Length <= 4 && TryWriteTinyHull( points, normalized ) ) {
			return points.Length;
		}
		Span<Point> hull = stackalloc Point[2 * points.Length];
		int count = WriteSortedHull( points, hull );
		hull[..count].CopyTo( normalized );
		return count;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static void ValidateRoundedCandidates( ReadOnlySpan<Point> points ) {
		if( points.Length > 2 * MaximumVertices ) {
			throw new ArgumentException( "Generated vertices exceed the bounded convex-hull scratch capacity.", nameof( points ) );
		}
		foreach( Point point in points ) {
			GeometryLimits.Validate( point, nameof( points ) );
		}
	}

	internal static Polygon FromNormalizedVertices( ReadOnlySpan<Point> points ) => new Polygon( Own( points ) );

	private static Polygon CreateRoundedHull(
		Span<Point> candidates
	) {
		if( candidates.Length < 3 ) {
			return None;
		}
		if( candidates.Length <= 4 ) {
			Span<Point> ordered = stackalloc Point[4];
			if( TryWriteTinyHull( candidates, ordered ) ) {
				return FromNormalizedVertices( ordered[..candidates.Length] );
			}
		}
		Span<Point> hull = stackalloc Point[2 * candidates.Length];
		int count = WriteSortedHull( candidates, hull );
		return count == 0 ? None : FromNormalizedVertices( hull[..count] );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	private static bool TryWriteTinyHull( ReadOnlySpan<Point> candidates, Span<Point> ordered ) {
		int count = candidates.Length;
		int winding = Math.Sign( Orientation( candidates[0], candidates[1], candidates[2] ) );
		// Four triangle signs cover all edge/other-vertex half-planes for a quad.
		if( winding == 0 || (count == 4 && (
			Orientation( candidates[0], candidates[1], candidates[3] ) * winding <= 0
			|| Orientation( candidates[0], candidates[2], candidates[3] ) * winding <= 0
			|| Orientation( candidates[1], candidates[2], candidates[3] ) * winding <= 0
		)) ) {
			return false;
		}
		int start = 0;
		for( int i = 1; i < count; i++ ) {
			if( ComparePoints( candidates[i], candidates[start] ) < 0 ) {
				start = i;
			}
		}
		int index = start;
		for( int i = 0; i < count; i++ ) {
			ordered[i] = candidates[index];
			index += winding;
			if( index == count ) {
				index = 0;
			} else if( index < 0 ) {
				index = count - 1;
			}
		}
		return true;
	}

	private static int WriteSortedHull( Span<Point> candidates, Span<Point> hull ) {
		candidates.Sort( ComparePoints );
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
			return 0;
		}
		if( hullCount > MaximumVertices ) {
			throw new InvalidOperationException( $"The generated polygon exceeds {MaximumVertices} normalized vertices." );
		}
		return hullCount;
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
		BoundaryNormalizer normalizer = new BoundaryNormalizer( normalized, nameof( points ) );
		for( int i = 0; i < points.Count; i++ ) {
			normalizer.Add( points[i] );
		}
		return Own( normalizer.Complete() );
	}

	internal ref struct BoundaryNormalizer {
		private readonly Span<Point> _normalized;
		private readonly string _parameterName;
		private int _count;

		internal BoundaryNormalizer( Span<Point> normalized, string parameterName ) {
			_normalized = normalized;
			_parameterName = parameterName;
			_count = 0;
		}

		internal void Add( Point point ) {
			GeometryLimits.Validate( point, _parameterName );
			if( _count > 0 && _normalized[_count - 1] == point ) {
				return;
			}
			while( _count >= 2 && Orientation( _normalized[_count - 2], _normalized[_count - 1], point ) == 0 ) {
				RequireBetween( _normalized[_count - 2], _normalized[_count - 1], point );
				_count--;
			}
			if( _count == _normalized.Length ) {
				throw new ArgumentException( $"Polygon exceeds {MaximumVertices} normalized vertices.", _parameterName );
			}
			_normalized[_count++] = point;
		}

		internal ReadOnlySpan<Point> Complete() {
			if( _count == 0 ) {
				return ReadOnlySpan<Point>.Empty;
			}
			if( _count > 1 && _normalized[_count - 1] == _normalized[0] ) {
				_count--;
			}
			while( _count >= 3 ) {
				if( Orientation( _normalized[_count - 2], _normalized[_count - 1], _normalized[0] ) == 0 ) {
					RequireBetween( _normalized[_count - 2], _normalized[_count - 1], _normalized[0] );
					_count--;
				} else if( Orientation( _normalized[_count - 1], _normalized[0], _normalized[1] ) == 0 ) {
					RequireBetween( _normalized[_count - 1], _normalized[0], _normalized[1] );
					_normalized.Slice( 1, --_count ).CopyTo( _normalized );
				} else {
					break;
				}
			}
			if( _count < 3 ) {
				throw new ArgumentException( "A nonempty polygon must have positive area and at least three distinct corners.", _parameterName );
			}
			if( _count > MaximumVertices ) {
				throw new ArgumentException( $"Polygon exceeds {MaximumVertices} normalized vertices.", _parameterName );
			}
			int winding = Math.Sign( Orientation( _normalized[0], _normalized[1], _normalized[2] ) );
			for( int i = 0; i < _count; i++ ) {
				int next = ( i + 1 ) % _count;
				for( int j = 0; j < _count; j++ ) {
					if( j != i && j != next
						&& Orientation( _normalized[i], _normalized[next], _normalized[j] ) * winding <= 0
					) {
						throw new ArgumentException( "Vertices must trace a convex boundary without self-intersection or repeated corners.", _parameterName );
					}
				}
			}
			return _normalized[.._count];
		}
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
		return (( (long)b.X - a.X ) * ( c.Y - a.Y )) - (( (long)b.Y - a.Y ) * ( c.X - a.X ));
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
