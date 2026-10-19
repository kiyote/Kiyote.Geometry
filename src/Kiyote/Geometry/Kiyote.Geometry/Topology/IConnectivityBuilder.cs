namespace Kiyote.Geometry.Topology;

public interface IConnectivityBuilder {

	/// <summary>
	/// Produces a derived layer holding the connected directions of every
	/// occupied cell.  Neighbours in other placements are exactly the seam
	/// contacts and are evaluated with <c>isSeam</c> set.  The result is a
	/// snapshot; removed cells are reset, but other cells are not re-evaluated,
	/// by later cell edits.
	/// Use <see cref="Update"/> to recalculate changed cells.
	/// </summary>
	/// <param name="compiledGridAssembly">The compiled assembly to evaluate.</param>
	/// <param name="strategy">Decides whether each pair of adjacent cells is connected.</param>
	/// <exception cref="InvalidOperationException">
	/// <paramref name="compiledGridAssembly"/> is stale.
	/// </exception>
	/// <exception cref="ObjectDisposedException">The compilation has been disposed.</exception>
	/// <exception cref="ArgumentException">The compilation is not a built-in implementation.</exception>
	IGridLayer<Direction> Build<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		TStrategy strategy
	) where TStrategy : struct, IConnectivityStrategy<TCell>;

	/// <summary>
	/// Recalculates the connected directions of the cells in
	/// <paramref name="area"/> and of every cell within one cell of it.  Call
	/// this after changing source cells in a way that affects the strategy.
	/// Changes must already be in the sources;
	/// commit any bound layers first.  Attaching or detaching makes the
	/// compiled assembly stale; recompile and build a new layer instead.
	/// An empty area does nothing. Neighbours outside signed 32-bit coordinates
	/// are absent; padded scratch must fit the runtime's array capacity.
	/// </summary>
	/// <param name="compiledGridAssembly">The compiled assembly the layer was built from.</param>
	/// <param name="layer">A layer returned by <see cref="Build"/> for <paramref name="compiledGridAssembly"/>.</param>
	/// <param name="strategy">The strategy used to build the layer.</param>
	/// <param name="area">The assembly-space cells that changed.</param>
	/// <exception cref="ArgumentException">
	/// <paramref name="layer"/> does not belong to <paramref name="compiledGridAssembly"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// <paramref name="compiledGridAssembly"/> is stale.
	/// </exception>
	/// <exception cref="ObjectDisposedException">The compilation has been disposed.</exception>
	/// <exception cref="ArgumentOutOfRangeException">The padded area exceeds array capacity.</exception>
	void Update<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		IGridLayer<Direction> layer,
		TStrategy strategy,
		Rect area
	) where TStrategy : struct, IConnectivityStrategy<TCell>;
}
