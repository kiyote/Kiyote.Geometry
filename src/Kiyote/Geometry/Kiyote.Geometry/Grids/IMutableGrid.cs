namespace Kiyote.Geometry.Grids;

public interface IMutableGrid<T>: IGrid<T> {

	new T? this[int column, int row] { get; set; }
}
