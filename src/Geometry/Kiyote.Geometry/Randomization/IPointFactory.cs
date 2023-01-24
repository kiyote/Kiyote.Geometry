namespace Kiyote.Geometry.Randomization;

public interface IPointFactory {

	IReadOnlyList<Point> Fill( IBounds bounds, int distanceApart );

}
