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
	/// The width of the source in its own, zero-based, coordinate space.
	/// </summary>
	int Width { get; }

	/// <summary>
	/// The height of the source in its own, zero-based, coordinate space.
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
	/// outside the source.  While attached to an assembly, only the assembly
	/// may call this, through <see cref="IGridAssembly{TCell}.TryAddCell"/>,
	/// so that seams and compiled spaces stay consistent.
	/// </summary>
	bool TrySetCell(
		int column,
		int row,
		in TCell cell
	);

	/// <summary>
	/// Attempts to vacate the supplied source-space location.  Returns false,
	/// leaving the source unchanged, when the source does not support changing
	/// its shape or the location is outside the source.  While attached to an
	/// assembly, only the assembly may call this, through
	/// <see cref="IGridAssembly{TCell}.TryRemoveCell"/>.
	/// </summary>
	bool TryClearCell(
		int column,
		int row
	);
}
