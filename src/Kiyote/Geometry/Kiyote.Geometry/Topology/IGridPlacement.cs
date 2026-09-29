namespace Kiyote.Geometry.Topology;

/// <summary>
/// An <see cref="IGridSource{TCell}"/> positioned within an
/// <see cref="IGridAssembly{TCell}"/>.
/// </summary>
public interface IGridPlacement<TCell> {

	PlacementId Id { get; }

	IGridSource<TCell> Source { get; }

	/// <summary>
	/// The assembly-space column of the source's origin.
	/// </summary>
	int Column { get; }

	/// <summary>
	/// The assembly-space row of the source's origin.
	/// </summary>
	int Row { get; }

	/// <summary>
	/// The bounds of the source in assembly space.
	/// </summary>
	Rect Bounds { get; }

	/// <summary>
	/// The occupied runs of the supplied assembly-space row, in assembly space.
	/// Empty when the row lies outside <see cref="Bounds"/>.
	/// </summary>
	ReadOnlySpan<CellRun> GetOccupiedRuns(
		int row
	);
}
