namespace Kiyote.Geometry.Randomization;

/**
* Derived from SharpNeat:
* https://sourceforge.net/projects/sharpneat/
*/

[System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1812:An internal (assembly-level) type is never instantiated.", Justification = "This class is instantiated via DI." )]
internal sealed class FastRandom : IRandom {
	private static readonly IRandom _seedRng = new FastRandom( Environment.TickCount );

	// The +1 ensures NextDouble doesn't generate 1.0
	public const double REAL_UNIT_INT = 1.0 / ( int.MaxValue + 1.0 );
	public const double REAL_UNIT_UINT = 1.0 / ( uint.MaxValue + 1.0 );
	public const float SINGLE_UNIT_INT = 1.0f / (float)( int.MaxValue + 1.0 );
	public const float SINGLE_UNIT_UINT = 1.0f / (float)( uint.MaxValue + 1.0 );
	public const uint Y = 842502087, Z = 3579807591, W = 273326509;

	// Used by NextBool
	// Buffer 32 bits in bitBuffer, return 1 at a time, keep track of how many have been returned
	// with bitMask.
	private uint _bitBuffer;
	private uint _bitMask;

	// Used by NextByte
	// Buffer of random bytes. A single UInt32 is used to buffer 4 bytes.
	// _byteBufferState tracks how bytes remain in the buffer, a value of 
	// zero  indicates that the buffer is empty.
	private uint _byteBuffer;
	private byte _byteBufferState;

	private uint _x;
	private uint _y;
	private uint _z;
	private uint _w;

	public FastRandom() {
		( this as IRandom ).Reinitialise( _seedRng.NextInt() );
	}

	public FastRandom(
		int seed
	) {
		( this as IRandom ).Reinitialise( seed );
	}

	void IRandom.Reinitialise(
		int seed
	) {
		// The only stipulation stated for the xorshift RNG is that at least one of
		// the seeds x,y,z,w is non-zero. We fulfill that requirement by only allowing
		// resetting of the x seed.

		// The first random sample will be very closely related to the value of _x we set here. 
		// Thus setting _x = seed will result in a close correlation between the bit patterns of the seed and
		// the first random sample, therefore if the seed has a pattern (e.g. 1,2,3) then there will also be 
		// a recognisable pattern across the first random samples.
		//
		// Such a strong correlation between the seed and the first random sample is an undesirable
		// charactersitic of a RNG, therefore we significantly weaken any correlation by hashing the seed's bits. 
		// This is achieved by multiplying the seed with four large primes each with bits distributed over the
		// full length of a 32bit value, finally adding the results to give _x.
		_x = (uint)( ( seed * 1431655781 )
					+ ( seed * 1183186591 )
					+ ( seed * 622729787 )
					+ ( seed * 338294347 ) );

		_y = Y;
		_z = Z;
		_w = W;

		_bitBuffer = 0;
		_bitMask = 1;
	}

	public uint NextUInt() {
		uint t = _x ^ ( _x << 11 );
		_x = _y;
		_y = _z;
		_z = _w;
		return _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 );
	}

	/// <summary>
	/// Generates a single random bit.
	/// This method's performance is improved by generating 32 bits in one operation and storing them
	/// ready for future calls.
	/// </summary>
	public bool NextBool() {
		if( 0 == _bitMask ) {
			// Generate 32 more bits.
			uint t = _x ^ ( _x << 11 );
			_x = _y;
			_y = _z;
			_z = _w;
			_bitBuffer = _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 );

			// Reset the bitMask that tells us which bit to read next.
			_bitMask = 0x80000000;
			return ( _bitBuffer & _bitMask ) == 0;
		}

		return ( _bitBuffer & ( _bitMask >>= 1 ) ) == 0;
	}


	/// <summary>
	/// Generates a signle random byte with range [0,255].
	/// This method's performance is improved by generating 4 bytes in one operation and storing them
	/// ready for future calls.
	/// </summary>
	byte IRandom.NextByte() {
		if( 0 == _byteBufferState ) {
			// Generate 4 more bytes.
			uint t = _x ^ ( _x << 11 );
			_x = _y;
			_y = _z;
			_z = _w;
			_byteBuffer = _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 );
			_byteBufferState = 0x4;
			return (byte)_byteBuffer;  // Note. Masking with 0xFF is unnecessary.
		}
		_byteBufferState >>= 1;
		return (byte)( _byteBuffer >>= 1 );
	}

	double IRandom.NextDouble() {
		uint t = _x ^ ( _x << 11 );
		_x = _y;
		_y = _z;
		_z = _w;

		// Here we can gain a 2x speed improvement by generating a value that can be cast to 
		// an int instead of the more easily available uint. If we then explicitly cast to an 
		// int the compiler will then cast the int to a double to perform the multiplication, 
		// this final cast is a lot faster than casting from a uint to a double. The extra cast
		// to an int is very fast (the allocated bits remain the same) and so the overall effect 
		// of the extra cast is a significant performance improvement.
		//
		// Also note that the loss of one bit of precision is equivalent to what occurs within 
		// System.Random.
		return REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 ) ) );
	}

	float IRandom.NextFloat() {
		uint t = _x ^ ( _x << 11 );
		_x = _y;
		_y = _z;
		_z = _w;

		// Here we can gain a 2x speed improvement by generating a value that can be cast to 
		// an int instead of the more easily available uint. If we then explicitly cast to an 
		// int the compiler will then cast the int to a double to perform the multiplication, 
		// this final cast is a lot faster than casting from a uint to a double. The extra cast
		// to an int is very fast (the allocated bits remain the same) and so the overall effect 
		// of the extra cast is a significant performance improvement.
		//
		// Also note that the loss of one bit of precision is equivalent to what occurs within 
		// System.Random.
		return SINGLE_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 ) ) );
	}

	float IRandom.NextFloat(
		float lowerBound,
		float upperBound
	) {
		if (upperBound < 0) {
			throw new ArgumentOutOfRangeException(
				nameof( upperBound ),
				upperBound,
				"upperBound must be > 0"
			);
		}

		if( lowerBound > upperBound ) {
			throw new ArgumentOutOfRangeException(
				nameof( upperBound ),
				upperBound,
				"upperBound must be > lowerBound"
			);
		}

		uint t = _x ^ ( _x << 11 );
		_x = _y;
		_y = _z;
		_z = _w;

		// The explicit int cast before the first multiplication gives better performance.
		// See comments in NextDouble.
		float range = upperBound - lowerBound;
		// 31 bits of precision will suffice if range<=int.MaxValue. This allows us to cast to an int and gain
		// a little more performance.
		return lowerBound + ( SINGLE_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 ) ) ) * range );
	}

	int IRandom.NextInt() {
		uint t = _x ^ ( _x << 11 );
		_x = _y; _y = _z; _z = _w;
		return (int)( 0x7FFFFFFF & ( _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 ) ) );
	}

	int IRandom.NextInt(
		int upperBound
	) {
		if( upperBound <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( upperBound ),
				upperBound,
				"upperBound must be > 0"
			);
		}

		uint t = _x ^ ( _x << 11 );
		_x = _y;
		_y = _z;
		_z = _w;

		// ENHANCEMENT: Can we do this without converting to a double and back again?
		// The explicit int cast before the first multiplication gives better performance.
		// See comments in NextDouble.
		return (int)( REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 ) ) ) * upperBound );
	}

	int IRandom.NextInt(
		int lowerBound,
		int upperBound
	) {
		if( upperBound <= 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( upperBound ),
				upperBound,
				"upperBound must be > 0"
			);
		}

		if( lowerBound > upperBound ) {
			throw new ArgumentOutOfRangeException(
				nameof( upperBound ),
				upperBound,
				"upperBound must be > lowerBound"
			);
		}

		uint t = _x ^ ( _x << 11 );
		_x = _y;
		_y = _z;
		_z = _w;

		int range = upperBound - lowerBound;
		// 31 bits of precision will suffice if range<=int.MaxValue. This allows us to cast to an int and gain
		// a little more performance.
		return lowerBound + (int)( REAL_UNIT_INT * (int)( 0x7FFFFFFF & ( _w = _w ^ ( _w >> 19 ) ^ t ^ ( t >> 8 ) ) ) * range );
	}

	void IRandom.NextBytes(
		byte[] buffer
	) {
		// Fill up the bulk of the buffer in chunks of 4 bytes at a time.
		uint x = _x;
		uint y = _y;
		uint z = _z;
		uint w = _w;
		int i = 0;
		uint t;
		for( int bound = buffer.Length - 3; i < bound; ) {
			// Generate 4 bytes. 
			// Increased performance is achieved by generating 4 random bytes per loop.
			// Also note that no mask needs to be applied to zero out the higher order bytes before
			// casting because the cast ignores those bytes. Thanks to Stefan Troschütz for pointing this out.
			t = x ^ ( x << 11 );
			x = y; y = z; z = w;
			w = w ^ ( w >> 19 ) ^ t ^ ( t >> 8 );

			buffer[i++] = (byte)w;
			buffer[i++] = (byte)( w >> 8 );
			buffer[i++] = (byte)( w >> 16 );
			buffer[i++] = (byte)( w >> 24 );
		}

		// Fill up any remaining bytes in the buffer.
		if( i < buffer.Length ) {
			// Generate 4 bytes.
			t = x ^ ( x << 11 );
			x = y; y = z; z = w;
			w = w ^ ( w >> 19 ) ^ t ^ ( t >> 8 );

			buffer[i++] = (byte)w;
			if( i < buffer.Length ) {
				buffer[i++] = (byte)( w >> 8 );
				if( i < buffer.Length ) {
					buffer[i++] = (byte)( w >> 16 );
				}
			}
		}
		_x = x;
		_y = y;
		_z = z;
		_w = w;
	}
}
