namespace Kiyote.Geometry.Grids;

public interface ICellStrategy<TCell, TResult> {

	TResult Evaluate(
		GridCell<TCell> cell
	);

}
