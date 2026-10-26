using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class VoronoiHullRayTests {
	[TestCase( 0, 0 )]
	[TestCase( 10000, -10000 )]
	[TestCase( -32768, -32768 )]
	[TestCase( 32757, 32757 )]
	public void Create_AxisAlignedHullAtEveryPosition_ClipsToExpectedQuadrants( int x, int y ) {
		Point[] sites = [new( x + 2, y + 2 ), new( x + 8, y + 2 ), new( x + 2, y + 8 ), new( x + 8, y + 8 )];
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( x, y, 11, 11 ), sites );
		for( int i = 0; i < sites.Length; i++ ) {
			int left = x + (i % 2 == 0 ? 0 : 5);
			int top = y + (i < 2 ? 0 : 5);
			Polygon expected = new Polygon( [
				new Point( left, top ), new Point( left + 5, top ),
				new Point( left + 5, top + 5 ), new Point( left, top + 5 )
			] );
			Cell cell = diagram.Cells[i];
			Assert.That( cell.Polygon.IsEquivalentTo( expected ), Is.True );
			Assert.That( cell.IsOpen, Is.True );
			Assert.That( cell.BoundingBox, Is.EqualTo( new Rect( left, top, 6, 6 ) ) );
			Assert.That( cell.Center, Is.EqualTo( sites[i] ) );
		}
	}
}
