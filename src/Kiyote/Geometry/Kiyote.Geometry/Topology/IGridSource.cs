namespace Kiyote.Geometry.Topology;

/// <summary>
/// A source of truth for a set of cells, such as a single ship.  A source is
/// read when a compiled space binds a layer and is written only when that
/// space commits.  While attached to an assembly its shape may only be changed
/// through <see cref="IGridAssembly{TCell}.TryAddCell"/> and
/// <see cref="IGridAssembly{TCell}.TryRemoveCell"/>.
/// </summary>
public interface IGridSource<TCell> {

	/// <summary>
	/// This source's stable, exclusively owned lifecycle state. Implementations
	/// must call <see cref="GridSourceOwnership.EnsureShapeChangeAllowed"/> before
	/// changing occupancy. Dimensions and occupied runs must remain consistent,
	/// and dimensions must not change while attached. Callbacks must not reenter
	/// the assembly. Queries must remain stable for the duration of an operation;
	/// failed mutations must leave the source unchanged. Writable cell/row references
	/// must not change occupancy. Decorators over the same storage must forward its
	/// ownership state. Sources and their assemblies are not thread-safe.
	/// </summary>
	GridSourceOwnership Ownership { get; }

	/// <summary>
	/// The positive width of the source in its own, zero-based, coordinate space.
	/// </summary>
	int Width { get; }

	/// <summary>
	/// The positive height of the source in its own, zero-based, coordinate space.
	/// </summary>
	int Height { get; }

	/// <summary>
	/// Returns the occupied cells of <paramref name="row"/> as ordered,
	/// non-overlapping, non-adjacent runs in source space.
	/// </summary>
	ReadOnlySpan<CellRun> GetOccupiedRuns(
		int row
	);

	/// <summary>
	/// Returns true when the supplied source-space location is within bounds
	/// and occupied.
	/// </summary>
	bool IsOccupied(
		int column,
		int row
	);

	/// <summary>
	/// Returns a reference to the cell at the supplied source-space location.
	/// Throws <see cref="ArgumentOutOfRangeException"/> when the location is
	/// outside the source.  The result for an unoccupied, in-bounds location is
	/// implementation defined and must not be relied upon.
	/// </summary>
	ref TCell GetCell(
		int column,
		int row
	);

	/// <summary>
	/// Attempts to expose <paramref name="row"/> as contiguous storage so that
	/// extraction and commit can copy whole runs.  Sources without row-major
	/// storage return false and are accessed through <see cref="GetCell"/>.
	/// </summary>
	bool TryGetRow(
		int row,
		out Span<TCell> cells
	);

	/// <summary>
	/// Attempts to occupy the supplied source-space location with
	/// <paramref name="cell"/>.  Returns false, leaving the source unchanged,
	/// when the source does not support changing its shape or the location is
	/// outside the source. While attached, only the assembly may change occupancy,
	/// through <see cref="IGridAssembly{TCell}.TryAddCell"/>,
	/// so that seams and compiled spaces stay consistent. Direct changes to
	/// occupancy while attached throw <see cref="InvalidOperationException"/>.
	/// Updating the value of an existing cell is allowed.
	/// </summary>
	bool TrySetCell(
		int column,
		int row,
		in TCell cell
	);

	/// <summary>
	/// Attempts to vacate the supplied source-space location.  Returns false,
	/// leaving the source unchanged, when the source does not support changing
	/// its shape or the location is outside the source. While attached, only the
	/// assembly may change occupancy, through
	/// <see cref="IGridAssembly{TCell}.TryRemoveCell"/>. Direct changes to occupancy
	/// while attached throw <see cref="InvalidOperationException"/>.
	/// </summary>
	bool TryClearCell(
		int column,
		int row
	);
}
