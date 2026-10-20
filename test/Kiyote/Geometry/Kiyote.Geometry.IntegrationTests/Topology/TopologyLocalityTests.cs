namespace Kiyote.Geometry.Topology.IntegrationTests;

[TestFixture]
internal sealed class TopologyLocalityTests {
	[Test]
	public void EditAndCommit_WarmedLocalChanges_AllocateNothing() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		Attach( assembly, 32, 2, 0, 0, false );
		Attach( assembly, 32, 2, 32, 0, false );
		using ICompiledGridAssembly<int> compiled = new GridCompiler().Compile( assembly, 32 );
		IGridLayer<int> layer = compiled.Bind<int, IntBinding>( default );
		PlacementId placement = assembly.Placements[0].Id;
		for( int i = 0; i < 100; i++ ) {
			Edit();
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		int successes = 0;
		for( int i = 0; i < 100; i++ ) {
			if( Edit() ) {
				successes++;
			}
		}
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( successes, Is.EqualTo( 100 ) );
		Assert.That( bytes, Is.Zero );

		bool Edit() {
			bool removed = assembly.TryRemoveCell( placement, 31, 0 );
			bool added = assembly.TryAddCell( placement, 31, 0, 1 );
			layer.MarkDirty( 0 );
			compiled.Commit();
			return removed && added;
		}
	}

	[TestCase( 0 )]
	[TestCase( 1 )]
	[TestCase( 2 )]
	public void Commit_SelectedDirtyChunks_WritesEachOccupiedCellExactlyOnce( int mode ) {
		GridAssembly<ValueCell> assembly = CreateValueAssembly();
		using ICompiledGridAssembly<ValueCell> compiled = new GridCompiler().Compile( assembly, 32 );
		Recorder recorder = new Recorder();
		IGridLayer<int> layer = compiled.Bind<int, RecordingBinding>( new RecordingBinding( recorder ) );
		HashSet<int> expected = [];
		foreach( int chunk in compiled.ChunkLayout.EnumerateChunks() ) {
			layer.GetValues( chunk ).Fill( 99 );
			if( mode == 2 || ( mode == 1 && chunk == 0 ) ) {
				layer.MarkDirty( chunk );
				layer.MarkDirty( chunk );
			}
		}
		foreach( IGridPlacement<ValueCell> placement in assembly.Placements ) {
			for( int y = 0; y < placement.Source.Height; y++ ) {
				for( int x = 0; x < placement.Source.Width; x++ ) {
					if( !placement.Source.IsOccupied( x, y ) ) {
						continue;
					}
					compiled.ChunkLayout.TryLocate( x + placement.Column, y + placement.Row, out int chunk, out _ );
					if( layer.IsDirty( chunk ) ) {
						expected.Add( placement.Source.GetCell( x, y ).Id );
					}
				}
			}
		}
		compiled.Commit();
		Assert.That( recorder.Written, Is.EquivalentTo( expected ) );
		foreach( int chunk in compiled.ChunkLayout.EnumerateChunks() ) {
			Assert.That( layer.IsDirty( chunk ), Is.False );
		}
		recorder.Written.Clear();
		compiled.Commit();
		Assert.That( recorder.Written, Is.Empty );
	}

	[Test]
	public void Commit_SwappedAndClearedDirtyState_FollowsLayerStorage() {
		GridAssembly<ValueCell> assembly = CreateValueAssembly();
		using ICompiledGridAssembly<ValueCell> compiled = new GridCompiler().Compile( assembly, 32 );
		Recorder recorder = new Recorder();
		IGridLayer<int> bound = compiled.Bind<int, RecordingBinding>( new RecordingBinding( recorder ) );
		IGridLayer<int> scratch = compiled.CreateLayer<int>();
		scratch.MarkDirty( 1 );
		scratch.ClearDirty();
		scratch.MarkDirty( 0 );
		scratch.MarkDirty( 0 );
		scratch.GetValues( 0 ).Fill( 27 );
		bound.MarkDirty( 1 );
		compiled.Swap( bound, scratch );
		Assert.That( scratch.IsDirty( 1 ), Is.True );
		Assert.That( bound.IsDirty( 1 ), Is.False );
		compiled.Commit();
		Assert.That( recorder.Written, Has.Count.EqualTo( 61 ) );
		Assert.That( recorder.Written.Distinct().Count(), Is.EqualTo( 61 ) );
		Assert.That( assembly.Placements[0].Source.GetCell( 0, 0 ).Value, Is.EqualTo( 27 ) );
		Assert.That( scratch.IsDirty( 1 ), Is.True );
		Assert.That( bound.IsDirty( 0 ), Is.False );
		bound.MarkDirty( 0 );
		bound.ClearDirty();
		recorder.Written.Clear();
		compiled.Commit();
		Assert.That( recorder.Written, Is.Empty );
	}

