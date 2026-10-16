namespace Kiyote.Geometry.Topology;

/// <summary>
/// Storage maintenance a compiled space performs on every layer it owns.
/// </summary>
internal interface IGridLayerStorage : IGridLayer {

	/// <summary>
	/// Resets the value of a cell to <see langword="default"/>.
	/// </summary>
	void Reset(
		int chunkIndex,
		int localIndex
	);
}
