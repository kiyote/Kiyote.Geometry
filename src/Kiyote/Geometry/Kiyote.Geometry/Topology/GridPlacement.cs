namespace Kiyote.Geometry.Topology;

internal sealed class GridPlacement<TCell> : IGridPlacement<TCell> {

	private CellRun[] _scratch;

	public GridPlacement(
		PlacementId id,
		IGridSource<TCell> source,
		int column,
		int row
	) {
		Id = id;
		Source = source;
		Column = column;
		Row = row;
		Bounds = new Rect( column, row, source.Width, source.Height );
		_scratch = [];
	}

	public PlacementId Id { get; }

	public IGridSource<TCell> Source { get; }

	public int Column { get; }

	public int Row { get; }

	public Rect Bounds { get; }

	/// <remarks>
	/// The returned span is only valid until the next call.
	/// </remarks>
	public ReadOnlySpan<CellRun> GetOccupiedRuns(
		int row
	) {
		ReadOnlySpan<CellRun> runs = Source.GetOccupiedRuns( row - Row );
		if( runs.Length > _scratch.Length ) {
			_scratch = new CellRun[runs.Length];
		}
		for( int i = 0; i < runs.Length; i++ ) {
			CellRun run = runs[i];
			_scratch[i] = new CellRun( run.Column + Column, row, run.Length );
		}
		return _scratch.AsSpan( 0, runs.Length );
	}
}
