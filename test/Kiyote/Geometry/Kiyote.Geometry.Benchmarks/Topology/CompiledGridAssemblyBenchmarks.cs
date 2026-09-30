using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

/// <summary>
/// Compiled assembly operations that leave its state unchanged, so they can
/// be invoked repeatedly against one compilation.
/// </summary>
[MemoryDiagnoser( false )]
public class CompiledGridAssemblyBenchmarks {

	private GridAssembly<BenchCell> _assembly = null!;
	private ICompiledGridAssembly<BenchCell> _compiled = null!;
	private IGridLayer<float> _pressure = null!;
	private PlacementId _placement;
	private BenchCell _cell;

	[Params( 64, 256 )]
	public int Size { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_assembly = TopologyBenchmarkData.CreateAssembly( Size );
		_compiled = new GridCompiler().Compile( _assembly, GridCompiler.DefaultChunkSize );
		_pressure = _compiled.Bind<float, PressureBinding>( default, 0 );
		_placement = _assembly.Placements[0].Id;
		_cell = new BenchCell {
			IsWalkable = true,
			Pressure = 1.0f
		};
	}

	[GlobalCleanup]
	public void GlobalCleanup() {
		_compiled.Dispose();
	}

	[Benchmark]
	public void Commit_AllChunksDirty() {
		int slots = _compiled.ChunkLayout.SlotCount;
		for( int slot = 0; slot < slots; slot++ ) {
			_pressure.MarkDirty( slot );
		}
		_compiled.Commit();
	}

	[Benchmark]
	public void Commit_OneChunkDirty() {
		_pressure.MarkDirty( 0 );
		_compiled.Commit();
	}

	[Benchmark]
	public void Commit_NothingDirty() {
		_compiled.Commit();
	}

	[Benchmark]
	public bool TryRemoveCell_TryAddCell_Propagated() {
		int middle = Size / 2;
		_ = _assembly.TryRemoveCell( _placement, middle, middle );
		return _assembly.TryAddCell( _placement, middle, middle, _cell );
	}
}
