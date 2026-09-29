namespace Kiyote.Geometry.Topology;

/// <summary>
/// The chunked geometry of a compiled space.  Holds no cell data; layers lay
/// their storage out according to the slots defined here.
/// </summary>
public interface IGridChunkLayout {

	/// <summary>
	/// The width and height of every chunk.  Always a power of two.
	/// </summary>
	int ChunkSize { get; }

	/// <summary>
	/// <c>log2(ChunkSize)</c>, for converting between cell and chunk coordinates.
	/// </summary>
	int ChunkShift { get; }

	/// <summary>
	/// The number of allocated chunk slots.  Slots are numbered <c>[0, SlotCount)</c>.
	/// </summary>
	int SlotCount { get; }

	/// <summary>
	/// The bounds of all occupied cells in assembly space, or <see langword="null"/>
	/// when the space is empty.
	/// </summary>
	Rect? Bounds { get; }

	ChunkState GetState(
		int slot
	);

	/// <summary>
	/// The assembly-space coordinates of the top-left cell of the chunk.
	/// </summary>
	Point GetOrigin(
		int slot
	);

	/// <summary>
	/// One bit per cell in row-major order, set when the cell is occupied.
	/// </summary>
	ReadOnlySpan<ulong> GetValidityMask(
		int slot
	);

	/// <summary>
	/// Returns the neighbouring chunk slot in <paramref name="direction"/>, or
	/// -1 when no chunk is allocated there.
	/// </summary>
	int GetNeighbour(
		int slot,
		Direction direction
	);

	bool TryGetSlot(
		int chunkColumn,
		int chunkRow,
		out int slot
	);

	/// <summary>
	/// Resolves an assembly-space cell to its chunk slot and row-major index
	/// within the chunk.  Returns false for unoccupied cells.
	/// </summary>
	bool TryGetCell(
		int column,
		int row,
		out int slot,
		out int localIndex
	);
}
