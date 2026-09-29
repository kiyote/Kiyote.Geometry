namespace Kiyote.Geometry.Topology;

/// <summary>
/// A layer whose values are extracted from, and committed to, source cells
/// through a struct binding.
/// </summary>
internal sealed class BoundGridLayer<TCell, T, TBinding> : GridLayer<T>, IBoundGridLayer<TCell>
	where TBinding : struct, IGridLayerBinding<TCell, T> {

	private readonly TBinding _binding;

	public BoundGridLayer(
		GridChunkLayout space,
		TBinding binding,
		int halo
	) : base( space, halo ) {
		_binding = binding;
	}

	public void Load(
		IGridSource<TCell> source,
		SourceRun run
	) {
		Span<T> target = Cells.Slice( IndexOf( run.Slot, run.LocalIndex ), run.Length );
		if( source.TryGetRow( run.SourceRow, out Span<TCell> row ) ) {
			ReadOnlySpan<TCell> cells = row.Slice( run.SourceColumn, run.Length );
			for( int i = 0; i < cells.Length; i++ ) {
				target[i] = _binding.Extract( in cells[i] );
			}
			return;
		}
		for( int i = 0; i < run.Length; i++ ) {
			target[i] = _binding.Extract( in source.GetCell( run.SourceColumn + i, run.SourceRow ) );
		}
	}

	public void Store(
		IGridSource<TCell> source,
		SourceRun run
	) {
		ReadOnlySpan<T> values = Cells.Slice( IndexOf( run.Slot, run.LocalIndex ), run.Length );
		if( source.TryGetRow( run.SourceRow, out Span<TCell> row ) ) {
			Span<TCell> cells = row.Slice( run.SourceColumn, run.Length );
			for( int i = 0; i < cells.Length; i++ ) {
				_binding.Commit( ref cells[i], values[i] );
			}
			return;
		}
		for( int i = 0; i < run.Length; i++ ) {
			_binding.Commit( ref source.GetCell( run.SourceColumn + i, run.SourceRow ), values[i] );
		}
	}
}
