using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Randomization.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
internal sealed class FastRandomTests {

	private const int Seed = 12345;

	private IRandom _random;

	[SetUp]
	public void SetUp() {
		_random = new FastRandom( Seed );
	}

	[Test]
	public void Ctor_Default_ProducesValues() {
		IRandom random = new FastRandom();

		Assert.That( random.NextInt(), Is.GreaterThanOrEqualTo( 0 ) );
	}

	[Test]
	public void Ctor_SameSeed_ProducesSameSequence() {
		IRandom first = new FastRandom( Seed );
		IRandom second = new FastRandom( Seed );

		for( int i = 0; i < 10; i++ ) {
			Assert.That( second.NextUInt(), Is.EqualTo( first.NextUInt() ) );
		}
	}

	[Test]
	public void Ctor_DifferentSeed_ProducesDifferentSequence() {
		IRandom first = new FastRandom( Seed );
		IRandom second = new FastRandom( Seed + 1 );

		Assert.That( second.NextUInt(), Is.Not.EqualTo( first.NextUInt() ) );
	}

	[Test]
	public void Reinitialise_SameSeed_RestartsSequence() {
		uint expected = _random.NextUInt();

		_random.Reinitialise( Seed );

		Assert.That( _random.NextUInt(), Is.EqualTo( expected ) );
	}

	[Test]
	public void NextUInt_RepeatedCalls_ValuesVary() {
		uint first = _random.NextUInt();
		uint second = _random.NextUInt();

		Assert.That( second, Is.Not.EqualTo( first ) );
	}

	[Test]
	public void NextBool_ManyCalls_ProducesBothValues() {
		// More than 32 calls so both the buffer refill and the buffered path run.
		bool sawTrue = false;
		bool sawFalse = false;
		for( int i = 0; i < 100; i++ ) {
			if( _random.NextBool() ) {
				sawTrue = true;
			} else {
				sawFalse = true;
			}
		}

		Assert.That( sawTrue, Is.True );
		Assert.That( sawFalse, Is.True );
	}

	[Test]
	public void NextDouble_ManyCalls_WithinUnitRange() {
		for( int i = 0; i < 1000; i++ ) {
			double value = _random.NextDouble();

			Assert.That( value, Is.GreaterThanOrEqualTo( 0.0 ) );
			Assert.That( value, Is.LessThan( 1.0 ) );
		}
	}

	[Test]
	public void NextFloat_ManyCalls_WithinUnitRange() {
		for( int i = 0; i < 1000; i++ ) {
			float value = _random.NextFloat();

			Assert.That( value, Is.GreaterThanOrEqualTo( 0.0f ) );
			Assert.That( value, Is.LessThan( 1.0f ) );
		}
	}

	[Test]
	public void NextFloat_Bounded_WithinRange() {
		for( int i = 0; i < 1000; i++ ) {
			float value = _random.NextFloat( 10.0f, 20.0f );

			Assert.That( value, Is.GreaterThanOrEqualTo( 10.0f ) );
			Assert.That( value, Is.LessThan( 20.0f ) );
		}
	}

	[Test]
	public void NextFloat_NegativeLowerBound_WithinRange() {
		for( int i = 0; i < 1000; i++ ) {
			float value = _random.NextFloat( -5.0f, 5.0f );

			Assert.That( value, Is.GreaterThanOrEqualTo( -5.0f ) );
			Assert.That( value, Is.LessThan( 5.0f ) );
		}
	}

	[Test]
	public void NextFloat_NegativeUpperBound_ThrowsException() {
		Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextFloat( -10.0f, -1.0f ) );
	}

	[Test]
	public void NextFloat_LowerBoundAboveUpperBound_ThrowsException() {
		Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextFloat( 20.0f, 10.0f ) );
	}

	[Test]
	public void NextInt_ManyCalls_NonNegative() {
		for( int i = 0; i < 1000; i++ ) {
			Assert.That( _random.NextInt(), Is.GreaterThanOrEqualTo( 0 ) );
		}
	}

	[Test]
	public void NextInt_UpperBound_WithinRange() {
		for( int i = 0; i < 1000; i++ ) {
			int value = _random.NextInt( 10 );

			Assert.That( value, Is.GreaterThanOrEqualTo( 0 ) );
			Assert.That( value, Is.LessThan( 10 ) );
		}
	}

	[TestCase( 0 )]
	[TestCase( -1 )]
	public void NextInt_UpperBoundNotPositive_ThrowsException(
		int upperBound
	) {
		Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextInt( upperBound ) );
	}

	[Test]
	public void NextInt_LowerAndUpperBound_WithinRange() {
		for( int i = 0; i < 1000; i++ ) {
			int value = _random.NextInt( 10, 20 );

			Assert.That( value, Is.GreaterThanOrEqualTo( 10 ) );
			Assert.That( value, Is.LessThan( 20 ) );
		}
	}

	[Test]
	public void NextInt_LowerAndUpperBoundUpperNotPositive_ThrowsException() {
		Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextInt( -20, -10 ) );
	}

	[Test]
	public void NextInt_LowerBoundAboveUpperBound_ThrowsException() {
		Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextInt( 30, 20 ) );
	}

	// 8 exercises only the 64 bit bulk loop, 4 adds the 32 bit tail, 3 adds the
	// byte tail, and 15 exercises all three paths in a single call.
	[TestCase( 8 )]
	[TestCase( 4 )]
	[TestCase( 3 )]
	[TestCase( 15 )]
	[TestCase( 16 )]
	public void NextBytes_VariousLengths_BufferFilled(
		int length
	) {
		byte[] buffer = new byte[length];

		_random.NextBytes( buffer );

		Assert.That( buffer, Is.Not.All.EqualTo( (byte)0 ) );
	}

	[Test]
	public void NextBytes_EmptyBuffer_DoesNotThrow() {
		byte[] buffer = [];

		Assert.DoesNotThrow( () => _random.NextBytes( buffer ) );
	}

	[Test]
	public void NextBytes_SameSeed_ProducesSameBytes() {
		byte[] first = new byte[32];
		byte[] second = new byte[32];
		IRandom other = new FastRandom( Seed );

		_random.NextBytes( first );
		other.NextBytes( second );

		Assert.That( second, Is.EqualTo( first ) );
	}

	[Test]
	public void NextBytes_ConsecutiveCalls_AdvancesState() {
		byte[] first = new byte[16];
		byte[] second = new byte[16];

		_random.NextBytes( first );
		_random.NextBytes( second );

		Assert.That( second, Is.Not.EqualTo( first ) );
	}
}
