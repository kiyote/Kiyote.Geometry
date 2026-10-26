using System.Buffers;

namespace Kiyote.Geometry.DelaunayVoronoi.D3;

internal ref struct DelaunatorEdgeStack {
	private Span<int> _buffer;
	private int[]? _rented;

	internal DelaunatorEdgeStack( Span<int> buffer ) {
		_buffer = buffer;
		_rented = null;
		Count = 0;
	}

	internal int Count { get; private set; }

	internal void Push( int edge ) {
		if( Count == _buffer.Length ) {
			Grow();
		}
		_buffer[Count++] = edge;
	}

	internal int Pop() {
		return _buffer[--Count];
	}

	private void Grow() {
		int[] next = ArrayPool<int>.Shared.Rent( checked( Math.Max( 16, _buffer.Length * 2 ) ) );
		_buffer[..Count].CopyTo( next );
		if( _rented is not null ) {
			ArrayPool<int>.Shared.Return( _rented );
		}
		_rented = next;
		_buffer = next;
	}

	public void Dispose() {
		if( _rented is not null ) {
			ArrayPool<int>.Shared.Return( _rented );
		}
		this = default;
	}
}
