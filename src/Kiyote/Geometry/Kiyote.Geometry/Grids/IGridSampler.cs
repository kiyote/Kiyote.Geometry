namespace Kiyote.Geometry.Grids;

public interface IGridSampler<TCoordinate, TValue> {

	TValue Sample(
		IGrid<TValue> grid,
		TCoordinate column,
		TCoordinate row
	);

}
