using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

/// <summary>
/// Incremental connectivity updates.  <see cref="ConnectivityBuilder.Build"/>
/// is measured in <see cref="CompiledGridAssemblyMutationBenchmarks"/>, since
/// each call registers a new layer.
/// </summary>
[MemoryDiagnoser( false )]
public class ConnectivityBuilderBenchmarks {

	private ICompiledGridAssembly<BenchCell> _compiled = null!;
	private IGridLayer<Direction> _connectivity = null!;
	private ConnectivityBuilder _builder = null!;

	[Params( 64, 256 )]
	public int Size { get; set; }

	[Params( 0, 1 )]
	public int Halo { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		GridAssembly<BenchCell> assembly = TopologyBenchmarkData.CreateAssembly( Size );
		_compiled = new GridCompiler().Compile( assembly, GridCompiler.DefaultChunkSize );
		_builder = new ConnectivityBuilder();
		_connectivity = _builder.Build( _compiled, new WalkableStrategy(), Halo );
	}

	[GlobalCleanup]
	public void GlobalCleanup() {
		_compiled.Dispose();
	}

	[Benchmark]
	public void Update_SingleCell() {
		int middle = Size / 2;
		_builder.Update( _compiled, _connectivity, new WalkableStrategy(), new Rect( middle, middle, 1, 1 ) );
	}

	[Benchmark]
	public void Update_SeamCell() {
		_builder.Update( _compiled, _connectivity, new WalkableStrategy(), new Rect( Size - 1, Size / 2, 1, 1 ) );
	}

	[Benchmark]
	public void Update_32x32() {
		int middle = Size / 2;
		_builder.Update( _compiled, _connectivity, new WalkableStrategy(), new Rect( middle, middle, 32, 32 ) );
	}
}
