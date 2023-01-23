namespace Kiyote.Geometry;

public interface IPolygon {

	IReadOnlyList<IPoint> Points { get; }

	IReadOnlyList<IPoint> Intersections( IReadOnlyList<IPoint> polygon );

	bool Contains( IPoint target );
}
