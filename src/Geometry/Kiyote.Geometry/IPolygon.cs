namespace Kiyote.Geometry;

public interface IPolygon {

	IReadOnlyList<Point> Points { get; }

	IReadOnlyList<Point> Intersections( IReadOnlyList<Point> polygon );

	bool Contains( Point target );

	bool TryFindIntersection( IPolygon polygon, out IPolygon clipped );
}
