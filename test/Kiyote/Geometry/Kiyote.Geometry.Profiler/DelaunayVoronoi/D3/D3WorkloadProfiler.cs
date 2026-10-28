using System.Diagnostics;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Testing;

namespace Kiyote.Geometry.Profiler.DelaunayVoronoi.D3;

internal static class D3WorkloadProfiler {
	internal static void Profile( string workload, int iterations ) {
		D3WorkloadFixture fixture = D3WorkloadFixture.Create( workload );
		IVoronoiFactory factory = new D3VoronoiFactory();
		fixture.Validate( factory.Create( fixture.Bounds, fixture.Input, fixture.Sanitize ) );
		for( int i = 0; i < 20; i++ ) {
			GC.KeepAlive( factory.Create( fixture.Bounds, fixture.Input, fixture.Sanitize ) );
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		long before = GC.GetAllocatedBytesForCurrentThread();
		for( int i = 0; i < iterations; i++ ) {
			GC.KeepAlive( factory.Create( fixture.Bounds, fixture.Input, fixture.Sanitize ) );
		}
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		stopwatch.Stop();
		Console.WriteLine( $"{workload}: inputs={fixture.Input.Length}, sites={fixture.Sites.Length}, iterations={iterations}, allocated={allocated / iterations} B/op, elapsed={stopwatch.Elapsed}" );
	}
}
