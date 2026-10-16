namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IGridCompiler"/>.
/// </summary>
public sealed class GridCompiler : IGridCompiler {

	/// <summary>
	/// The recommended chunk size.  Chunk sizes are powers of two, so any size
	/// of 32 or more is a whole number of 256-bit vectors of any element type;
	/// 32 also keeps a float chunk with a one-cell padded border within a
	/// typical L1 cache.
	/// </summary>
	public const int DefaultChunkSize = 32;

	/// <summary>
	/// The smallest supported chunk size.  Every chunk row must hold a whole
	/// number of <see cref="System.Runtime.Intrinsics.Vector256{T}"/> lanes for
	/// any layer element type, and a byte-sized element has 32 lanes.
	/// </summary>
	public const int MinimumChunkSize = 32;

	/// <inheritdoc/>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="chunkSize"/> is not a power of two between
	/// <see cref="MinimumChunkSize"/> and
	/// <see cref="Kiyote.Buffers.ChunkedGrids.ChunkedGridLayout.MaximumChunkSize"/>.
	/// </exception>
	public ICompiledGridAssembly<TCell> Compile<TCell>(
		IGridAssembly<TCell> assembly,
		int chunkSize
	) {
		ArgumentNullException.ThrowIfNull( assembly );

		return new CompiledGridAssembly<TCell>( assembly, chunkSize );
	}
}
