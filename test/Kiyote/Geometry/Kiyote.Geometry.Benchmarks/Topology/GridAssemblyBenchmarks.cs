using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

/// <summary>
/// Assembly operations with no compiled assembly observing, so edits only
/// touch the source, the occupancy index and seams.
/// </summary>
[MemoryDiagnoser( false )]
public class GridAssemblyBenchmarks {

	private GridAssembly<BenchCell> _assembly = null!;
	private DenseGridSource<BenchCell> _extra = null!;
	private PlacementId _placement;
	private BenchCell _cell;

	[Params( 64, 256 )]
	public int Size { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_assembly = TopologyBenchmarkData.CreateAssembly( Size );
		_extra = TopologyBenchmarkData.CreateSource( Size );
		_placement = _assembly.Placements[0].Id;
		_cell = new BenchCell {
			IsWalkable = true,
			Pressure = 1.0f
		};
	}

	[Benchmark]
	public bool TryAttach_TryDetach() {
		AttachResult result = _assembly.TryAttach( _extra, 2 * Size, 0 );
		return _assembly.TryDetach( result.Placement );
	}

	[Benchmark]
	public int TryGetPlacementAt_Scan() {
		int count = 0;
		int extent = 2 * Size;
		for( int row = 0; row < extent; row++ ) {
			for( int column = 0; column < extent; column++ ) {
				if( _assembly.TryGetPlacementAt( column, row, out _ ) ) {
					count++;
				}
			}
		}
		return count;
	}

	[Benchmark]
	public bool TryRemoveCell_TryAddCell_Interior() {
		int middle = Size / 2;
		_ = _assembly.TryRemoveCell( _placement, middle, middle );
		return _assembly.TryAddCell( _placement, middle, middle, _cell );
	}

	[Benchmark]
	public bool TryRemoveCell_TryAddCell_Seam() {
		int edge = Size - 1;
		int middle = Size / 2;
		_ = _assembly.TryRemoveCell( _placement, edge, middle );
		return _assembly.TryAddCell( _placement, edge, middle, _cell );
	}
}
