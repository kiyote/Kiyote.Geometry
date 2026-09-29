namespace Kiyote.Geometry.Topology;

/// <summary>
/// Storage maintenance a compiled space performs on every layer it owns.
/// </summary>
internal interface IGridLayerStorage : IGridLayer {

	/// <summary>
	/// Grows storage to match the current slot count of the space.
	/// </summary>
	void EnsureSlots();

	/// <summary>
	/// Resets the value of an interior cell to <see langword="default"/>.
	/// </summary>
	void Reset(
		int slot,
		int localIndex
	);
}