	[Test]
	public void Commit_BindingThrows_RetainsDirtyChunksForRetry() {
		GridAssembly<ValueCell> assembly = CreateValueAssembly();
		using ICompiledGridAssembly<ValueCell> compiled = new GridCompiler().Compile( assembly, 32 );
		Recorder recorder = new Recorder { ThrowOnId = 1 };
		IGridLayer<int> layer = compiled.Bind<int, RecordingBinding>( new RecordingBinding( recorder ) );
		foreach( int chunk in compiled.ChunkLayout.EnumerateChunks() ) {
			layer.MarkDirty( chunk );
		}
		Assert.Throws<InvalidOperationException>( compiled.Commit );
		foreach( int chunk in compiled.ChunkLayout.EnumerateChunks() ) {
			Assert.That( layer.IsDirty( chunk ), Is.True );
		}
		recorder.ThrowOnId = null;
		recorder.Written.Clear();
		compiled.Commit();
		Assert.That( recorder.Written, Has.Count.EqualTo( 77 ) );
		Assert.That( recorder.Written.Distinct().Count(), Is.EqualTo( 77 ) );
	}

	[TestCase( 19 )]
	[TestCase( 71 )]
	[TestCase( 137 )]
	public void Edit_SeededSplitsMergesSeamsAndNewChunks_MatchesFreshCompilation( int seed ) {
		GridAssembly<int> assembly = new GridAssembly<int>();
		Attach( assembly, 40, 4, -20, -2, false );
		Attach( assembly, 16, 4, 20, -2, false );
		Attach( assembly, 128, 3, 200, 100, true );
		using ICompiledGridAssembly<int> compiled = new GridCompiler().Compile( assembly, 32 );
		IGridLayer<int> layer = compiled.Bind<int, IntBinding>( default );
		Random random = new Random( seed );
		for( int step = 0; step < 200; step++ ) {
			IGridPlacement<int> placement = assembly.Placements[random.Next( assembly.Placements.Count )];
			int x = random.Next( placement.Source.Width );
			int y = random.Next( placement.Source.Height );
			int worldX = x + placement.Column;
			int worldY = y + placement.Row;
			if( placement.Source.IsOccupied( x, y ) ) {
				compiled.ChunkLayout.TryLocate( worldX, worldY, out int chunk, out int local );
				Assert.That( assembly.TryRemoveCell( placement.Id, x, y ), Is.True, $"Seed {seed}, step {step}" );
				Assert.That( layer.GetValues( chunk )[local], Is.Zero );
			} else {
				Assert.That( assembly.TryAddCell( placement.Id, x, y, step + 1 ), Is.True, $"Seed {seed}, step {step}" );
				Assert.That( layer[worldX, worldY], Is.EqualTo( step + 1 ) );
			}
			if( step % 10 == 0 ) {
				AssertMatchesFresh( assembly, compiled, $"Seed {seed}, step {step}" );
			}
		}
		AssertMatchesFresh( assembly, compiled, $"Seed {seed}, final" );
	}

