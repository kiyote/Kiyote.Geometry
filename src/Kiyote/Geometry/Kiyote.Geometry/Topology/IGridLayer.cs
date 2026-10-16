using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// Type-independent view of a layer.
/// </summary>
public interface IGridLayer {

	/// <summary>
	/// The chunk layout the layer's values follow; the same instance as
	/// <see cref="ICompiledGridAssembly{TCell}.ChunkLayout"/>.
	/// </summary>
	ChunkedGridLayout Layout { get; }

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
/// unoccupied cells through <see cref="Values"/> are the caller's
/// responsibility.
/// </remarks>
public interface IGridLayer<T> : IGridLayer where T : unmanaged {

	/// <summary>
	/// The layer's values.  A chunk index and local index from
	/// <see cref="Layout"/> identify the same cell in every layer of the
	/// compiled assembly.  Do not change the layout's topology through it;
	/// cell edits are made through the assembly.
	/// </summary>
	ChunkedGridLayer<T> Values { get; }

	/// <summary>
	/// Returns a reference to the value at the supplied assembly-space cell.
	/// Intended for convenience rather than hot loops.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The cell is not occupied.</exception>
	ref T this[int column, int row] { get; }
}
