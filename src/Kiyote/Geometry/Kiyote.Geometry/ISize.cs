namespace Kiyote.Geometry;

public interface ISize: IEquatable<ISize> {
	int Width { get; }

	int Height { get; }
}