	[Test]
	public void Edit_RemoveAllAndRepopulate_ReusesEmptyChunkIndexes() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		Attach( assembly, 5, 3, 0, 0, false );
		Attach( assembly, 5, 3, 5, 0, false );
		using ICompiledGridAssembly<int> compiled = new GridCompiler().Compile( assembly, 32 );
		IGridLayer<int> layer = compiled.Bind<int, IntBinding>( default );
		for( int iteration = 0; iteration < 3; iteration++ ) {
			foreach( IGridPlacement<int> placement in assembly.Placements ) {
				for( int y = 0; y < 3; y++ ) {
					for( int x = 0; x < 5; x++ ) {
						Assert.That( assembly.TryRemoveCell( placement.Id, x, y ), Is.True );
					}
				}
			}
			Assert.That( compiled.SourceRuns.Length, Is.Zero );
			Assert.That( compiled.SeamLinks.Length, Is.Zero );
			Assert.That( assembly.Seams, Is.Empty );
			layer.MarkDirty( 0 );
			compiled.Commit();
			Assert.That( layer.IsDirty( 0 ), Is.False );
			foreach( IGridPlacement<int> placement in assembly.Placements.Reverse() ) {
				for( int y = 2; y >= 0; y-- ) {
					for( int x = 4; x >= 0; x-- ) {
						Assert.That( assembly.TryAddCell( placement.Id, x, y, 1 ), Is.True );
					}
				}
			}
			AssertMatchesFresh( assembly, compiled, $"Iteration {iteration}" );
		}
	}

	private static void AssertMatchesFresh( GridAssembly<int> assembly, ICompiledGridAssembly<int> compiled, string context ) {
		GridAssembly<int> copy = new GridAssembly<int>();
		foreach( IGridPlacement<int> placement in assembly.Placements ) {
			DenseGridSource<int> source = new DenseGridSource<int>( placement.Source.Width, placement.Source.Height );
			for( int y = 0; y < source.Height; y++ ) {
				for( int x = 0; x < source.Width; x++ ) {
					if( placement.Source.IsOccupied( x, y ) ) {
						source.TrySetCell( x, y, placement.Source.GetCell( x, y ) );
					}
				}
			}
			copy.TryAttach( source, placement.Id, placement.Column, placement.Row );
		}
		using ICompiledGridAssembly<int> fresh = new GridCompiler().Compile( copy, 32 );
		Assert.That( CanonicalRuns( compiled ), Is.EquivalentTo( CanonicalRuns( fresh ) ), context );
		Assert.That( CanonicalLinks( compiled ), Is.EquivalentTo( CanonicalLinks( fresh ) ), context );
		Assert.That( assembly.Seams.SelectMany( seam => seam.Contacts ),
			Is.EquivalentTo( copy.Seams.SelectMany( seam => seam.Contacts ) ), context );
	}

	private static IEnumerable<(PlacementId, int, int, int, Point)> CanonicalRuns( ICompiledGridAssembly<int> compiled ) =>
		compiled.SourceRuns.ToArray().Select( run => (run.Placement, run.SourceColumn, run.SourceRow, run.Length,
			Location( compiled.ChunkLayout, run.ChunkIndex, run.LocalIndex )) );

	private static IEnumerable<(Point, Point, Direction)> CanonicalLinks( ICompiledGridAssembly<int> compiled ) =>
		compiled.SeamLinks.ToArray().Select( link => (Location( compiled.ChunkLayout, link.ChunkIndex, link.LocalIndex ),
			Location( compiled.ChunkLayout, link.NeighbourChunkIndex, link.NeighbourLocalIndex ), link.Direction) );

	private static Point Location( GridLayout layout, int chunk, int local ) =>
		new Point( layout.GetChunkColumn( chunk ) * 32 + local % 32, layout.GetChunkRow( chunk ) * 32 + local / 32 );

	private static void Attach( GridAssembly<int> assembly, int width, int height, int x, int y, bool sparse ) {
		DenseGridSource<int> source = new DenseGridSource<int>( width, height );
		for( int row = 0; row < height; row++ ) {
			for( int column = 0; column < ( sparse ? 1 : width ); column++ ) {
				source.TrySetCell( column, row, 1 );
			}
		}
		Assert.That( assembly.TryAttach( source, x, y ).Succeeded, Is.True );
	}

	private static GridAssembly<ValueCell> CreateValueAssembly() {
		GridAssembly<ValueCell> assembly = new GridAssembly<ValueCell>();
		for( int placement = 0; placement < 2; placement++ ) {
			DenseGridSource<ValueCell> source = new DenseGridSource<ValueCell>( 20, 2 );
			for( int y = 0; y < 2; y++ ) {
				for( int x = 0; x < 20; x++ ) {
					int id = placement * 20 + x + y * 40;
					if( id is not ( 5 or 23 or 45 ) ) {
						source.TrySetCell( x, y, new ValueCell( id, 1 ) );
					}
				}
			}
			assembly.TryAttach( source, placement * 20, 0 );
		}
		return assembly;
	}

	private record struct ValueCell( int Id, int Value );

	private sealed class Recorder {
		public List<int> Written { get; } = [];
		public int? ThrowOnId { get; set; }
	}

	private readonly struct RecordingBinding( Recorder recorder ) : IGridLayerBinding<ValueCell, int> {
		public int Extract( in ValueCell cell ) => cell.Value;
		public void Commit( ref ValueCell cell, int value ) {
			if( cell.Id == recorder.ThrowOnId ) {
				throw new InvalidOperationException( "Expected binding failure." );
			}
			recorder.Written.Add( cell.Id );
			cell.Value = value;
		}
	}

	private readonly struct IntBinding : IGridLayerBinding<int, int> {
		public int Extract( in int cell ) => cell;
		public void Commit( ref int cell, int value ) => cell = value;
	}
}
