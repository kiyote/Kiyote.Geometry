namespace Kiyote.Geometry.Trees.UnitTests;

[TestFixture]
public sealed class SimpleQuadTreeFactoryTests {

	private IQuadTreeFactory _factory;

	[SetUp]
	public void SetUp() {
		_factory = new SimpleQuadTreeFactory();
	}

	[Test]
	public void Create_ValidArea_TreeReturned() {
		Rect r = new Rect( 10, 10, 10, 10 );
		Assert.DoesNotThrow( () => _factory.Create<Rect>( r ) );
	}
}
