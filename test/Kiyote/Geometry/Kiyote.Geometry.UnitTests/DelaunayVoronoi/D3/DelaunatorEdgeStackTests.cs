using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.UnitTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DelaunatorEdgeStackTests {
	[Test]
	public void Push_StackCapacityExceeded_PreservesAllPendingEdges() {
		Span<int> initial = stackalloc int[1];
		DelaunatorEdgeStack stack = new DelaunatorEdgeStack( initial );
		try {
			for( int i = 0; i < 4096; i++ ) {
				stack.Push( i );
			}
			for( int i = 4095; i >= 0; i-- ) {
				Assert.That( stack.Pop(), Is.EqualTo( i ) );
			}
			Assert.That( stack.Count, Is.Zero );
			stack.Push( 99 );
			Assert.That( stack.Pop(), Is.EqualTo( 99 ) );
		} finally {
			stack.Dispose();
		}
	}

	[Test]
	public void Push_InitialCapacitySufficient_DoesNotAllocate() {
		Span<int> initial = stackalloc int[512];
		DelaunatorEdgeStack stack = new DelaunatorEdgeStack( initial );
		try {
			long before = GC.GetAllocatedBytesForCurrentThread();
			for( int i = 0; i < initial.Length; i++ ) {
				stack.Push( i );
			}
			for( int i = 0; i < initial.Length; i++ ) {
				_ = stack.Pop();
			}
			long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			Assert.That( allocated, Is.Zero );
		} finally {
			stack.Dispose();
		}
	}
}
