namespace Kiyote.Geometry.Topology;

/// <summary>
/// A snapshot of an <see cref="IGridAssembly{TCell}"/> compiled into fixed-size
/// chunks.  Layer values are copies; sources are only written by
/// <see cref="Commit"/>.  At most one compilation of an assembly should be
/// live at a time; the caller is responsible for disposing it before
/// compiling again.
/// </summary>
public interface ICompiledGridAssembly<TCell> : IDisposable {

	IGridAssembly<TCell> Assembly { get; }

	/// <summary>
	/// The assembly version this space was compiled from.
	/// </summary>
	int Version { get; }

	/// <summary>
	/// True when the assembly has been attached to or detached from since
	/// compilation.  Per-cell edits do not make a space stale; they are applied
	/// immediately.  A stale space can still be committed.
	/// </summary>
	bool IsStale { get; }

	/// <summary>
	/// The chunk layout of the whole compiled assembly: every allocated chunk
	/// slot, its origin, neighbours and cell occupancy.  Holds no cell data.
	/// Every layer in <see cref="Layers"/> stores its values according to this
	/// layout, so a slot and local index identify the same cell in all layers.
	/// </summary>
	IGridChunkLayout ChunkLayout { get; }

	/// <summary>
	/// Every source-to-chunk mapping.
	/// </summary>
	ReadOnlySpan<SourceRun> SourceRuns { get; }

	/// <summary>
	/// The assembly's seams resolved to chunk storage.
	/// </summary>
	ReadOnlySpan<ChunkSeamLink> SeamLinks { get; }

	IReadOnlyList<IGridLayer> Layers { get; }

	/// <summary>
	/// Creates a layer holding one <typeparamref name="T"/> per occupied cell,
	/// fills it from the sources and adds it to <see cref="Layers"/>.  The
	/// layer's values are written back to the sources by <see cref="Commit"/>.
	/// </summary>
	/// <typeparam name="T">The type of value stored in the layer.</typeparam>
	/// <typeparam name="TBinding">
	/// The strategy for reading a <typeparamref name="T"/> from a source cell
	/// and writing it back.  Implement as a struct so calls are devirtualised
	/// and inlined.
	/// </typeparam>
	/// <param name="binding">
	/// The strategy instance.  <see cref="IGridLayerBinding{TCell, T}.Extract"/>
	/// is called for every cell now and for cells added later;
	/// <see cref="IGridLayerBinding{TCell, T}.Commit"/> is called for cells in
	/// dirty chunks when <see cref="Commit"/> runs.
	/// </param>
	/// <param name="halo">
	/// The width of the border, in cells, stored around each chunk.  Must be 0
	/// or 1.  With a halo of 1, each chunk also holds a copy of the adjacent
	/// cells from its neighbouring chunks.  Code that reads a cell's neighbours
	/// can then stay within a single chunk's storage, even at chunk edges.
	/// Halo cells with no neighbouring chunk hold <see langword="default"/>.
	/// The halo is filled when the layer is bound.  After writing values, call
	/// <see cref="IGridLayer{T}.ExchangeHalos"/> to refresh it.  Use 1 for
	/// stencil-style passes over <see cref="IGridLayer{T}.GetChunk"/> spans,
	/// such as diffusion, blurring, edge detection or neighbour counts, where
	/// each cell reads its 8 neighbours.  Use 0 when cells are processed
	/// without looking at their neighbours, or when neighbours are only read
	/// through the layer's indexer; this saves memory and exchange time.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="halo"/> is not 0 or 1.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// This compiled assembly has been disposed.
	/// </exception>
	IGridLayer<T> Bind<T, TBinding>(
		TBinding binding,
		int halo
	) where TBinding : struct, IGridLayerBinding<TCell, T>;

	/// <summary>
	/// Writes the dirty chunks of every bound layer back to the
	/// sources, then clears their dirty flags.
	/// </summary>
	void Commit();
}
