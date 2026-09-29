namespace Kiyote.Geometry.Topology;

public interface IConnectivityBuilder {

	/// <summary>
	/// Produces a derived layer holding the connected directions of every
	/// occupied cell.  Neighbours in other placements are exactly the seam
	/// contacts and are evaluated with <c>isSeam</c> set.  The result is a
	/// snapshot; it is resized, but not re-evaluated, by later cell edits.
	/// Use <see cref="Update"/> to recalculate changed cells.
	/// </summary>
	/// <param name="compiledGridAssembly">The compiled assembly to evaluate.</param>
	/// <param name="strategy">Decides whether each pair of adjacent cells is connected.</param>
	/// <param name="halo">
	/// The width of the border, in cells, stored around each chunk of the
	/// result.  Must be 0 or 1.  The halo does not affect the directions
	/// calculated.  Use 1 when a later pass over chunk storage needs the
	/// neighbouring cells' directions, such as a flow field or region
	/// labelling that checks whether a neighbour links back.  Use 0 when the
	/// layer is only read cell by cell, such as by a pathfinder expanding a
	/// node's own directions.  <see cref="Update"/> refreshes the halo.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// <paramref name="compiledGridAssembly"/> is stale.
	/// </exception>
	IGridLayer<Direction> Build<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		TStrategy strategy,
		int halo
	) where TStrategy : struct, IConnectivityStrategy<TCell>;

	/// <summary>
	/// Recalculates the connected directions of the cells in
	/// <paramref name="area"/> and of every cell within one cell of it, then
	/// refreshes the layer's halo.  Call this after changing source cells in a
	/// way that affects the strategy.  Changes must already be in the sources;
	/// commit any bound layers first.  Attaching or detaching makes the
	/// compiled assembly stale; recompile and build a new layer instead.
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
	void Update<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		IGridLayer<Direction> layer,
		TStrategy strategy,
		Rect area
	) where TStrategy : struct, IConnectivityStrategy<TCell>;
}
