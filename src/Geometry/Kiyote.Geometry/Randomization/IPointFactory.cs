namespace Kiyote.Geometry.Randomization;

public interface IPointFactory {

	IReadOnlyList<IPoint> Fill( IBounds bounds, int distanceApart );

}
