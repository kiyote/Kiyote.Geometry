namespace Kiyote.Geometry.Topology;

public interface IGridCompiler {

	/// <summary>
	/// Compiles <paramref name="assembly"/> into a chunked snapshot.
	/// </summary>
	/// <param name="assembly">The assembly to compile.</param>
	/// <param name="chunkSize">The chunk width and height.  Must be a power of two.</param>
	ICompiledGridAssembly<TCell> Compile<TCell>(
		IGridAssembly<TCell> assembly,
		int chunkSize
	);
}
