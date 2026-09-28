namespace Kiyote.Geometry.Grids.Connectivity;

/// <summary>
/// A data grid attached to an <see cref="IConnectivityGrid{TCell}"/>, as captured
/// in a <see cref="GridTopology{TCell}"/>.
/// </summary>
/// <param name="Grid">The attached data grid.</param>
/// <param name="Column">The inclusive left edge of the leaf in connectivity space.</param>
/// <param name="Row">The inclusive top edge of the leaf in connectivity space.</param>
/// <param name="Width">The width of the leaf.</param>
/// <param name="Height">The height of the leaf.</param>
/// <param name="Offset">
/// The index of the leaf's first cell within the topology's combined, row-major
/// cell buffer.  The leaf's cells occupy
/// <c>[Offset, Offset + (Width * Height))</c>.
/// </param>
public readonly record struct TopologyLeaf<TCell>(
	IGrid<TCell> Grid,
	int Column,
	int Row,
	int Width,
	int Height,
	int Offset
) {
	public int Length => Width * Height;
}
