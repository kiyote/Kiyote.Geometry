namespace Kiyote.Geometry.Topology;

/// <summary>
/// A cell presented to an <see cref="IConnectivityStrategy{TCell}"/>.
/// </summary>
/// <param name="Column">The assembly-space column.</param>
/// <param name="Row">The assembly-space row.</param>
/// <param name="Placement">The owning placement, or <see cref="PlacementId.None"/> when unoccupied.</param>
/// <param name="Cell">The source cell value, or <see langword="default"/> when unoccupied.</param>
public readonly record struct TopologyCell<TCell>(
	int Column,
	int Row,
	PlacementId Placement,
	TCell? Cell
) {
	public bool IsOccupied => !Placement.IsNone;
}
