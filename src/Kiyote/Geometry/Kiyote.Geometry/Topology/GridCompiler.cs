namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IGridCompiler"/>.
/// </summary>
public sealed class GridCompiler : IGridCompiler {

	public const int DefaultChunkSize = 32;

	public ICompiledGridAssembly<TCell> Compile<TCell>(
		IGridAssembly<TCell> assembly,
		int chunkSize
	) {
		ArgumentNullException.ThrowIfNull( assembly );

		return new CompiledGridAssembly<TCell>( assembly, chunkSize );
	}
}
