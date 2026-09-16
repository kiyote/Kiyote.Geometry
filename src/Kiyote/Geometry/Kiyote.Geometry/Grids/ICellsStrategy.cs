namespace Kiyote.Geometry.Grids;

public interface ICellsStrategy<TCell, TResult> {

	TResult Evaluate(
		GridCell<TCell> source,
		GridCell<TCell> destination
	);

}
