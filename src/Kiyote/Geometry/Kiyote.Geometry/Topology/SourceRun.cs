namespace Kiyote.Geometry.Topology;

/// <summary>
/// A contiguous strip of cells that maps directly between a source and a
/// chunk.  Runs never cross a chunk boundary.  Used for both extraction and
/// commit.
/// </summary>
/// <param name="Placement">The placement the run belongs to.</param>
/// <param name="SourceColumn">The first column of the run in source space.</param>
/// <param name="SourceRow">The row of the run in source space.</param>
/// <param name="Slot">The chunk slot the run lies in.</param>
/// <param name="LocalIndex">The row-major index of the first cell within the chunk.</param>
/// <param name="Length">The number of cells in the run.</param>
public readonly record struct SourceRun(
	PlacementId Placement,
	int SourceColumn,
	int SourceRow,
	int Slot,
	int LocalIndex,
	int Length
);
