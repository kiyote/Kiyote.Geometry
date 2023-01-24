namespace Kiyote.Geometry.Randomization;

public interface IPointFactory {

	IReadOnlyList<Point> Fill( Bounds bounds, int distanceApart );

}
