using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class D3DelaunayFactoryTests {

	private IDelaunayFactory _delaunayFactory;

	[SetUp]
	public void SetUp() {
		_delaunayFactory = new D3DelaunayFactory();
	}

	[Test]
	public void Create_HappyPath_DelaunayCreated() {
		ISize size = new Point( 1000, 1000 );
		int cellWidth = size.Width / 20;
		int cellHeight = size.Height / 20;

		List<Point> points = [];
		for( int c = cellWidth / 2; c < size.Width; c += cellWidth ) {
			for( int r = cellHeight / 2; r < size.Height; r += cellHeight ) {
				points.Add( new Point( c, r ) );
			}
		}

		IDelaunay delaunay = _delaunayFactory.Create( points );

		Assert.That( delaunay, Is.Not.Null );
	}
}
