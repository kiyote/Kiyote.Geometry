using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DelaunatorInvariantTests {
	[TestCase( 0 )]
	[TestCase( 1 )]
	[TestCase( 2 )]
	public void Create_TooFewSites_ThrowsExplicitDegeneracy( int count ) {
		Assert.That( () => MapboxDelaunatorFactory.Create( new double[count * 2] ),
			Throws.TypeOf<InvalidOperationException>() );
	}

	[Test]
	public void Create_UnpairedCoordinates_ThrowsArgumentException() {
		Assert.That( () => MapboxDelaunatorFactory.Create( new double[7] ),
			Throws.TypeOf<ArgumentException>() );
	}

	[Test]
	public void Create_CoincidentOrCollinearSites_ThrowsExplicitDegeneracy() {
		Assert.That( () => MapboxDelaunatorFactory.Create( [1, 1, 1, 1, 1, 1] ),
			Throws.TypeOf<InvalidOperationException>() );
		Assert.That( () => MapboxDelaunatorFactory.Create( [0, 0, 1, 1, 2, 2] ),
			Throws.TypeOf<InvalidOperationException>() );
	}

	[Test]
	public void Create_TranslatedAnisotropicSites_PreservesSeedAndTopology() {
		Point[] sites = [
			new( 0, 0 ), new( 100, 0 ), new( 100, 10 ), new( 0, 10 ),
			new( 51, 5 ), new( 20, 3 ), new( 80, 7 )
		];
		Point[] translated = sites.Select( p => p + new Point( 10000, -10000 ) ).ToArray();
		MapboxDelaunator first = MapboxDelaunatorFactory.Create( sites.ToCoords(), true );
		MapboxDelaunator second = MapboxDelaunatorFactory.Create( translated.ToCoords(), true );
		Assert.That( second.Triangles.ToArray(), Is.EqualTo( first.Triangles.ToArray() ) );
		Assert.That( second.HalfEdges.ToArray(), Is.EqualTo( first.HalfEdges.ToArray() ) );
		Assert.That( second.Hull, Is.EqualTo( first.Hull ) );
	}

	[TestCase( 5291 )]
	[TestCase( 7291 )]
	[TestCase( 8129 )]
	public void Create_SeededFullDomainSites_SatisfiesExactMeshInvariants( int seed ) {
		Random random = new Random( seed );
		HashSet<Point> distinct = [];
		while( distinct.Count < 400 ) {
			distinct.Add( new Point( random.Next( -32768, 32768 ), random.Next( -32768, 32768 ) ) );
		}
		ValidateMesh( distinct.ToArray() );
	}

	[Test]
	public void Create_CocircularAndNearCollinearSites_SatisfiesExactMeshInvariants() {
		ValidateMesh( [
			new( -32768, -32768 ), new( 32767, -32768 ), new( 32767, 32767 ), new( -32768, 32767 ),
			new( 0, 0 ), new( 1, 0 ), new( 32766, 32765 ), new( -32767, -32766 )
		] );
		Point[] grid = new Point[400];
		for( int i = 0; i < grid.Length; i++ ) {
			grid[i] = new Point( (i % 20) * 100, (i / 20) * 100 );
		}
		ValidateMesh( grid );
	}

	private static void ValidateMesh( Point[] sites ) {
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( sites.ToCoords(), true );
		ReadOnlySpan<int> triangles = mesh.Triangles;
		ReadOnlySpan<int> halfEdges = mesh.HalfEdges;
		bool[] used = new bool[sites.Length];
		HashSet<Edge> boundary = [];
		Assert.That( triangles.Length, Is.EqualTo( 3 * ( (2 * sites.Length) - 2 - mesh.Hull.Length ) ) );
		for( int e = 0; e < triangles.Length; e++ ) {
			int index = triangles[e];
			Assert.That( index, Is.InRange( 0, sites.Length - 1 ) );
			used[index] = true;
			int next = e % 3 == 2 ? e - 2 : e + 1;
			int opposite = halfEdges[e];
			if( opposite == -1 ) {
				boundary.Add( new Edge( sites[index], sites[triangles[next]] ) );
			} else {
				Assert.That( opposite, Is.InRange( 0, halfEdges.Length - 1 ) );
				Assert.That( halfEdges[opposite], Is.EqualTo( e ) );
				int oppositeNext = opposite % 3 == 2 ? opposite - 2 : opposite + 1;
				Assert.That( triangles[opposite], Is.EqualTo( triangles[next] ) );
				Assert.That( triangles[oppositeNext], Is.EqualTo( index ) );
				if( e < opposite ) {
					int start = e - (e % 3);
					int oppositePrevious = opposite % 3 == 0 ? opposite + 2 : opposite - 1;
					Int128 determinant = InCircle( sites[triangles[start]], sites[triangles[start + 1]],
						sites[triangles[start + 2]], sites[triangles[oppositePrevious]] );
					Assert.That( determinant >= 0, Is.True, $"Illegal edge {e}, determinant {determinant}" );
				}
			}
			if( e % 3 == 0 ) {
				Point a = sites[index], b = sites[triangles[e + 1]], c = sites[triangles[e + 2]];
				long winding = ( ((long)b.X - a.X) * (c.Y - a.Y) ) - ( ((long)b.Y - a.Y) * (c.X - a.X) );
				Assert.That( winding, Is.LessThan( 0 ) );
			}
		}
		Assert.That( used, Is.All.True );
		Assert.That( boundary, Has.Count.EqualTo( mesh.Hull.Length ) );
		for( int i = 0; i < mesh.Hull.Length; i++ ) {
			Assert.That( boundary, Does.Contain( new Edge( sites[mesh.Hull[i]],
				sites[mesh.Hull[(i + 1) % mesh.Hull.Length]] ) ) );
		}
	}

	private static Int128 InCircle( Point a, Point b, Point c, Point p ) {
		Int128 ax = (long)a.X - p.X, ay = (long)a.Y - p.Y;
		Int128 bx = (long)b.X - p.X, by = (long)b.Y - p.Y;
		Int128 cx = (long)c.X - p.X, cy = (long)c.Y - p.Y;
		Int128 aSquared = (ax * ax) + (ay * ay);
		Int128 bSquared = (bx * bx) + (by * by);
		Int128 cSquared = (cx * cx) + (cy * cy);
		// Bounded deltas are < 2^16, so the absolute determinant is < 2^69.
		return (ax * ( (by * cSquared) - (bSquared * cy) ))
			- (ay * ( (bx * cSquared) - (bSquared * cx) ))
			+ (aSquared * ( (bx * cy) - (by * cx) ));
	}
}
