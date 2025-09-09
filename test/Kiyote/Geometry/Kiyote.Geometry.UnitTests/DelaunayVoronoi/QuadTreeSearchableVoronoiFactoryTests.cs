using Kiyote.Geometry.Trees;
using Moq;

namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
public sealed class QuadTreeSearchableVoronoiFactoryTests {

	private Mock<IQuadTreeFactory> _treeFactory;
	private ISearchableVoronoiFactory _searchableFactory;

	[SetUp]
	public void SetUp() {
		_treeFactory = new Mock<IQuadTreeFactory>( MockBehavior.Strict );
		_searchableFactory = new QuadTreeSearchableVoronoiFactory(
			_treeFactory.Object
		);
	}

	[TearDown]
	public void TearDown() {
		_treeFactory.VerifyAll();
	}

	[Test]
	public void Create_ValidVoronoi_QuadTreeCreated() {
		IReadOnlyList<Cell> cells = [
			new Cell(
				new Point( 10, 10 ),
				new Polygon([
					new Point( 0, 0),
					new Point( 20, 0 ),
					new Point( 20, 20 ),
					new Point( 0, 20)
				]),
				false,
				new Rect(0, 0, 20, 20 )
			)
		];
		Mock<IVoronoi> voronoi = new Mock<IVoronoi>( MockBehavior.Strict );
		voronoi
			.Setup( v => v.Cells )
			.Returns( cells );
		int sizeWidth = 1;
		int sizeHeight = 2;
		Mock<ISize> size = new Mock<ISize>( MockBehavior.Strict );
		size
			.Setup( s => s.Width )
			.Returns( sizeWidth );
		size
			.Setup( s => s.Height )
			.Returns( sizeHeight );
		Mock<IQuadTree<Rect>> quadTree = new Mock<IQuadTree<Rect>>( MockBehavior.Strict );
		quadTree
			.Setup( qt => qt.Insert( It.IsAny<Rect>() ) )
			.Callback<Rect>( ( rect ) => {
				Assert.That( rect.X1, Is.EqualTo( 0 ) );
				Assert.That( rect.Y1, Is.EqualTo( 0 ) );
				Assert.That( rect.X2, Is.EqualTo( 19 ) );
				Assert.That( rect.Y2, Is.EqualTo( 19 ) );
			} );
		_treeFactory
			.Setup( tf => tf.Create<Rect>( It.IsAny<Rect>() ) )
			.Callback<IRect>( ( rect ) => {
				Assert.That( rect.Width, Is.EqualTo( sizeWidth ) );
				Assert.That( rect.Height, Is.EqualTo( sizeHeight ) );
			} )
			.Returns( quadTree.Object );

		ISearchableVoronoi searchableVoronoi = _searchableFactory.Create(
			voronoi.Object,
			size.Object
		);

		Assert.That( searchableVoronoi, Is.Not.Null );
		quadTree.VerifyAll();
		voronoi.VerifyAll();
		size.VerifyAll();
	}
}
