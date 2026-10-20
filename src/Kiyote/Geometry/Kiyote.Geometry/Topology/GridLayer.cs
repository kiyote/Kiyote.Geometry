using System.Runtime.InteropServices;
using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// A <see cref="ChunkedGridLayer{T}"/> over a compiled assembly's layout,
/// together with the per-chunk dirty flags used by commit.
/// </summary>
internal class GridLayer<T> : IGridLayer<T>, IGridLayerStorage where T : unmanaged {

	private bool[] _dirty;
	private List<int> _dirtyChunks;

	public GridLayer(
		GridLayout layout
	) {
		Layout = layout;
		Values = new ChunkedGridLayer<T>( layout.Storage );
		_dirty = [];
		_dirtyChunks = [];
	}

	public GridLayout Layout { get; }

	internal ChunkedGridLayer<T> Values { get; }

	public ReadOnlySpan<int> DirtyChunks => CollectionsMarshal.AsSpan( _dirtyChunks );

	public Span<T> GetValues( int chunkIndex ) => Values.GetValues( chunkIndex );

	public Span<T> GetRowValues( int chunkIndex, int row ) => Values.GetRowValues( chunkIndex, row );

	public void CopyPadded( int chunkIndex, int halo, Span<T> values ) =>
		Values.GetNeighbourhood( chunkIndex ).CopyPadded( halo, values );

	public void CopyPaddedRow( int chunkIndex, int row, int halo, Span<T> values ) =>
		Values.GetNeighbourhood( chunkIndex ).CopyPaddedRow( row, halo, values );

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
		if( !_dirty[chunkIndex] ) {
			_dirtyChunks.Add( chunkIndex );
			_dirty[chunkIndex] = true;
		}
	}

	public void ClearDirty() {
		foreach( int chunk in _dirtyChunks ) {
			_dirty[chunk] = false;
		}
		_dirtyChunks.Clear();
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
		( _dirtyChunks, other._dirtyChunks ) = ( other._dirtyChunks, _dirtyChunks );
	}
}
