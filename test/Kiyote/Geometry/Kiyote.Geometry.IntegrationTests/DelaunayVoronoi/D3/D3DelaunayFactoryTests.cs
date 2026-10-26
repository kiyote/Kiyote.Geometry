using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

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

	[TestCase( 3 )]
	[TestCase( 32 )]
	[TestCase( 128 )]
	public void Create_AllHullSites_MaterializesOnlyLogicalTrianglesInMeshOrder( int count ) {
		Point[] sites = Enumerable.Range( 0, count ).Select( i => {
			double angle = i * 2 * Math.PI / count;
			return new Point( (int)Math.Round( Math.Cos( angle ) * 10000 ), (int)Math.Round( Math.Sin( angle ) * 10000 ) );
		} ).ToArray();
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( sites.ToCoords(), true );
		Assert.That( mesh.Hull.Length, Is.EqualTo( count ) );
		Assert.That( mesh.Hull, Is.All.InRange( 0, count - 1 ) );
		IDelaunay result = _delaunayFactory.Create( sites );
		Assert.That( result.Triangles.Count, Is.EqualTo( count - 2 ) );
		for( int i = 0; i < result.Triangles.Count; i++ ) {
			Triangle expected = new Triangle(
				sites[mesh.Triangles[i * 3]], sites[mesh.Triangles[(i * 3) + 1]], sites[mesh.Triangles[(i * 3) + 2]]
			);
			Assert.That( result.Triangles[i], Is.EqualTo( expected ) );
		}
		for( int i = 0; i < count; i++ ) {
			Assert.That( result.Hull[i], Is.EqualTo( sites[mesh.Hull[i]] ) );
		}
		Triangle[] triangles = result.Triangles.ToArray();
		Point[] hull = result.Hull.ToArray();
		sites.AsSpan().Fill( new Point( 0, 0 ) );
		Assert.That( () => _delaunayFactory.Create( sites ), Throws.ArgumentException );
		_ = _delaunayFactory.Create( [new( 0, 0 ), new( 10, 0 ), new( 0, 10 )] );
		Assert.That( result.Triangles, Is.EqualTo( triangles ) );
		Assert.That( result.Hull, Is.EqualTo( hull ) );
		Assert.That( () => ((IList<Triangle>)result.Triangles)[0] = default, Throws.TypeOf<NotSupportedException>() );
		Assert.That( () => ((IList<Point>)result.Hull)[0] = default, Throws.TypeOf<NotSupportedException>() );
	}
}
