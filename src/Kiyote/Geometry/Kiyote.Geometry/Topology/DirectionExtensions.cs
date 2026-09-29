namespace Kiyote.Geometry.Topology;

public static class DirectionExtensions {

	/// <summary>
	/// Returns the offset of the neighbouring cell in <paramref name="direction"/>.
	/// North is towards negative Y.  Returns (0, 0) for anything other than a
	/// single direction.
	/// </summary>
	public static Point ToOffset(
		this Direction direction
	) {
		return direction switch {
			Direction.North => new Point( 0, -1 ),
			Direction.NorthEast => new Point( 1, -1 ),
			Direction.East => new Point( 1, 0 ),
			Direction.SouthEast => new Point( 1, 1 ),
			Direction.South => new Point( 0, 1 ),
			Direction.SouthWest => new Point( -1, 1 ),
			Direction.West => new Point( -1, 0 ),
			Direction.NorthWest => new Point( -1, -1 ),
			_ => new Point( 0, 0 )
		};
	}

	/// <summary>
	/// Returns the direction of a neighbouring offset, or
	/// <see cref="Direction.None"/> when <paramref name="offset"/> is not a
	/// direct neighbour.
	/// </summary>
	public static Direction FromOffset(
		Point offset
	) {
		return ( offset.X, offset.Y ) switch {
			(0, -1) => Direction.North,
			(1, -1) => Direction.NorthEast,
			(1, 0) => Direction.East,
			(1, 1) => Direction.SouthEast,
			(0, 1) => Direction.South,
			(-1, 1) => Direction.SouthWest,
			(-1, 0) => Direction.West,
			(-1, -1) => Direction.NorthWest,
			_ => Direction.None
		};
	}

	/// <summary>
	/// Returns the direction pointing the opposite way.
	/// </summary>
	public static Direction Opposite(
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
			_ => Direction.None
		};
	}

	public static bool IsDiagonal(
		this Direction direction
	) {
		return direction is Direction.NorthEast
			or Direction.SouthEast
			or Direction.SouthWest
			or Direction.NorthWest;
	}

	/// <summary>
	/// Returns the two orthogonal directions that make up a diagonal, with the
	/// vertical component first (e.g. North then East for NorthEast).  Returns
	/// false for non-diagonal directions.
	/// </summary>
	public static bool TryGetOrthogonals(
		this Direction direction,
		out Direction first,
		out Direction second
	) {
		(first, second) = direction switch {
			Direction.NorthEast => (Direction.North, Direction.East),
			Direction.SouthEast => (Direction.South, Direction.East),
			Direction.SouthWest => (Direction.South, Direction.West),
			Direction.NorthWest => (Direction.North, Direction.West),
			_ => (Direction.None, Direction.None)
		};
		return first != Direction.None;
	}
}
