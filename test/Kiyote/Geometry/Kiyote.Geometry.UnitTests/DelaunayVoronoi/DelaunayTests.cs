namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
public sealed class DelaunayTests {

	[Test]
	public void Ctor_ValidParameters_PropertiesAssigned() {
		IReadOnlyList<Point> points = [
			new Point(1, 1)
		];
		IReadOnlyList<Triangle> triangles = [
			new Triangle(
				new Point( 2, 2 ),
				new Point( 3, 3 ),
				new Point( 4, 4 )
			)
		];
		IReadOnlyList<Point> hull = [
			new Point( 5, 5 )
		];

		Delaunay delaunay = new Delaunay(
			points,
			triangles,
			hull
		);

		Assert.That( delaunay.Points, Is.SameAs( points ) );
		Assert.That( delaunay.Triangles, Is.SameAs( triangles ) );
		Assert.That( delaunay.Hull, Is.SameAs( hull ) );
	}
}
