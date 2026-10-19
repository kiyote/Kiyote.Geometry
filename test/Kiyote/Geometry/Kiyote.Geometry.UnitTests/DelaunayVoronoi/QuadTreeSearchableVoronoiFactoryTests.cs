using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.Trees;
using Moq;

namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
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
		int sizeWidth = 20;
		int sizeHeight = 21;
		Mock<ISize> size = new Mock<ISize>( MockBehavior.Strict );
		size
			.Setup( s => s.Width )
			.Returns( sizeWidth );
		size
			.Setup( s => s.Height )
			.Returns( sizeHeight );
		IQuadTree<IndexedCell> quadTree = new SimpleQuadTree<IndexedCell>( new Rect( 0, 0, sizeWidth, sizeHeight ) );
		_treeFactory
			.Setup( tf => tf.Create<IndexedCell>( It.IsAny<Rect>() ) )
			.Callback<IRect>( ( rect ) => {
				Assert.That( rect.Width, Is.EqualTo( sizeWidth ) );
				Assert.That( rect.Height, Is.EqualTo( sizeHeight ) );
			} )
			.Returns( quadTree );

		ISearchableVoronoi searchableVoronoi = _searchableFactory.Create(
			voronoi.Object,
			size.Object
		);

		Assert.That( searchableVoronoi, Is.Not.Null );
		IReadOnlyList<IndexedCell> indexed = quadTree.Query( new Rect( 0, 0, sizeWidth, sizeHeight ) );
		Assert.That( indexed, Has.Count.EqualTo( 1 ) );
		Assert.That( indexed[0].Cell, Is.SameAs( cells[0] ) );
		voronoi.VerifyAll();
		size.VerifyAll();
	}
}
