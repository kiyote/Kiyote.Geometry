using System.Runtime.InteropServices;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// Intrusive chunk lists over a dense owner's entry indices. The owner must
/// append and swap-remove entries in lockstep with this index.
/// </summary>
internal sealed class ChunkEntryIndex {
	private int[] _heads = [];
	private readonly List<(int Previous, int Next)> _links = [];

	public int First( int chunk ) => (uint)chunk < (uint)_heads.Length ? _heads[chunk] : -1;

	public int Next( int entry ) => _links[entry].Next;

	public void Add( int chunk, int chunkCapacity ) {
		if( chunk >= _heads.Length ) {
			int previousLength = _heads.Length;
			int capacity = (int)Math.Max( chunkCapacity, Math.Min( Array.MaxLength, Math.Max( 4L, 2L * previousLength ) ) );
			Array.Resize( ref _heads, capacity );
			_heads.AsSpan( previousLength ).Fill( -1 );
		}
		int entry = _links.Count;
		int first = _heads[chunk];
		_links.Add( (-1, first) );
		if( first >= 0 ) {
			CollectionsMarshal.AsSpan( _links )[first].Previous = entry;
		}
		_heads[chunk] = entry;
	}

	public void RemoveAt( int entry, int chunk, int lastChunk ) {
		Span<(int Previous, int Next)> links = CollectionsMarshal.AsSpan( _links );
		(int previous, int next) = links[entry];
		if( previous < 0 ) {
			_heads[chunk] = next;
		} else {
			links[previous].Next = next;
		}
		if( next >= 0 ) {
			links[next].Previous = previous;
		}

		int last = links.Length - 1;
		if( entry != last ) {
			// Unlink first: the moved tail may have been adjacent to the removed entry.
			(int movedPrevious, int movedNext) = links[last];
			links[entry] = links[last];
			if( movedPrevious < 0 ) {
				_heads[lastChunk] = entry;
			} else {
				links[movedPrevious].Next = entry;
			}
			if( movedNext >= 0 ) {
				links[movedNext].Previous = entry;
			}
		}
		_links.RemoveAt( last );
	}
}
