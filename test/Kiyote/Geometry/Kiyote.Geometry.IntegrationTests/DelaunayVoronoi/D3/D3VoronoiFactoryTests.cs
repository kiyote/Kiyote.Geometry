using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

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

	[Test]
	public void Create_JitteredGrid_QuantizedCellsRemainConvexAndContainTheirSites() {
		Random random = new Random( 7291 );
		Point[] points = new Point[400];
		for( int row = 0; row < 20; row++ ) {
			for( int column = 0; column < 20; column++ ) {
				points[(row * 20) + column] = new Point(
					-475 + (column * 50) + random.Next( -10, 11 ),
					-475 + (row * 50) + random.Next( -10, 11 )
				);
			}
		}
		Rect bounds = new Rect( -500, -500, 1001, 1001 );
		IVoronoi diagram = _voronoiFactory.Create( bounds, points );
		Assert.That( diagram.Cells, Has.Count.EqualTo( points.Length ) );
		for( int cellIndex = 0; cellIndex < points.Length; cellIndex++ ) {
			Polygon polygon = diagram.Cells[cellIndex].Polygon;
			Assert.That( polygon.Points.Count, Is.GreaterThanOrEqualTo( 3 ), $"Cell {cellIndex}" );
			Assert.That( polygon.Contains( points[cellIndex] ), Is.True, $"Cell {cellIndex}" );
			int winding = 0;
			for( int i = 0; i < polygon.Points.Count; i++ ) {
				Point a = polygon.Points[i], b = polygon.Points[( i + 1 ) % polygon.Points.Count];
				Assert.That( bounds.Contains( a ), Is.True, $"Cell {cellIndex}, vertex {i}" );
				for( int j = 0; j < polygon.Points.Count; j++ ) {
					if( j == i || j == ( i + 1 ) % polygon.Points.Count ) {
						continue;
					}
					Point p = polygon.Points[j];
					long side = (( (long)b.X - a.X ) * ( p.Y - a.Y )) - (( (long)b.Y - a.Y ) * ( p.X - a.X ));
					Assert.That( side, Is.Not.Zero, $"Cell {cellIndex}, edge {i}, vertex {j}" );
					winding = winding == 0 ? Math.Sign( side ) : winding;
					Assert.That( Math.Sign( side ), Is.EqualTo( winding ), $"Cell {cellIndex}, edge {i}, vertex {j}" );
				}
			}
		}
	}
}
