namespace Kiyote.Geometry.Topology;

/// <summary>
/// Type-independent view of a layer.
/// </summary>
public interface IGridLayer {

	/// <summary>
	/// The chunk layout the layer's values follow; the same instance as
	/// <see cref="ICompiledGridAssembly{TCell}.ChunkLayout"/>.
	/// </summary>
	GridLayout Layout { get; }

	/// <summary>
	/// True when the chunk has been marked dirty since the last
	/// <see cref="ClearDirty"/>.  False for chunk indices that are not allocated.
	/// </summary>
	bool IsDirty(
		int chunkIndex
	);

	/// <summary>
	/// Marks a chunk as changed so that <see cref="ICompiledGridAssembly{TCell}.Commit"/>
	/// writes it back to the sources.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="chunkIndex"/> is not allocated in <see cref="Layout"/>.
	/// </exception>
	void MarkDirty(
		int chunkIndex
	);

	void ClearDirty();
}

/// <summary>
/// Typed cell data laid out over the chunks of a compiled assembly.
/// </summary>
/// <remarks>
/// The library never makes an unoccupied cell hold anything other than
/// <see langword="default"/>: new layers start zeroed, unoccupied cells are
/// never loaded and removed cells are reset.  Values written directly into
/// unoccupied cells through <see cref="GetValues"/> are the caller's
/// responsibility.
/// </remarks>
public interface IGridLayer<T> : IGridLayer where T : unmanaged {

	/// <summary>
	/// Writable chunk values. A chunk index and local index from
	/// <see cref="Layout"/> identify the same cell in every layer of the
	/// compiled assembly. Mark the chunk dirty after writing a bound layer.
	/// Obtained spans cannot be revoked; do not retain them across storage swaps.
	/// </summary>
	Span<T> GetValues( int chunkIndex );

	/// <summary>Returns writable values for one local row of a chunk.</summary>
	Span<T> GetRowValues( int chunkIndex, int row );

	/// <summary>
	/// Copies a chunk and a halo into caller-owned row-major scratch. The halo
	/// must be between zero and the chunk size. Missing chunks contribute zeroes.
	/// </summary>
	void CopyPadded( int chunkIndex, int halo, Span<T> values );

	/// <summary>Copies one local row of a chunk's padded neighbourhood.</summary>
	void CopyPaddedRow( int chunkIndex, int row, int halo, Span<T> values );

	/// <summary>
	/// Returns a reference to the value at the supplied assembly-space cell.
	/// Intended for convenience rather than hot loops.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The cell is not occupied.</exception>
	ref T this[int column, int row] { get; }
}
