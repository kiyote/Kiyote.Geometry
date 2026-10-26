using System.Buffers;

namespace Kiyote.Geometry.DelaunayVoronoi.D3;

internal ref struct DelaunatorEdgeStack {
	private Span<int> _buffer;
	private int[]? _rented;

	internal DelaunatorEdgeStack( Span<int> buffer ) {
		_buffer = buffer;
		_rented = null;
	}

	internal Span<int> Buffer => _buffer;

	internal Span<int> Grow() {
		int[] next = ArrayPool<int>.Shared.Rent( checked( Math.Max( 16, _buffer.Length * 2 ) ) );
		_buffer.CopyTo( next );
		if( _rented is not null ) {
			ArrayPool<int>.Shared.Return( _rented );
		}
		_rented = next;
		_buffer = next;
		return _buffer;
	}

	public void Dispose() {
		if( _rented is not null ) {
			ArrayPool<int>.Shared.Return( _rented );
		}
		this = default;
	}
}
