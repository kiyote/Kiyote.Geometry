namespace Kiyote.Geometry.Topology;

/// <summary>
/// Determines whether two neighbouring occupied cells are connected.  Implement
/// as a struct so calls are devirtualised and inlined.
/// </summary>
public interface IConnectivityStrategy<TCell> {

	/// <param name="source">The cell connectivity is being evaluated for.</param>
	/// <param name="destination">The neighbouring cell in <paramref name="direction"/>.</param>
	/// <param name="direction">The direction of <paramref name="destination"/> relative to <paramref name="source"/>.</param>
	/// <param name="orthogonalA">
	/// When <paramref name="direction"/> is diagonal, the neighbour along the
	/// first orthogonal component (e.g. North for NorthEast).  Otherwise
	/// <see langword="default"/>.
	/// </param>
	/// <param name="orthogonalB">
	/// When <paramref name="direction"/> is diagonal, the neighbour along the
	/// second orthogonal component (e.g. East for NorthEast).  Otherwise
	/// <see langword="default"/>.
	/// </param>
	/// <param name="isSeam">
	/// True when <paramref name="source"/> and <paramref name="destination"/>
	/// belong to different placements.
	/// </param>
	bool Evaluate(
		in TopologyCell<TCell> source,
		in TopologyCell<TCell> destination,
		Direction direction,
		in TopologyCell<TCell> orthogonalA,
		in TopologyCell<TCell> orthogonalB,
		bool isSeam
	);
}
