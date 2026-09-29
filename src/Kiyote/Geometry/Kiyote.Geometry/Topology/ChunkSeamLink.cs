namespace Kiyote.Geometry.Topology;

/// <summary>
/// A <see cref="GridContact"/> resolved to chunk storage.
/// </summary>
/// <param name="Slot">The chunk slot of the first cell.</param>
/// <param name="LocalIndex">The row-major index of the first cell within its chunk.</param>
/// <param name="NeighbourSlot">The chunk slot of the neighbouring cell.</param>
/// <param name="NeighbourLocalIndex">The row-major index of the neighbouring cell within its chunk.</param>
/// <param name="Direction">The direction of the neighbour relative to the first cell.</param>
public readonly record struct ChunkSeamLink(
	int Slot,
	int LocalIndex,
	int NeighbourSlot,
	int NeighbourLocalIndex,
	Direction Direction
);
