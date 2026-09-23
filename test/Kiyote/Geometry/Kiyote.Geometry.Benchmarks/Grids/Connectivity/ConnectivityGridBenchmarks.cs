using Kiyote.Geometry.Grids;
using Kiyote.Geometry.Grids.Connectivity;

namespace Kiyote.Geometry.Benchmarks.Grids.Connectivity;

[MemoryDiagnoser( false )]
public class ConnectivityGridBenchmarks {

	private sealed class ConnectivityStrategy : IConnectivityStrategy<bool> {
		public bool Evaluate(
			GridCell<bool> source,
			GridCell<bool> destination,
			Direction direction,
			GridCell<bool> orthogonalA,
			GridCell<bool> orthogonalB
		) {
			return true;
		}
	}

	private readonly IConnectivityGrid<bool> _grid;
	private readonly ConnectivityStrategy _strategy;

	public ConnectivityGridBenchmarks() {
		_strategy = new ConnectivityStrategy();
		_grid = new ConnectivityGrid<bool>();
		ArrayGrid<bool> block1 = new ArrayGrid<bool>( 10, 10 );
		ArrayGrid<bool> block2 = new ArrayGrid<bool>( 2, 10 );
		ArrayGrid<bool> block3 = new ArrayGrid<bool>( 10, 10 );

		_grid.TryAttach( block1, 0, 0 );
		_grid.TryAttach( block2, 10, 9 );
		_grid.TryAttach( block3, 12, 18 );
		_grid.UpdateConnectivity( _strategy );
	}

	[Benchmark]
	public void UpdateConnectivity() {
		_ = _grid.UpdateConnectivity( _strategy );
	}
}
