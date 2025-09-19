namespace Kiyote.Geometry.Randomization;

public interface IPointFactory {

	IReadOnlyList<Point> Fill(
		ISize size,
		int distanceApart		
	);

	IReadOnlyList<Point> Fill(
		Point size,
		int distanceApart
	);

	IReadOnlyList<Point> Fill(
		int width,
		int height,
		int distanceApart
	);

	IReadOnlyList<Point> Fill(
		ISize size,
		int distanceApart,
		bool clipToBounds
	);

	IReadOnlyList<Point> Fill(
		Point size,
		int distanceApart,
		bool clipToBounds
	);

	IReadOnlyList<Point> Fill(
		int width,
		int height,
		int distanceApart,
		bool clipToBounds
	);

}
