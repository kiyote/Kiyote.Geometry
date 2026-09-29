namespace Kiyote.Geometry.Topology;

/// <summary>
/// Maps a field of <typeparamref name="TCell"/> to layer values.  Implement as
/// a struct so calls are devirtualised and inlined.
/// </summary>
public interface IGridLayerBinding<TCell, T> {

	/// <summary>
	/// Reads the layer value from a source cell.
	/// </summary>
	T Extract(
		in TCell cell
	);

	/// <summary>
	/// Writes a layer value back into a source cell.
	/// </summary>
	void Commit(
		ref TCell cell,
		T value
	);
}
