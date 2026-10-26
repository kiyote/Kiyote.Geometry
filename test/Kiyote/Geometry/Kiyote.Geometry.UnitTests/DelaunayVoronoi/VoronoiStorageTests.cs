using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class VoronoiStorageTests {
	[Test]
	public void Create_GridWithWarmScratch_StaysWithinFullDiagramAllocationBudget() {
		Point[] sites = Grid();
		Rect bounds = new Rect( 0, 0, 100, 100 );
		IVoronoiFactory factory = new D3VoronoiFactory();
		for( int i = 0; i < 10; i++ ) {
			GC.KeepAlive( factory.Create( bounds, sites ) );
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		IVoronoi result = factory.Create( bounds, sites );
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		GC.KeepAlive( result );
		Assert.That( result.Cells.Count, Is.EqualTo( sites.Length ) );
		Assert.That( allocated, Is.LessThanOrEqualTo( 240 * 1024 ) );
	}

	[Test]
	public void Create_ConcurrentReuseAndFailures_PreserveOriginalDiagramStorage() {
		Point[] sites = Grid();
		Rect bounds = new Rect( 0, 0, 100, 100 );
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi original = factory.Create( bounds, sites );
		Point[][] polygons = original.Cells.Select( cell => cell.Polygon.Points.ToArray() ).ToArray();
		int[][] neighbours = Enumerable.Range( 0, sites.Length )
			.Select( i => original.GetNeighbourIndices( i ).ToArray() ).ToArray();
		Edge[] edges = original.Edges.ToArray();
		Point[] duplicates = [.. sites, sites[0]];
		Parallel.For( 0, 16, _ => {
			Assert.That( () => factory.Create( bounds, duplicates ), Throws.ArgumentException );
			IVoronoi next = factory.Create( bounds, sites );
			Assert.That( next.Cells.Count, Is.EqualTo( sites.Length ) );
		} );
		Assert.That( original.Points, Is.EqualTo( sites ) );
		Assert.That( original.Edges, Is.EqualTo( edges ) );
		for( int i = 0; i < sites.Length; i++ ) {
			Assert.That( original.Cells[i].Polygon.Points, Is.EqualTo( polygons[i] ) );
			Assert.That( original.GetNeighbourIndices( i ).ToArray(), Is.EqualTo( neighbours[i] ) );
		}
	}

	private static Point[] Grid() {
		Point[] sites = new Point[400];
		for( int i = 0; i < sites.Length; i++ ) {
			sites[i] = new Point( 2 + (i % 20 * 5), 2 + (i / 20 * 5) );
		}
		return sites;
	}
}
