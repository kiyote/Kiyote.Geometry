namespace Kiyote.Geometry.Grids.Connectivity;

public static class ExtensionMethods {

	public static bool IsDiagonal(
		this Direction direction
	) {
		return direction is Direction.NorthEast or Direction.SouthEast or Direction.SouthWest or Direction.NorthWest;
	}

	public static bool IsCardinal(
		this Direction direction
	) {
		return direction is Direction.North or Direction.East or Direction.South or Direction.West;
	}

	public static Direction GetOpposite(
		this Direction direction
	) {
		return direction switch {
			Direction.North => Direction.South,
			Direction.NorthEast => Direction.SouthWest,
			Direction.East => Direction.West,
			Direction.SouthEast => Direction.NorthWest,
			Direction.South => Direction.North,
			Direction.SouthWest => Direction.NorthEast,
			Direction.West => Direction.East,
			Direction.NorthWest => Direction.SouthEast,
			_ => throw new ArgumentException( $"Invalid direction: {direction}" )
		};
	}
}
