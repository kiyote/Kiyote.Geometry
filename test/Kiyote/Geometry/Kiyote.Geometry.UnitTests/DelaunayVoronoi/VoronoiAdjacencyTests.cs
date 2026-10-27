using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class VoronoiAdjacencyTests {
	[TestCase( -1 )]
	[TestCase( 3 )]
	[TestCase( int.MaxValue )]
	public void GetNeighbourIndices_InvalidIndex_ThrowsArgumentOutOfRangeException( int index ) {
		IVoronoi diagram = CreateTriangle();
		Assert.That( () => { _ = diagram.GetNeighbourIndices( index ); },
			Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void GetNeighbourIndices_RepeatedTraversal_DoesNotAllocate() {
		IVoronoi diagram = CreateTriangle();
		for( int i = 0; i < diagram.Cells.Length; i++ ) {
			_ = diagram.GetNeighbourIndices( i );
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		int sum = 0;
		for( int pass = 0; pass < 100; pass++ ) {
			for( int i = 0; i < diagram.Cells.Length; i++ ) {
				sum += diagram.GetNeighbourIndices( i ).Length;
			}
		}
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( sum, Is.EqualTo( 600 ) );
		Assert.That( allocated, Is.Zero );
	}

	[Test]
	public void GetNeighbourIndices_Grid_EqualsPrimalMeshAdjacency() {
		Point[] sites = new Point[100];
		for( int i = 0; i < sites.Length; i++ ) {
			sites[i] = new Point( 2 + (i % 10 * 5), 2 + (i / 10 * 5) );
		}
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( sites.ToCoords(), true );
		HashSet<int>[] expected = Enumerable.Range( 0, sites.Length ).Select( _ => new HashSet<int>() ).ToArray();
		ReadOnlySpan<int> triangles = mesh.Triangles;
		for( int e = 0; e < triangles.Length; e++ ) {
			int next = e % 3 == 2 ? e - 2 : e + 1;
			expected[triangles[e]].Add( triangles[next] );
			expected[triangles[next]].Add( triangles[e] );
		}
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 51, 51 ), sites );
		for( int i = 0; i < sites.Length; i++ ) {
			Assert.That( diagram.GetNeighbourIndices( i ).ToArray(), Is.EquivalentTo( expected[i] ) );
		}
	}

	private static IVoronoi CreateTriangle() {
		IVoronoiFactory factory = new D3VoronoiFactory();
		return factory.Create( new Rect( -20, -20, 41, 41 ), [new( -10, -10 ), new( 10, -10 ), new( 0, 10 )] );
	}
}
