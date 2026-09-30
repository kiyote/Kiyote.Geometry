using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

/// <summary>
/// Operations that permanently change a compiled assembly: each registers a
/// new layer.
/// and runs the operation once.
/// </summary>
[MemoryDiagnoser( false )]
[InvocationCount( 1 )]
public class CompiledGridAssemblyMutationBenchmarks {

	private GridAssembly<BenchCell> _assembly = null!;
	private ICompiledGridAssembly<BenchCell> _compiled = null!;
	private ConnectivityBuilder _builder = null!;

	[Params( 64, 256 )]
	public int Size { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_assembly = TopologyBenchmarkData.CreateAssembly( Size );
		_builder = new ConnectivityBuilder();
	}

	[IterationSetup]
	public void IterationSetup() {
		_compiled = new GridCompiler().Compile( _assembly, GridCompiler.DefaultChunkSize );
		_ = _compiled.Bind<float, PressureBinding>( default, 1 );
	}

	[IterationCleanup]
	public void IterationCleanup() {
		_compiled.Dispose();
	}

	[Benchmark]
	public IGridLayer<float> Bind_Halo0() {
		return _compiled.Bind<float, PressureBinding>( default, 0 );
	}

	[Benchmark]
	public IGridLayer<float> Bind_Halo1() {
		return _compiled.Bind<float, PressureBinding>( default, 1 );
	}

	[Benchmark]
	public IGridLayer<Direction> ConnectivityBuilder_Build_Halo0() {
		return _builder.Build( _compiled, new WalkableStrategy(), 0 );
	}

	[Benchmark]
	public IGridLayer<Direction> ConnectivityBuilder_Build_Halo1() {
		return _builder.Build( _compiled, new WalkableStrategy(), 1 );
	}
	}
