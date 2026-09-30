using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

[MemoryDiagnoser( false )]
public class DirectionExtensionsBenchmarks {

	private const int DirectionCount = 8;

	private readonly Direction[] _directions = [
		Direction.North,
		Direction.NorthEast,
		Direction.East,
		Direction.SouthEast,
		Direction.South,
		Direction.SouthWest,
		Direction.West,
		Direction.NorthWest
	];

	[Benchmark( OperationsPerInvoke = DirectionCount )]
	public int ToOffset() {
		int sum = 0;
		foreach( Direction direction in _directions ) {
			Point offset = direction.ToOffset();
			sum += offset.X + offset.Y;
		}
		return sum;
	}

	[Benchmark( OperationsPerInvoke = DirectionCount )]
	public int Opposite() {
		int sum = 0;
		foreach( Direction direction in _directions ) {
			sum += (int)direction.Opposite();
		}
		return sum;
	}

	[Benchmark( OperationsPerInvoke = DirectionCount )]
	public int TryGetOrthogonals() {
		int sum = 0;
		foreach( Direction direction in _directions ) {
			if( direction.TryGetOrthogonals( out Direction first, out Direction second ) ) {
				sum += (int)first + (int)second;
			}
		}
		return sum;
	}
}
