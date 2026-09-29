namespace Kiyote.Geometry.Topology;

/// <summary>
/// The outcome of <see cref="IGridAssembly{TCell}.TryAttach"/>.
/// </summary>
/// <param name="Placement">
/// The new placement when the attach succeeded, otherwise <see cref="PlacementId.None"/>.
/// </param>
/// <param name="Overlaps">
/// The overlaps that caused the attach to be rejected.  Empty when the attach succeeded.
/// </param>
/// <param name="Seams">
/// The seams formed with existing placements.  Empty when the attach was rejected.
/// </param>
public readonly record struct AttachResult(
	PlacementId Placement,
	IReadOnlyList<GridOverlap> Overlaps,
	IReadOnlyList<GridSeam> Seams
) {
	public bool Succeeded => !Placement.IsNone;
}
