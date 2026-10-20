using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class ChunkEntryIndexTests {
	[Test]
	public void Add_IncrementalChunks_UsesGeometricStorageGrowth() {
		ChunkEntryIndex index = new ChunkEntryIndex();
		long before = GC.GetAllocatedBytesForCurrentThread();
		for( int chunk = 0; chunk < 1024; chunk++ ) {
			index.Add( chunk, chunk + 1 );
		}
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( index.First( 1023 ), Is.EqualTo( 1023 ) );
		Assert.That( bytes, Is.LessThanOrEqualTo( 65536 ) );
	}

	[TestCase( 17 )]
	[TestCase( 44 )]
	[TestCase( 110 )]
	public void AddAndRemoveAt_SeededAppendAndSwapRemoval_PreserveEveryChunkChain( int seed ) {
		ChunkEntryIndex index = new ChunkEntryIndex();
		List<int> chunks = [];
		Random random = new Random( seed );
		for( int step = 0; step < 1500; step++ ) {
			if( chunks.Count == 0 || random.Next( 3 ) != 0 ) {
				int chunk = random.Next( 128 );
				index.Add( chunk, 128 );
				chunks.Add( chunk );
			} else {
				int removed = random.Next( chunks.Count );
				index.RemoveAt( removed, chunks[removed], chunks[^1] );
				chunks[removed] = chunks[^1];
				chunks.RemoveAt( chunks.Count - 1 );
			}
			if( step % 25 != 0 && step != 1499 ) {
				continue;
			}
			HashSet<int> seen = [];
			for( int chunk = 0; chunk < 128; chunk++ ) {
				for( int entry = index.First( chunk ); entry >= 0; entry = index.Next( entry ) ) {
					Assert.That( entry, Is.LessThan( chunks.Count ), $"Seed {seed}, step {step}" );
					Assert.That( seen.Add( entry ), Is.True, $"Cycle/duplicate: seed {seed}, step {step}" );
					Assert.That( chunks[entry], Is.EqualTo( chunk ), $"Seed {seed}, step {step}" );
				}
			}
			Assert.That( seen, Has.Count.EqualTo( chunks.Count ), $"Seed {seed}, step {step}" );
		}
	}
}
