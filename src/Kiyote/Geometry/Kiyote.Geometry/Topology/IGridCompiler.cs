namespace Kiyote.Geometry.Topology;

public interface IGridCompiler {

	/// <summary>
	/// Compiles a built-in <see cref="GridAssembly{TCell}"/> into a live chunked
	/// snapshot. Custom sources and bindings are supported, not alternate assembly
	/// implementations.
	/// </summary>
	/// <param name="assembly">The assembly to compile.</param>
	/// <param name="chunkSize">
	/// The chunk width and height.  Must be a power of two of at least
	/// <see cref="GridCompiler.MinimumChunkSize"/>.
	/// </param>
	/// <exception cref="ArgumentException">The assembly is not a built-in implementation.</exception>
	ICompiledGridAssembly<TCell> Compile<TCell>(
		IGridAssembly<TCell> assembly,
		int chunkSize
	);
}
