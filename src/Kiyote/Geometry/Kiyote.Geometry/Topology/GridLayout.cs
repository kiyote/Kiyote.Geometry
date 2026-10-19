using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// A live, read-only view of a compiled assembly's chunk topology. Occupancy
/// changes only through the assembly; chunk indices remain stable while live.
/// </summary>
public sealed class GridLayout {
	internal GridLayout( ChunkedGridLayout storage ) {
		Storage = storage;
	}

	internal ChunkedGridLayout Storage { get; }

	public int ChunkSize => Storage.ChunkSize;
	public int ChunkShift => Storage.ChunkShift;
	public int ChunkMask => Storage.ChunkMask;
	public int ChunkCount => Storage.ChunkCount;
	public int ChunkCapacity => Storage.ChunkCapacity;
	public int PresenceWordsPerRow => Storage.PresenceWordsPerRow;
	public int PresenceWordsPerChunk => Storage.PresenceWordsPerChunk;
	public int Version => Storage.Version;
	public bool IsEmpty => Storage.IsEmpty;

	public bool IsAllocated( int chunkIndex ) => Storage.IsAllocated( chunkIndex );
	public int GetChunkColumn( int chunkIndex ) => Storage.GetChunkColumn( chunkIndex );
	public int GetChunkRow( int chunkIndex ) => Storage.GetChunkRow( chunkIndex );
	public int GetPopulatedCount( int chunkIndex ) => Storage.GetPopulatedCount( chunkIndex );
	public ChunkFill GetFill( int chunkIndex ) => Storage.GetFill( chunkIndex );
	public bool IsPopulated( int column, int row ) => Storage.IsPopulated( column, row );
	public bool TryGetChunk( int chunkColumn, int chunkRow, out int chunkIndex ) =>
		Storage.TryGetChunk( chunkColumn, chunkRow, out chunkIndex );
	public bool TryLocate( int column, int row, out int chunkIndex, out int localIndex ) =>
		Storage.TryLocate( column, row, out chunkIndex, out localIndex );
	public ReadOnlySpan<int> GetNeighbours( int chunkIndex ) => Storage.GetNeighbours( chunkIndex );
	public ReadOnlySpan<ulong> GetPresence( int chunkIndex ) => Storage.GetPresence( chunkIndex );
	public ReadOnlySpan<ulong> GetPresenceRow( int chunkIndex, int row ) => Storage.GetPresenceRow( chunkIndex, row );
	public ulong ReadPresenceBits( int chunkIndex, int column, int row, int count ) =>
		Storage.ReadPresenceBits( chunkIndex, column, row, count );
	public ChunkIndexEnumerator EnumerateChunks() => Storage.EnumerateChunks();
}
