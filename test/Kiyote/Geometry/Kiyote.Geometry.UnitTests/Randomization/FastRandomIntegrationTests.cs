namespace Kiyote.Geometry.Randomization.UnitTests;

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

		Assert.That( value, Is.EqualTo( 1585680410 ) );
	}

	[Test]
	public void NextIntUpperBound_FixedSeed_ValueMatches() {
		// Expected value is 516872986
		int value = _random.NextInt( 700000000 );

		Assert.That( value, Is.LessThan( 700000000 ) );
	}

	[Test]
	public void NextIntUpperBoundLowerBound_FixedSeed_ValueInRange() {
		// Expected value is 543033988
		int value = _random.NextInt( 100000000, 700000000 );

		Assert.That( value, Is.LessThan( 700000000 ) );
		Assert.That( value, Is.GreaterThan( 100000000 ) );
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

		Assert.That( value, Is.EqualTo( 3733164058 ) );
	}

	[Test]
	public void NextBool_FixedSeed_ValueMatches() {
		bool value = _random.NextBool();

		Assert.That( value, Is.EqualTo( true ) );
	}

	[Test]
	public void NextBool_ExhaustBitmask_ValueMatches() {
		_ = _random.NextBool();
		bool value = _random.NextBool();

		Assert.That( value, Is.False );
	}

	[Test]
	public void NextDouble_FixedSeed_ValueMatches() {
		double value = _random.NextDouble();

		Assert.That( value, Is.EqualTo( 0.73838998097926378d ) );
	}

	[Test]
	public void NextFloat_FixedSeed_ValueMatches() {
		float value = _random.NextFloat();

		Assert.That( value, Is.EqualTo( 0.738389969f ) );
	}

	[Test]
	public void NextFloatLowerBoundUpperBound_FixedSeed_ValueInRange() {
		// expected value 0.690712
		float value = _random.NextFloat( 0.1f, 0.9f );

		Assert.That( value, Is.LessThan( 0.9f ) );
		Assert.That( value, Is.GreaterThan( 0.1f ) );
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

		Assert.That( buffer[0], Is.EqualTo( 26 ) );
		Assert.That( buffer[1], Is.EqualTo( 144 ) );
	}

	[Test]
	public void NextBytes_OneLessThanMultipleOf4_ValuesMatch() {
		byte[] buffer = new byte[7];
		_random.NextBytes( buffer );

		Assert.That( buffer[0], Is.EqualTo( 26 ) );
		Assert.That( buffer[1], Is.EqualTo( 144 ) );
		Assert.That( buffer[2], Is.EqualTo( 131 ) );
		Assert.That( buffer[3], Is.EqualTo( 222 ) );
		Assert.That( buffer[4], Is.EqualTo( 186 ) );
		Assert.That( buffer[5], Is.EqualTo( 117 ) );
		Assert.That( buffer[6], Is.EqualTo( 68 ) );
	}
}

