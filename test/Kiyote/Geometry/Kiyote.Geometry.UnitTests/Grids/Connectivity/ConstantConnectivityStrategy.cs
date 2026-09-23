using Kiyote.Geometry.Grids;
using Kiyote.Geometry.Grids.Connectivity;

namespace Kiyote.Geometry.UnitTests.Grids.Connectivity;

public sealed class ConstantConnectivityStrategy : IConnectivityStrategy<bool> {
	bool IConnectivityStrategy<bool>.Evaluate(
		GridCell<bool> source,
		GridCell<bool> destination,
		Direction direction,
		GridCell<bool> orthogonalA,
		GridCell<bool> orthogonalB
	) {
		return true;
	}
}
