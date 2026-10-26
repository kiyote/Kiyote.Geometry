using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.UnitTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DelaunatorEdgeStackTests {
	[Test]
	public void Grow_StackCapacityExceeded_PreservesAllPendingEdges() {
		Span<int> initial = stackalloc int[1];
		DelaunatorEdgeStack stack = new DelaunatorEdgeStack( initial );
		try {
			Span<int> pending = stack.Buffer;
			int count = 0;
			for( int i = 0; i < 4096; i++ ) {
				if( count == pending.Length ) {
					pending = stack.Grow();
				}
				pending[count++] = i;
			}
			for( int i = 4095; i >= 0; i-- ) {
				Assert.That( pending[--count], Is.EqualTo( i ) );
			}
			Assert.That( count, Is.Zero );
			Assert.That( stack.Buffer.Length, Is.GreaterThanOrEqualTo( 4096 ) );
			Assert.That( stack.Buffer[4095], Is.EqualTo( 4095 ) );
		} finally {
			stack.Dispose();
		}
	}

	[Test]
	public void Buffer_InitialCapacitySufficient_DoesNotAllocate() {
		Span<int> initial = stackalloc int[512];
		DelaunatorEdgeStack stack = new DelaunatorEdgeStack( initial );
		try {
			long before = GC.GetAllocatedBytesForCurrentThread();
			Span<int> pending = stack.Buffer;
			for( int i = 0; i < initial.Length; i++ ) {
				pending[i] = i;
			}
			long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			Assert.That( allocated, Is.Zero );
		} finally {
			stack.Dispose();
		}
	}
}
