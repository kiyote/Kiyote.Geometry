using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.UnitTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DelaunayIndexTests {
	[Test]
	public void BuildIndices_DirtySlicedBuffers_InitializesLogicalRangeAndPrefersHullEdges() {
		Point[] sites = [new( -10, -10 ), new( 10, -10 ), new( 10, 10 ), new( -10, 10 ), new( 0, 0 )];
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( sites.ToCoords(), true );
		int[] inedges = Enumerable.Repeat( 7291, sites.Length + 2 ).ToArray();
		int[] hullIndex = Enumerable.Repeat( 7291, sites.Length + 2 ).ToArray();
		D3DelaunayFactory.BuildIndices( mesh, inedges.AsSpan( 1, sites.Length ), hullIndex.AsSpan( 1, sites.Length ) );
		for( int i = 0; i < sites.Length; i++ ) {
			int edge = inedges[i + 1];
			int next = edge % 3 == 2 ? edge - 2 : edge + 1;
			Assert.That( mesh.Triangles[next], Is.EqualTo( i ) );
			int position = Array.IndexOf( mesh.Hull, i );
			Assert.That( hullIndex[i + 1], Is.EqualTo( position ) );
			if( position >= 0 ) {
				Assert.That( mesh.HalfEdges[edge], Is.EqualTo( -1 ) );
			}
		}
		Assert.That( inedges[0], Is.EqualTo( 7291 ) );
		Assert.That( inedges[^1], Is.EqualTo( 7291 ) );
		Assert.That( hullIndex[0], Is.EqualTo( 7291 ) );
		Assert.That( hullIndex[^1], Is.EqualTo( 7291 ) );
	}

	[Test]
	public void BuildIndices_WarmCallerOwnedBuffers_DoesNotAllocate() {
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( [-10, -10, 10, -10, 0, 10], true );
		Span<int> inedges = stackalloc int[3];
		Span<int> hullIndex = stackalloc int[3];
		D3DelaunayFactory.BuildIndices( mesh, inedges, hullIndex );
		long before = GC.GetAllocatedBytesForCurrentThread();
		for( int i = 0; i < 100; i++ ) {
			D3DelaunayFactory.BuildIndices( mesh, inedges, hullIndex );
		}
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( allocated, Is.Zero );
	}

	[Test]
	public void BuildIndices_MismatchedIndexLengths_RejectsExplicitly() {
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( [-10, -10, 10, -10, 0, 10], true );
		Assert.That( () => D3DelaunayFactory.BuildIndices( mesh, new int[3], new int[4] ), Throws.ArgumentException );
	}
}
