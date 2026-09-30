using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

[MemoryDiagnoser( false )]
public class GridLayerBenchmarks {

	private ICompiledGridAssembly<BenchCell> _compiled = null!;
	private IGridLayer<float> _pressure = null!;

	[Params( 64, 256 )]
	public int Size { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		GridAssembly<BenchCell> assembly = TopologyBenchmarkData.CreateAssembly( Size );
		_compiled = new GridCompiler().Compile( assembly, GridCompiler.DefaultChunkSize );
		_pressure = _compiled.Bind<float, PressureBinding>( default, 1 );
	}

	[GlobalCleanup]
	public void GlobalCleanup() {
		_compiled.Dispose();
	}

	[Benchmark]
	public float Indexer_Scan() {
		float sum = 0.0f;
		int extent = 2 * Size;
		for( int row = 0; row < extent; row++ ) {
			for( int column = 0; column < extent; column++ ) {
				sum += _pressure[column, row];
			}
		}
		return sum;
	}

	[Benchmark]
	public float GetChunk_Scan() {
		float sum = 0.0f;
		int slots = _compiled.ChunkLayout.SlotCount;
		for( int slot = 0; slot < slots; slot++ ) {
			Span<float> chunk = _pressure.GetChunk( slot );
			for( int i = 0; i < chunk.Length; i++ ) {
				sum += chunk[i];
			}
		}
		return sum;
	}

	[Benchmark]
	public float Cells_Scan() {
		float sum = 0.0f;
		Span<float> cells = _pressure.Cells;
		for( int i = 0; i < cells.Length; i++ ) {
			sum += cells[i];
		}
		return sum;
	}

	[Benchmark]
	public void ExchangeHalos() {
		_pressure.ExchangeHalos();
	}
}
