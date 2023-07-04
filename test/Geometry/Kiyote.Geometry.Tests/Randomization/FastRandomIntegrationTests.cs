namespace Kiyote.Geometry.Randomization.Tests;

[TestFixture]
public sealed class FastRandomIntegrationTests {

	private IRandom _random;

	[SetUp]
	public void SetUp() {
		_random = new FastRandom( 1 );
	}

	[Test]
	public void NextInt_FixedSeed_ValueMatches() {
		int value = _random.NextInt();

		Assert.AreEqual( 1585680410, value );
	}

	[Test]
	public void NextIntUpperBound_FixedSeed_ValueMatches() {
		// Expected value is 516872986
		int value = _random.NextInt( 700000000 );

		Assert.Less( value, 700000000 );
	}

	[Test]
	public void NextIntUpperBoundLowerBound_FixedSeed_ValueInRange() {
		// Expected value is 543033988
		int value = _random.NextInt( 100000000, 700000000 );

		Assert.Less( value, 700000000 );
		Assert.Greater( value, 100000000 );
	}

	[Test]
	public void NextIntUpperBound_NegativeUpperBound_ThrowsException() {
		_ = Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextInt( -1 ) );
	}

	[Test]
	public void NextIntUpperBoundLowerBound_NegativeUpperBound_ThrowsException() {
		_ = Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextInt( -2, -1 ) );
	}

	[Test]
	public void NextIntUpperBoundLowerBound_LowerBoundAboveUpperBound_ThrowsException() {
		_ = Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextInt( 2, 1 ) );
	}

	[Test]
	public void NextUInt_FixedSeed_ValueMatches() {
		uint value = _random.NextUInt();

		Assert.AreEqual( 3733164058, value );
	}

	[Test]
	public void NextBool_FixedSeed_ValueMatches() {
		bool value = _random.NextBool();

		Assert.AreEqual( true, value );
	}

	[Test]
	public void NextBool_ExhaustBitmask_ValueMatches() {
		_ = _random.NextBool();
		bool value = _random.NextBool();

		Assert.AreEqual( false, value );
	}

	[Test]
	public void NextDouble_FixedSeed_ValueMatches() {
		double value = _random.NextDouble();

		Assert.AreEqual( 0.73838998097926378d, value );
	}

	[Test]
	public void NextFloat_FixedSeed_ValueMatches() {
		float value = _random.NextFloat();

		Assert.AreEqual( 0.738389969f, value );
	}

	[Test]
	public void NextFloatLowerBoundUpperBound_FixedSeed_ValueInRange() {
		// expected value 0.690712
		float value = _random.NextFloat( 0.1f, 0.9f );

		Assert.Less( value, 0.9f );
		Assert.Greater( value, 0.1f );
	}

	[Test]
	public void NextFloatUpperBoundLowerBound_NegativeUpperBound_ThrowsException() {
		_ = Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextFloat( -2, -1 ) );
	}

	[Test]
	public void NextFloatUpperBoundLowerBound_LowerBoundAboveUpperBound_ThrowsException() {
		_ = Assert.Throws<ArgumentOutOfRangeException>( () => _random.NextFloat( 2, 1 ) );
	}

	[Test]
	public void NextBytes_FewerThan4Bytes_ValuesMatch() {
		byte[] buffer = new byte[2];
		_random.NextBytes( buffer );

		Assert.AreEqual( 26, buffer[0] );
		Assert.AreEqual( 144, buffer[1] );
	}

	[Test]
	public void NextBytes_OneLessThanMultipleOf4_ValuesMatch() {
		byte[] buffer = new byte[7];
		_random.NextBytes( buffer );

		Assert.AreEqual( 26, buffer[0] );
		Assert.AreEqual( 144, buffer[1] );
		Assert.AreEqual( 131, buffer[2] );
		Assert.AreEqual( 222, buffer[3] );
		Assert.AreEqual( 186, buffer[4] );
		Assert.AreEqual( 117, buffer[5] );
		Assert.AreEqual( 68, buffer[6] );
	}
}

