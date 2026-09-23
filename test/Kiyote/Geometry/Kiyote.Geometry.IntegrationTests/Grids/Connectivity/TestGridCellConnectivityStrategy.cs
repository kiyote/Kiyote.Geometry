using Kiyote.Geometry.Grids;
using Kiyote.Geometry.Grids.Connectivity;

namespace Kiyote.Geometry.IntegrationTests.Grids.Connectivity;

public sealed class TestGridCellConnectivityStrategy : IConnectivityStrategy<TestGridCell> {
	bool IConnectivityStrategy<TestGridCell>.Evaluate(
		GridCell<TestGridCell> source,
		GridCell<TestGridCell> destination,
		Direction direction,
		GridCell<TestGridCell> orthogonalA,
		GridCell<TestGridCell> orthogonalB
	) {
		if( source.Cell is null
			|| destination.Cell is null
		) {
			return false;
		}

		return source.Cell.IsSolid && destination.Cell.IsSolid;
	}
}
