namespace Kiyote.Geometry;

internal sealed record Polygon( IReadOnlyList<IPoint> Points ) : IPolygon {

	IReadOnlyList<IPoint> IPolygon.Intersections(
		IReadOnlyList<IPoint> polygon
	) {
		throw new InvalidOperationException();
	}
}
