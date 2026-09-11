namespace Kiyote.Geometry.Grids;

public interface IImmutableGrid<T> {

	T? this[int column, int row] { get; }
}
