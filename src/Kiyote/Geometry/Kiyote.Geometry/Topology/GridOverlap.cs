namespace Kiyote.Geometry.Topology;

/// <summary>
/// Describes the cells shared between a rejected attachment and an existing
/// placement.
/// </summary>
/// <param name="Existing">The placement that was collided with.</param>
/// <param name="Bounds">The bounding rectangle of the overlapping cells, in assembly space.</param>
/// <param name="Cells">The exact overlapping cells, in assembly space.</param>
public readonly record struct GridOverlap(
	PlacementId Existing,
	Rect Bounds,
	IReadOnlyList<CellRun> Cells
);
