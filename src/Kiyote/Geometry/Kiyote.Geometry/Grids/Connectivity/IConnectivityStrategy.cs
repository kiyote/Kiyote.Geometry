using Kiyote.Geometry.Grids;

namespace Kiyote.Geometry.Grids.Connectivity;

/// <summary>
/// Determines whether two neighbouring cells are connected, for use by
/// <see cref="IConnectivityGrid{TCell}"/>.
/// </summary>
public interface IConnectivityStrategy<TCell> {

	/// <summary>
	/// Evaluates whether <paramref name="source"/> is connected to
	/// <paramref name="destination"/>, which lies in the supplied
	/// <paramref name="direction"/> relative to <paramref name="source"/>.
	/// </summary>
	/// <param name="source">The cell connectivity is being evaluated for.</param>
	/// <param name="destination">The neighbouring cell in <paramref name="direction"/>.</param>
	/// <param name="direction">
	/// The direction of <paramref name="destination"/> relative to
	/// <paramref name="source"/>.
	/// </param>
	/// <param name="orthogonalA">
	/// When <paramref name="direction"/> is diagonal, the cell adjacent to
	/// <paramref name="source"/> along the first of the two orthogonal directions
	/// that make up the diagonal (e.g. North for NorthEast). Otherwise
	/// <see langword="default"/>.
	/// </param>
	/// <param name="orthogonalB">
	/// When <paramref name="direction"/> is diagonal, the cell adjacent to
	/// <paramref name="source"/> along the second of the two orthogonal directions
	/// that make up the diagonal (e.g. East for NorthEast). Otherwise
	/// <see langword="default"/>.
	/// </param>
	bool Evaluate(
		GridCell<TCell> source,
		GridCell<TCell> destination,
		Direction direction,
		GridCell<TCell> orthogonalA,
		GridCell<TCell> orthogonalB
	);

}

