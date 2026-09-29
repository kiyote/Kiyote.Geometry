namespace Kiyote.Geometry.Topology;

/// <summary>
/// Describes how many cells of a chunk are occupied.
/// </summary>
public enum ChunkState {
	/// <summary>No cells are occupied.  Such chunks are not allocated.</summary>
	Empty = 0,
	/// <summary>Some cells are occupied.  Kernels must respect the validity mask.</summary>
	Partial = 1,
	/// <summary>Every cell is occupied.  Kernels may ignore the validity mask.</summary>
	Full = 2
}
