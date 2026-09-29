namespace Kiyote.Geometry.Topology;

/// <summary>
/// Type-independent view of a layer.
/// </summary>
public interface IGridLayer {

	IGridChunkLayout Space { get; }

	/// <summary>
	/// The number of halo cells surrounding each chunk.  Either 0 or 1.  A
	/// halo of 1 holds copies of the adjacent cells from neighbouring chunks,
	/// so a pass over a chunk's storage can read each cell's 8 neighbours
	/// without leaving the chunk.  It is useful for stencil-style work such as
	/// diffusion, blurring or neighbour counts.  Halo cells are only current
	/// after <see cref="IGridLayer{T}.ExchangeHalos"/>.
	/// </summary>
	int Halo { get; }

	/// <summary>
	/// The number of elements between the starts of consecutive rows of a
	/// chunk, <c>ChunkSize + (2 * Halo)</c>.
	/// </summary>
	int Stride { get; }

	bool IsDirty(
		int slot
	);

	void MarkDirty(
		int slot
	);

	void ClearDirty();
}

/// <summary>
/// Typed, chunked cell data laid out over an <see cref="IGridChunkLayout"/>.
/// </summary>
public interface IGridLayer<T> : IGridLayer {

	/// <summary>
	/// The entire backing store.  Suitable for element-wise operations that
	/// ignore neighbours.
	/// </summary>
	Span<T> Cells { get; }

	/// <summary>
	/// The storage for a single chunk, including halo cells, in row-major
	/// order with <see cref="IGridLayer.Stride"/>.
	/// </summary>
	Span<T> GetChunk(
		int slot
	);

	/// <summary>
	/// The interior cells of a single chunk row, excluding halo cells.
	/// </summary>
	Span<T> GetRow(
		int slot,
		int row
	);

	/// <summary>
	/// Returns a reference to the value at the supplied assembly-space cell.
	/// Intended for convenience rather than hot loops.
	/// </summary>
	ref T this[int column, int row] { get; }

	/// <summary>
	/// Copies the edge cells of each chunk into the halos of its neighbours.
	/// No-op when <see cref="IGridLayer.Halo"/> is 0.
	/// </summary>
	void ExchangeHalos();
}
