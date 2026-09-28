namespace Kiyote.Geometry.Grids.Connectivity;

/// <summary>
/// A directed connection from a cell in one leaf to an adjacent cell in a
/// different leaf.  A symmetric connection appears twice, once from each side.
/// </summary>
/// <param name="Index">The combined-buffer index of the source cell.</param>
/// <param name="NeighbourIndex">The combined-buffer index of the connected cell.</param>
/// <param name="Direction">The direction from the source cell to the connected cell.</param>
public readonly record struct SeamLink(
	int Index,
	int NeighbourIndex,
	Direction Direction
);
