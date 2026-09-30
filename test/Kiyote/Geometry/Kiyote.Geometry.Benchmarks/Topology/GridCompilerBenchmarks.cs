using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

[MemoryDiagnoser( false )]
public class GridCompilerBenchmarks {

	private GridAssembly<BenchCell> _assembly = null!;
	private GridCompiler _compiler = null!;

	[Params( 64, 256 )]
	public int Size { get; set; }

	[Params( 16, 32 )]
	public int ChunkSize { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_assembly = TopologyBenchmarkData.CreateAssembly( Size );
		_compiler = new GridCompiler();
	}

	[Benchmark]
	public int Compile() {
		using ICompiledGridAssembly<BenchCell> compiled = _compiler.Compile( _assembly, ChunkSize );
		return compiled.ChunkLayout.SlotCount;
	}
}
