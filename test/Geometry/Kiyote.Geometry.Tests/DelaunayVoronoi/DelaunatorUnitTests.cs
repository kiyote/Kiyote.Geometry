namespace Kiyote.Geometry.DelaunayVoronoi.Tests;

[TestFixture]
public sealed class DelaunatorUnitTests {


	[TestCase( 0, 0, 1, 0, 0, 1, 0.5, 0.5 )]
	[TestCase( 25, 25, 75, 25, 75, 75, 50, 50 )]
	[TestCase( -25, -25, 25, -25, -25, 25, 0, 0 )]
	public void Circumcenter_ReferencePoints_CorrectValueReturned(
		double x1,
		double y1,
		double x2,
		double y2,
		double x3,
		double y3,
		double ex,
		double ey
	) {
		Delaunator.Circumcenter( x1, y1, x2, y2, x3, y3, out double x, out double y );
		Assert.AreEqual( ex, x );
		Assert.AreEqual( ey, y );
	}
}
