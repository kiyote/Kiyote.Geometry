namespace Kiyote.Geometry.DelaunayVoronoi.D3;

// Counts and indices are coordinate offsets; mutations always move complete pairs.
internal ref struct VoronoiCellBuffer {
	private readonly Span<double> _coordinates;

	internal VoronoiCellBuffer( Span<double> coordinates ) {
		if( coordinates.Length % 2 != 0 ) {
			throw new ArgumentException( "Coordinate scratch must hold complete vertex pairs.", nameof( coordinates ) );
		}
		_coordinates = coordinates;
		Count = 0;
	}

	internal int Count { get; private set; }

	internal ReadOnlySpan<double> Coordinates => _coordinates[..Count];

	internal double this[int index] => Coordinates[index];

	internal void Clear() {
		Count = 0;
	}

	internal void Insert( int index, double x, double y ) {
		if( (uint)index > (uint)Count || index % 2 != 0 ) {
			throw new ArgumentOutOfRangeException( nameof( index ) );
		}
		if( _coordinates.Length - Count < 2 ) {
			throw new InvalidOperationException( $"Clipped Voronoi scratch exceeds {_coordinates.Length / 2} vertices." );
		}
		if( index != Count ) {
			_coordinates[index..Count].CopyTo( _coordinates[(index + 2)..] );
		}
		_coordinates[index] = x;
		_coordinates[index + 1] = y;
		Count += 2;
	}

	internal void RemoveAt( int index ) {
		if( (uint)index >= (uint)Count || index % 2 != 0 ) {
			throw new ArgumentOutOfRangeException( nameof( index ) );
		}
		_coordinates[(index + 2)..Count].CopyTo( _coordinates[index..] );
		Count -= 2;
	}
}
