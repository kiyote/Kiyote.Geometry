namespace Kiyote.Geometry.Topology;

/// <summary>
/// The operations a compiled space needs from a layer without knowing its
/// value type.
/// </summary>
internal interface IBoundGridLayer<TCell> : IGridLayerStorage {

	ReadOnlySpan<int> DirtyChunks { get; }

	void Load(
		IGridSource<TCell> source,
		SourceRun run
	);

	void Store(
		IGridSource<TCell> source,
		SourceRun run
	);
}
