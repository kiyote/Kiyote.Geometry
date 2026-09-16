namespace Kiyote.Geometry.Grids;

public interface ICallbackStrategy<TCell, TValue> {

	void Callback(
		GridCell<TCell> cell,
		TValue value
	);

}
