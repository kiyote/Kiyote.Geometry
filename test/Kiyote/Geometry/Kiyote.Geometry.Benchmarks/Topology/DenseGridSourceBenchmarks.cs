using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

[MemoryDiagnoser( false )]
public class DenseGridSourceBenchmarks {

	private DenseGridSource<BenchCell> _source = null!;
	private BenchCell _cell;

	[Params( 64, 256 )]
	public int Size { get; set; }

	[GlobalSetup]
	public void GlobalSetup() {
		_source = TopologyBenchmarkData.CreateSource( Size );
		_cell = new BenchCell {
			IsWalkable = true,
			Pressure = 1.0f
		};
		_ = GetOccupiedRuns_Cached();
	}

	[Benchmark]
	public int GetOccupiedRuns_Cached() {
		int count = 0;
		for( int row = 0; row < Size; row++ ) {
			count += _source.GetOccupiedRuns( row ).Length;
		}
		return count;
	}

	[Benchmark]
	public int TryClearCell_TrySetCell_RebuildRuns() {
		int middle = Size / 2;
		_ = _source.TryClearCell( middle, middle );
		int count = _source.GetOccupiedRuns( middle ).Length;
		_ = _source.TrySetCell( middle, middle, _cell );
		count += _source.GetOccupiedRuns( middle ).Length;
		return count;
	}

	[Benchmark]
	public int IsOccupied_Scan() {
		int count = 0;
		for( int row = 0; row < Size; row++ ) {
			for( int column = 0; column < Size; column++ ) {
				if( _source.IsOccupied( column, row ) ) {
					count++;
				}
			}
		}
		return count;
	}

	[Benchmark]
	public float GetCell_Scan() {
		float sum = 0.0f;
		for( int row = 0; row < Size; row++ ) {
			for( int column = 0; column < Size; column++ ) {
				sum += _source.GetCell( column, row ).Pressure;
			}
		}
		return sum;
	}

	[Benchmark]
	public float TryGetRow_Scan() {
		float sum = 0.0f;
		for( int row = 0; row < Size; row++ ) {
			if( _source.TryGetRow( row, out Span<BenchCell> cells ) ) {
				for( int i = 0; i < cells.Length; i++ ) {
					sum += cells[i].Pressure;
				}
			}
		}
		return sum;
	}
}
