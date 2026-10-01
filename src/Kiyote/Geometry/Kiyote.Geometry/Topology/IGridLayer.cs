namespace Kiyote.Geometry.Topology;

/// <summary>
/// Type-independent view of a layer.
/// </summary>
public interface IGridLayer {

	IGridChunkLayout Space { get; }

	/// <summary>
	/// The number of halo cells surrounding each chunk, from 0 to
	/// <see cref="IGridChunkLayout.ChunkSize"/>.  A halo of <c>h</c> holds
	/// copies of the cells within <c>h</c> of the chunk edge from neighbouring
	/// chunks, so a pass over a chunk's storage can read neighbours up to
	/// <c>h</c> cells away without leaving the chunk.  It is useful for
	/// stencil-style work such as diffusion, blurring, neighbour counts or
	/// interpolated sampling.  Halo cells are only current after
	/// <see cref="IGridLayer{T}.ExchangeHalos"/>.
	/// </summary>
	int Halo { get; }

	/// <summary>
	/// The number of elements between the starts of consecutive rows of a
	/// chunk, <c>ChunkSize + (2 * Halo)</c>.
	/// </summary>
	int Stride { get; }

	/// <summary>
	/// The number of elements occupied by each slot, including halo cells,
	/// <c>Stride * Stride</c>.  The layout is guaranteed: the cell at
	/// chunk-local <c>( column, row )</c> of a slot is at index
	/// <c>slot * ChunkLength + ( row + Halo ) * Stride + ( column + Halo )</c>,
	/// with <c>column</c> and <c>row</c> ranging from <c>-Halo</c> to
	/// <c>ChunkSize + Halo - 1</c>.  Layers created with the same halo from
	/// the same compiled assembly share the same layout.
	/// </summary>
	int ChunkLength { get; }

	/// <summary>
	/// Returns the index into the layer's cells of a chunk-local position.
	/// <paramref name="localColumn"/> and <paramref name="localRow"/> may lie
	/// in the halo, from <c>-Halo</c> to <c>ChunkSize + Halo - 1</c>.
	/// </summary>
	int IndexOf(
		int slot,
		int localColumn,
		int localRow
	);

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
/// <remarks>
/// The library never makes an unoccupied cell, or a halo cell facing a
/// missing chunk, hold anything other than <see langword="default"/>: new
/// layers start zeroed, unoccupied cells are never loaded, removed cells are
/// reset and halo exchange clears regions with no neighbour.  Values written
/// directly into such cells through <see cref="Cells"/>,
/// <see cref="GetChunk"/>, <see cref="GetRow"/> or
/// <see cref="GetChunkReference"/> are the caller's responsibility.
/// </remarks>
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
	/// Returns a reference to the first element, including halo cells, of a
	/// single chunk's storage.  The reference is invalidated by
	/// <see cref="ICompiledGridAssembly{TCell}.Swap"/> and by cell edits that
	/// allocate chunks, so fetch it again after either.
	/// </summary>
	ref T GetChunkReference(
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

	/// <summary>
	/// Refreshes the halo of a single slot from its neighbours.  Halo cells
	/// with no neighbouring chunk are set to <see langword="default"/>.
	/// No-op when <see cref="IGridLayer.Halo"/> is 0.
	/// </summary>
	void ExchangeHalo(
		int slot
	);
}
