using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

internal struct BenchCell {
	public bool IsWalkable;
	public float Pressure;
}

internal readonly struct WalkableStrategy : IConnectivityStrategy<BenchCell> {
	public bool Evaluate(
		in TopologyCell<BenchCell> source,
		in TopologyCell<BenchCell> destination,
		Direction direction,
		in TopologyCell<BenchCell> orthogonalA,
		in TopologyCell<BenchCell> orthogonalB,
		bool isSeam
	) {
		if( direction.IsDiagonal() ) {
			return source.Cell.IsWalkable
				&& destination.Cell.IsWalkable
				&& orthogonalA.Cell.IsWalkable
				&& orthogonalB.Cell.IsWalkable;
		}
		return source.Cell.IsWalkable && destination.Cell.IsWalkable;
	}
}

internal readonly struct PressureBinding : IGridLayerBinding<BenchCell, float> {
	public float Extract(
		in BenchCell cell
	) {
		return cell.Pressure;
	}

	public void Commit(
		ref BenchCell cell,
		float value
	) {
		cell.Pressure = value;
	}
}

internal static class TopologyBenchmarkData {

	/// <summary>
	/// A fully occupied square source with walls around its edge and a wall
	/// on every eighth row, broken by a door.
	/// </summary>
	public static DenseGridSource<BenchCell> CreateSource(
		int size
	) {
		DenseGridSource<BenchCell> source = new DenseGridSource<BenchCell>( size, size );
		for( int row = 0; row < size; row++ ) {
			for( int column = 0; column < size; column++ ) {
				bool wall = row == 0
					|| column == 0
					|| row == size - 1
					|| column == size - 1
					|| ( row % 8 == 0 && column % 8 != 4 );
				BenchCell cell = new BenchCell {
					IsWalkable = !wall,
					Pressure = 1.0f
				};
				_ = source.TrySetCell( column, row, cell );
			}
		}
		return source;
	}

	/// <summary>
	/// Four touching sources in a 2x2 arrangement, covering
	/// (0, 0) to (2 * size - 1, 2 * size - 1).
	/// </summary>
	public static GridAssembly<BenchCell> CreateAssembly(
		int size
	) {
		GridAssembly<BenchCell> assembly = new GridAssembly<BenchCell>();
		_ = assembly.TryAttach( CreateSource( size ), 0, 0 );
		_ = assembly.TryAttach( CreateSource( size ), size, 0 );
		_ = assembly.TryAttach( CreateSource( size ), 0, size );
		_ = assembly.TryAttach( CreateSource( size ), size, size );
		return assembly;
	}
}
