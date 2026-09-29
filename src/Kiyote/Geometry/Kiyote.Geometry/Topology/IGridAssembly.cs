using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// A set of non-overlapping placements of grid sources sharing a
/// single coordinate space.  Attach and detach are structural changes that make
/// compiled spaces stale; per-cell edits are propagated immediately to compiled
/// spaces.  Not thread-safe; callers must synchronise access.
/// </summary>
public interface IGridAssembly<TCell> {

	/// <summary>
	/// Changes whenever a placement is attached or detached.  Compiled spaces
	/// compare against this value to detect that they are stale.
	/// </summary>
	int Version { get; }

	/// <summary>
	/// The union of all placement bounds, or <see langword="null"/> when empty.
	/// </summary>
	Rect? Bounds { get; }

	IReadOnlyList<IGridPlacement<TCell>> Placements { get; }

	/// <summary>
	/// All seams between placements, captured at attach time.
	/// </summary>
	IReadOnlyList<GridSeam> Seams { get; }

	/// <summary>
	/// Attempts to place <paramref name="source"/> with its origin at the
	/// supplied assembly-space location.  The attach is rejected, leaving the
	/// assembly unchanged, if any occupied cell overlaps an existing placement.
	/// </summary>
	AttachResult TryAttach(
		IGridSource<TCell> source,
		int column,
		int row
	);

	/// <summary>
	/// Attempts to place <paramref name="source"/> using a caller-supplied
	/// identifier.  Throws when <paramref name="id"/> is
	/// <see cref="PlacementId.None"/> or already attached.  The overload without
	/// an identifier allocates one that is unused within this assembly.
	/// </summary>
	AttachResult TryAttach(
		IGridSource<TCell> source,
		PlacementId id,
		int column,
		int row
	);

	/// <summary>
	/// Removes the placement and any seams it participates in.
	/// </summary>
	bool TryDetach(
		PlacementId placement
	);

	/// <summary>
	/// Returns the placement with the supplied ID, if any.
	/// </summary>
	/// <remarks>
	/// Probe this assembly for the placement and from the placement back to the source.
	/// </remarks>
	/// <param name="placement">The ID of the placement to retrieve.</param>
	/// <param name="result">The placement with the specified ID, if found.</param>
	/// <returns><c>true</c> if the placement was found; otherwise, <c>false</c>.</returns>
	bool TryGetPlacement(
		PlacementId placement,
		[MaybeNullWhen( false )] out IGridPlacement<TCell> result
	);

	/// <summary>
	/// Attempts to occupy a cell of a placement's source.  Fails, leaving
	/// everything unchanged, when the placement does not exist, the location
	/// is outside
	/// the source, or the cell is already occupied by any placement.  On success
	/// seams and every compiled space of this assembly are updated.
	/// </summary>
	/// <param name="placement">The placement whose source receives the cell.</param>
	/// <param name="column">The source-space column.</param>
	/// <param name="row">The source-space row.</param>
	/// <param name="cell">The value of the new cell.</param>
	bool TryAddCell(
		PlacementId placement,
		int column,
		int row,
		in TCell cell
	);

	/// <summary>
	/// Attempts to vacate a cell of a placement's source.  Uncommitted layer
	/// values for the cell are discarded.
	/// </summary>
	/// <param name="placement">The placement whose source loses the cell.</param>
	/// <param name="column">The source-space column.</param>
	/// <param name="row">The source-space row.</param>
	bool TryRemoveCell(
		PlacementId placement,
		int column,
		int row
	);

	/// <summary>
	/// Returns the placement occupying the supplied assembly-space cell, if any.
	/// </summary>
	bool TryGetPlacementAt(
		int column,
		int row,
		[MaybeNullWhen( false )] out IGridPlacement<TCell> result
	);
}
