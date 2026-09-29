namespace Kiyote.Geometry.Topology;

/// <summary>
/// A horizontal, contiguous strip of cells.
/// </summary>
/// <param name="Column">The inclusive left-most column of the run.</param>
/// <param name="Row">The row of the run.</param>
/// <param name="Length">The number of cells in the run.  Always greater than zero.</param>
public readonly record struct CellRun(
	int Column,
	int Row,
	int Length
) {
	/// <summary>
	/// The inclusive right-most column of the run.
	/// </summary>
	public int LastColumn => Column + Length - 1;
}
