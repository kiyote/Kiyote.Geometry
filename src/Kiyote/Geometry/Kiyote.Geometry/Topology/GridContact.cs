namespace Kiyote.Geometry.Topology;

/// <summary>
/// A pair of occupied cells, belonging to different placements, that are
/// neighbours of one another.  Coordinates are in assembly space.
/// </summary>
/// <param name="Column">The column of the cell in the first placement.</param>
/// <param name="Row">The row of the cell in the first placement.</param>
/// <param name="NeighbourColumn">The column of the neighbouring cell in the second placement.</param>
/// <param name="NeighbourRow">The row of the neighbouring cell in the second placement.</param>
/// <param name="Direction">
/// The direction of the neighbouring cell relative to the first cell.  May be
/// diagonal.
/// </param>
public readonly record struct GridContact(
	int Column,
	int Row,
	int NeighbourColumn,
	int NeighbourRow,
	Direction Direction
);
