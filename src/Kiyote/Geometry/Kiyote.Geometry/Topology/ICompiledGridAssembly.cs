namespace Kiyote.Geometry.Topology;

/// <summary>
/// A snapshot of an <see cref="IGridAssembly{TCell}"/> compiled into fixed-size
/// chunks.  Layer values are copies; sources are only written by
/// <see cref="Commit"/>.  At most one compilation of an assembly should be
/// live at a time; the caller is responsible for disposing it before
/// compiling again.
/// </summary>
/// <remarks>
/// This is a consumption interface for the closed built-in implementation family.
/// Layer management, commit, and connectivity operations reject stale or disposed
/// compilations. Disposal remains idempotent and snapshot inspection is allowed. Commit before
/// attaching or detaching, then dispose and recompile. Retained layers and spans
/// are detached snapshots after invalidation; they no longer receive cell edits.
/// </remarks>
public interface ICompiledGridAssembly<TCell> : IDisposable {

	IGridAssembly<TCell> Assembly { get; }

	/// <summary>
	/// The assembly version this space was compiled from.
	/// </summary>
	int Version { get; }

	/// <summary>
	/// True when the assembly has been attached to or detached from since
	/// compilation.  Per-cell edits do not make a space stale; they are applied
	/// immediately. A stale space cannot be committed or used for further operations.
	/// </summary>
	bool IsStale { get; }

	/// <summary>
	/// The chunk layout of the whole compiled assembly: every allocated chunk,
	/// its index, neighbours and cell occupancy.  Holds no cell data.  Every
	/// layer in <see cref="Layers"/> stores its values over this layout, so a
	/// chunk index and local index identify the same cell in all layers.
	/// This view exposes no topology mutation: occupancy is changed through
	/// <see cref="IGridAssembly{TCell}.TryAddCell"/> and
	/// <see cref="IGridAssembly{TCell}.TryRemoveCell"/>.  Chunks are never
	/// removed while the compiled assembly is live, so chunk indices are stable.
	/// </summary>
	GridLayout ChunkLayout { get; }

	/// <summary>
	/// Every source-to-chunk mapping. The span is valid until the next cell edit.
	/// </summary>
	ReadOnlySpan<SourceRun> SourceRuns { get; }

	/// <summary>
	/// The assembly's seams resolved to chunk storage. The span is valid until
	/// the next cell edit.
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
	/// <remarks>
	/// For stencil-style passes that read neighbouring cells across chunk
	/// borders, copy each chunk with a border into scratch using
	/// <see cref="IGridLayer{T}.CopyPadded"/>.
	/// </remarks>
	/// <exception cref="ObjectDisposedException">
	/// This compiled assembly has been disposed.
	/// </exception>
	IGridLayer<T> Bind<T, TBinding>(
		TBinding binding
	) where T : unmanaged
		where TBinding : struct, IGridLayerBinding<TCell, T>;

	/// <summary>
	/// Creates a scratch layer over the same chunk layout that is not bound to
	/// the sources.  It starts zeroed, grows with cell edits and has removed
	/// cells reset to <see langword="default"/>, like bound layers, but is never
	/// written by <see cref="Commit"/>.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// This compiled assembly has been disposed.
	/// </exception>
	IGridLayer<T> CreateLayer<T>() where T : unmanaged;

	/// <summary>
	/// Stops maintaining a layer owned by this assembly and removes it from
	/// <see cref="Layers"/>.  A removed bound layer is no longer committed.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if the layer belonged to this assembly.
	/// </returns>
	bool RemoveLayer(
		IGridLayer layer
	);

	/// <summary>
	/// Exchanges the values and dirty flags of two layers owned by this
	/// assembly in O(1).  Bindings stay with their layer objects, so after a
	/// swap <see cref="Commit"/> stores whatever values the bound layer now
	/// holds, for the chunks marked dirty in the flags it now holds.  Spans
	/// obtained from either layer before the swap refer to the other layer
	/// afterwards.
	/// </summary>
	/// <exception cref="ArgumentException">
	/// Either layer does not belong to this assembly, or they are the same layer.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// This compiled assembly has been disposed.
	/// </exception>
	void Swap<T>(
		IGridLayer<T> a,
		IGridLayer<T> b
	) where T : unmanaged;

	/// <summary>
	/// Creates a layer holding, for each occupied cell, the directions in
	/// which the neighbouring cell is unoccupied: off-grid, a hole, a gap
	/// between placements or a missing chunk.  A diagonal is flagged whenever
	/// the diagonal cell itself is unoccupied, regardless of the two cells
	/// beside it.  Unoccupied cells hold <see cref="Direction.None"/>.  The
	/// result depends only on occupancy, not on any connectivity strategy.
	/// </summary>
	/// <remarks>
	/// The layer is a snapshot.  Like any other layer it grows with cell
	/// edits and removed cells are reset, but the directions of the remaining
	/// cells are not recalculated.  After changing the topology, remove the
	/// layer with <see cref="RemoveLayer"/> and create it again.
	/// </remarks>
	/// <exception cref="ObjectDisposedException">
	/// This compiled assembly has been disposed.
	/// </exception>
	IGridLayer<Direction> CreateVacuumLayer();

	/// <summary>
	/// Writes the dirty chunks of every bound layer back to the
	/// sources, then clears their dirty flags.
	/// </summary>
	void Commit();
}
