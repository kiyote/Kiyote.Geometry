namespace Kiyote.Geometry.Grids.Connectivity;

/// <summary>
/// An immutable snapshot of the leaves and connectivity of an
/// <see cref="IConnectivityGrid{TCell}"/>, laid out for fast per-cell iteration.
/// </summary>
/// <remarks>
/// Every leaf's cells are assigned a contiguous, row-major range within a single
/// combined buffer (see <see cref="TopologyLeaf{TCell}.Offset"/>).  Consumers can
/// allocate their own per-field buffers of <see cref="CellCount"/> elements and
/// use the same indices.  Connections within a leaf are described by
/// <see cref="Cells"/>; connections crossing between leaves are listed in
/// <see cref="Seams"/>.
/// </remarks>
public sealed class GridTopology<TCell> {

	private readonly TopologyLeaf<TCell>[] _leaves;
	private readonly Direction[] _connectivity;
	private readonly SeamLink[] _seams;

	internal GridTopology(
		TopologyLeaf<TCell>[] leaves,
		Direction[] connectivity,
		SeamLink[] seams,
		int version
	) {
		_leaves = leaves;
		_connectivity = connectivity;
		_seams = seams;
		Version = version;
	}

	/// <summary>
	/// The <see cref="IGrid{T}.Version"/> of the source grid when this snapshot was built.
	/// </summary>
	public int Version { get; }

	/// <summary>
	/// The total number of cells across all leaves.
	/// </summary>
	public int CellCount => _connectivity.Length;

	public ReadOnlySpan<TopologyLeaf<TCell>> Leaves => _leaves;

	/// <summary>
	/// The connectivity of every cell, indexed by combined-buffer index.
	/// </summary>
	public ReadOnlySpan<Direction> Cells => _connectivity;

	/// <summary>
	/// Directed connections between cells in different leaves.
	/// </summary>
	public ReadOnlySpan<SeamLink> Seams => _seams;

	/// <summary>
	/// The connectivity of the cells belonging to the leaf at <paramref name="leaf"/>,
	/// in row-major order.
	/// </summary>
	public ReadOnlySpan<Direction> GetConnectivity(
		int leaf
	) {
		TopologyLeaf<TCell> entry = _leaves[leaf];
		return _connectivity.AsSpan( entry.Offset, entry.Length );
	}

	/// <summary>
	/// Finds the combined-buffer index of the cell at the supplied connectivity-space
	/// location.
	/// </summary>
	/// <returns><see langword="false"/> if no leaf covers the location.</returns>
	public bool TryGetIndex(
		int column,
		int row,
		out int index
	) {
		foreach( TopologyLeaf<TCell> leaf in _leaves ) {
			int localColumn = column - leaf.Column;
			int localRow = row - leaf.Row;
			if( localColumn >= 0
				&& localColumn < leaf.Width
				&& localRow >= 0
				&& localRow < leaf.Height
			) {
				index = leaf.Offset + ( localRow * leaf.Width ) + localColumn;
				return true;
			}
		}
		index = -1;
		return false;
	}
}
