namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IGridCompiler"/>.
/// </summary>
public sealed class GridCompiler : IGridCompiler {

	/// <summary>
	/// The recommended chunk size.  Chunk sizes are powers of two, so any size
	/// of 16 or more is a whole multiple of a 128-bit or 256-bit vector of
	/// bytes or floats; 32 also suits 512-bit vectors and keeps a float chunk
	/// with a halo of 1 within a typical L1 cache.
	/// </summary>
	public const int DefaultChunkSize = 32;

	public ICompiledGridAssembly<TCell> Compile<TCell>(
		IGridAssembly<TCell> assembly,
		int chunkSize
	) {
		ArgumentNullException.ThrowIfNull( assembly );

		return new CompiledGridAssembly<TCell>( assembly, chunkSize );
	}
}
