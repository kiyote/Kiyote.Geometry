using Kiyote.Geometry.Randomization;
using Moq;

namespace Kiyote.Geometry.Noises.UnitTests;

[TestFixture]
public sealed class MidpointDisplacementNoisyEdgeFactoryTests {

	private Mock<IRandom> _random;
	private INoisyEdgeFactory _factory;

	[SetUp]
	public void SetUp() {
		_random = new Mock<IRandom>( MockBehavior.Strict );
		_factory = new MidpointDisplacementNoisyEdgeFactory( _random.Object );
	}

	[TearDown]
	public void TearDown() {
		_random.VerifyAll();
	}

	[Test]
	public void Create_OneLevel_ExpectSplit() {
		Edge toSplit = new Edge( 0, 9, 9, 9 );
		Edge control = new Edge( 5, 0, 5, 19 );

		_random
			.Setup( r => r.NextFloat( 0.25f, 0.75f ) )
			.Returns( 0.5f );
		NoisyEdge result = _factory.Create( toSplit, control, 0.5f, 1 );

		Edge[] expected = [
				new Edge( 0, 9, 5, 9 ),
				new Edge( 5, 9, 9, 9 )
			];
		Assert.That( result.Source, Is.EqualTo( toSplit ) );
		for( int i = 0; i < expected.Length; i++ ) {
			bool equivalent = result.Noise[i].IsEquivalentTo( expected[i] );
			Assert.That( equivalent, Is.True );
		}
	}

	[Test]
	public void Create_TwoLevels_ExpectSplits() {
		Edge toSplit = new Edge( 0, 9, 9, 9 );
		Edge control = new Edge( 5, 0, 5, 19 );

		_random
			.Setup( r => r.NextFloat( 0.25f, 0.75f ) )
			.Returns( 0.5f );
		NoisyEdge result = _factory.Create( toSplit, control, 0.5f, 2 );

		Edge[] expected = [
				new Edge( 0, 9, 2, 9 ),
				new Edge( 2, 9, 5, 9 ),
				new Edge( 5, 9, 7, 9 ),
				new Edge( 7, 9, 9, 9 )
			];
		Assert.That( result.Source, Is.EqualTo( toSplit ) );
		for( int i = 0; i < expected.Length; i++ ) {
			Assert.That( result.Noise, Has.Member( expected[i] ) );
		}
	}
}
