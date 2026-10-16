using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// A <see cref="ChunkedGridLayer{T}"/> over a compiled assembly's layout,
/// together with the per-chunk dirty flags used by commit.
/// </summary>
internal class GridLayer<T> : IGridLayer<T>, IGridLayerStorage where T : unmanaged {

	private bool[] _dirty;

	public GridLayer(
		ChunkedGridLayout layout
	) {
		Values = new ChunkedGridLayer<T>( layout );
		_dirty = [];
	}

	public ChunkedGridLayout Layout => Values.Layout;

	public ChunkedGridLayer<T> Values { get; }

	public ref T this[int column, int row] => ref Values.GetReference( column, row );

	public bool IsDirty(
		int chunkIndex
	) {
		return (uint)chunkIndex < (uint)_dirty.Length && _dirty[chunkIndex];
	}

	public void MarkDirty(
		int chunkIndex
	) {
		if( !Layout.IsAllocated( chunkIndex ) ) {
			throw new ArgumentOutOfRangeException( nameof( chunkIndex ), chunkIndex, "The chunk is not allocated." );
		}
		if( chunkIndex >= _dirty.Length ) {
			Array.Resize( ref _dirty, Layout.ChunkCapacity );
		}
		_dirty[chunkIndex] = true;
	}

	public void ClearDirty() {
		Array.Clear( _dirty );
	}

	public void Reset(
		int chunkIndex,
		int localIndex
	) {
		Values.GetValues( chunkIndex )[localIndex] = default;
	}

	/// <summary>
	/// Exchanges values and dirty flags with another layer over the same layout.
	/// </summary>
	internal void SwapStorage(
		GridLayer<T> other
	) {
		Values.SwapStorage( other.Values );
		( _dirty, other._dirty ) = ( other._dirty, _dirty );
	}
}
