namespace Kiyote.Geometry.Topology;

/// <summary>
/// Receives per-cell edits from a <see cref="GridAssembly{TCell}"/> so they can
/// be applied immediately.  Coordinates are in assembly space.
/// </summary>
internal interface IGridAssemblyObserver {

	/// <param name="placement">The placement that gained the cell.</param>
	/// <param name="column">The assembly-space column.</param>
	/// <param name="row">The assembly-space row.</param>
	/// <param name="contacts">The seam contacts the new cell formed.</param>
	void OnCellAdded(
		PlacementId placement,
		int column,
		int row,
		ReadOnlySpan<GridContact> contacts
	);

	void OnCellRemoved(
		PlacementId placement,
		int column,
		int row
	);
}
