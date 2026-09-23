namespace Kiyote.Geometry.Grids.Connectivity;

public interface IConnectivityGrid<TCell> : IGrid<Direction> {

	/// <summary>
	/// Attaches a data grid whose connectivity will be tracked by this grid.  A
	/// backing cache of <see cref="Direction"/> values, sized to match
	/// <paramref name="grid"/>, is allocated and exposed through this grid's
	/// <see cref="IGrid{Direction}"/> surface at the supplied location.
	/// </summary>
	bool TryAttach(
		IGrid<TCell> grid,
		int column,
		int row
	);

	/// <summary>
	/// Detaches a previously attached data grid, discarding its connectivity cache.
	/// </summary>
	bool TryDetach(
		IGrid<TCell> grid
	);

	/// <summary>
	/// Re-evaluates connectivity for every attached grid, using
	/// <paramref name="connectivity"/> to determine whether each pair of
	/// neighbouring cells is connected.
	/// </summary>
	/// <returns><see langword="true"/> if any cached value changed.</returns>
	bool UpdateConnectivity<TConnectivityStrategy>(
		TConnectivityStrategy connectivity
	) where TConnectivityStrategy : IConnectivityStrategy<TCell>;

	/// <summary>
	/// Re-evaluates connectivity within the supplied bounds only, for every
	/// attached grid.
	/// </summary>
	/// <returns><see langword="true"/> if any cached value changed.</returns>
	bool UpdateConnectivity<TConnectivityStrategy>(
		TConnectivityStrategy connectivity,
		int startColumn,
		int startRow,
		int endColumn,
		int endRow
	) where TConnectivityStrategy : IConnectivityStrategy<TCell>;

	/// <summary>
	/// Evaluates whether two adjacent cells are connected using the supplied
	/// strategy.  Returns <see langword="false"/> when the two locations are not
	/// adjacent, or when they are not both covered by the same attached grid.
	/// </summary>
	bool IsConnectedTo<TConnectivityStrategy>(
		TConnectivityStrategy connectivity,
		int sourceColumn,
		int sourceRow,
		int destinationColumn,
		int destinationRow
	) where TConnectivityStrategy : IConnectivityStrategy<TCell>;

}
