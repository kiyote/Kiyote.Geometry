namespace Kiyote.Geometry.Randomization;

public interface IPointFactory {

	IReadOnlyList<Point> Fill(
		ISize size,
		int distanceApart
	);

}
