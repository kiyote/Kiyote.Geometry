using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class D3VoronoiFactoryTests {

	private IVoronoiFactory _voronoiFactory;

	[SetUp]
	public void SetUp() {
		_voronoiFactory = new D3VoronoiFactory();
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

		Rect bounds = new Rect( 0, 0, size );
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		Assert.That( voronoi, Is.Not.Null );
	}

	[Test]
	public void Create_FixedPoints_ReferenceDiagramCreated() {
		ISize size = new Point( 30, 30 );
		Rect bounds = new Rect( 0, 0, size );
		IReadOnlyList<Point> points = [
			new Point(10, 10),
			new Point(20, 10),
			new Point(10, 20),
			new Point(20, 20)
		];
		IVoronoi voronoi = _voronoiFactory.Create( bounds, points );

		Assert.That( voronoi, Is.Not.Null );
	}
}

