using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Geometry.Topology.IntegrationTests;

[TestFixture]
internal sealed class CompiledGridAssemblyTests {

	private IGridCompiler _compiler;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		_compiler = new GridCompiler();
	}

	[Test]
	public void TryAddCell_BoundLayer_LayerUpdated() {
		DenseGridSource<TestCell> source = AsciiMapSource.Create( AsciiMapSource.Map1 );
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		PlacementId placement = new PlacementId( 1 );
		_ = assembly.TryAttach( source, placement, 0, 0 );

		ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<bool> walkability = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding() );

		Assert.That( () => walkability[20, 1], Throws.TypeOf<ArgumentOutOfRangeException>() );
		compiledAssembly.Assembly.TryAddCell( placement, 20, 1, new TestCell( true, false, false ) );
		Assert.That( walkability[20, 1], Is.True );
	}

	[Test]
	public void TryAddCell_NewChunk_BoundLayerLoadedAndDirtyTracked() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 128, 1, out PlacementId placement );
		for( int column = 64; column < 128; column++ ) {
			Assert.That( assembly.TryRemoveCell( placement, column, 0 ), Is.True );
		}
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<bool> walkability = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding() );
		Assert.That( compiledAssembly.ChunkLayout.ChunkCount, Is.EqualTo( 2 ) );

		Assert.That( assembly.TryAddCell( placement, 100, 0, new TestCell( true, false, false ) ), Is.True );

		Assert.That( compiledAssembly.ChunkLayout.TryLocate( 100, 0, out int chunkIndex, out _ ), Is.True );
		walkability.MarkDirty( chunkIndex );
		using( Assert.EnterMultipleScope() ) {
			Assert.That( compiledAssembly.ChunkLayout.ChunkCount, Is.EqualTo( 3 ) );
			Assert.That( walkability[100, 0], Is.True );
			Assert.That( walkability.IsDirty( chunkIndex ), Is.True );
		}
	}

	[Test]
	public void MarkDirty_UnallocatedChunk_Throws() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> layer = compiledAssembly.CreateLayer<int>();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => layer.MarkDirty( compiledAssembly.ChunkLayout.ChunkCapacity ), Throws.TypeOf<ArgumentOutOfRangeException>() );
			Assert.That( layer.IsDirty( compiledAssembly.ChunkLayout.ChunkCapacity ), Is.False );
		}
	}

	[TestCase( 10, TestName = "TryRemoveCell_TryAddCell_MiddleOfRun_RunsUnchanged" )]
	[TestCase( 0, TestName = "TryRemoveCell_TryAddCell_StartOfRun_RunsUnchanged" )]
	[TestCase( 31, TestName = "TryRemoveCell_TryAddCell_EndOfChunkRun_RunsUnchanged" )]
	[TestCase( 32, TestName = "TryRemoveCell_TryAddCell_StartOfChunkRun_RunsUnchanged" )]
	[TestCase( 63, TestName = "TryRemoveCell_TryAddCell_EndOfRun_RunsUnchanged" )]
	public void TryRemoveCell_TryAddCell_Repeated_RunsUnchanged(
		int column
	) {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out PlacementId placement );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		SourceRun[] expected = compiledAssembly.SourceRuns.ToArray();

		for( int i = 0; i < 100; i++ ) {
			Assert.That( assembly.TryRemoveCell( placement, column, 0 ), Is.True );
			Assert.That( assembly.TryAddCell( placement, column, 0, new TestCell( true, false, false ) ), Is.True );
		}

		Assert.That( compiledAssembly.SourceRuns.ToArray(), Is.EquivalentTo( expected ) );
	}

	[Test]
	public void TryRemoveCell_TryAddCell_SingleCellRun_RunsUnchanged() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out PlacementId placement );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		Assert.That( assembly.TryRemoveCell( placement, 10, 0 ), Is.True );
		Assert.That( assembly.TryRemoveCell( placement, 12, 0 ), Is.True );
		SourceRun[] expected = compiledAssembly.SourceRuns.ToArray();
		Assert.That( expected, Has.One.Matches<SourceRun>( run => run.Length == 1 ) );

		for( int i = 0; i < 100; i++ ) {
			Assert.That( assembly.TryRemoveCell( placement, 11, 0 ), Is.True );
			Assert.That( assembly.TryAddCell( placement, 11, 0, new TestCell( true, false, false ) ), Is.True );
		}

		Assert.That( compiledAssembly.SourceRuns.ToArray(), Is.EquivalentTo( expected ) );
	}

	[Test]
	public void TryRemoveCell_TryAddCell_Scattered_MatchesFreshCompile() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out PlacementId placement );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		int[] columns = [0, 5, 6, 7, 31, 32, 33, 50, 63];

		foreach( int column in columns ) {
			Assert.That( assembly.TryRemoveCell( placement, column, 0 ), Is.True );
		}
		foreach( int column in columns.Reverse() ) {
			Assert.That( assembly.TryAddCell( placement, column, 0, new TestCell( true, false, false ) ), Is.True );
		}

		using ICompiledGridAssembly<TestCell> fresh = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		Assert.That( compiledAssembly.SourceRuns.ToArray(), Is.EquivalentTo( fresh.SourceRuns.ToArray() ) );
	}

	[Test]
	public void CreateLayer_CellEdits_ResizedResetAndNotCommitted() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 128, 1, out PlacementId placement );
		for( int column = 64; column < 128; column++ ) {
			Assert.That( assembly.TryRemoveCell( placement, column, 0 ), Is.True );
		}
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> scratch = compiledAssembly.CreateLayer<int>();

		Assert.That( GetAllValues( scratch ), Is.All.Zero );
		scratch[10, 0] = 5;
		scratch.MarkDirty( 0 );
		compiledAssembly.Commit();
		Assert.That( scratch.IsDirty( 0 ), Is.True );

		Assert.That( assembly.TryRemoveCell( placement, 10, 0 ), Is.True );
		Assert.That( assembly.TryAddCell( placement, 10, 0, new TestCell( true, false, false ) ), Is.True );
		Assert.That( scratch[10, 0], Is.Zero );

		Assert.That( assembly.TryAddCell( placement, 100, 0, new TestCell( true, false, false ) ), Is.True );
		scratch[100, 0] = 7;
		Assert.That( scratch[100, 0], Is.EqualTo( 7 ) );
	}

	[Test]
	public void Swap_BoundAndScratch_CommitStoresScratchValues() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<bool> bound = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding() );
		IGridLayer<bool> scratch = compiledAssembly.CreateLayer<bool>();
		scratch.MarkDirty( 0 );
		scratch.MarkDirty( 1 );

		compiledAssembly.Swap( bound, scratch );
		compiledAssembly.Commit();

		Assert.That( scratch[5, 0], Is.True );
		Assert.That( bound[5, 0], Is.False );
		IGridLayer<bool> reloaded = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding() );
		Assert.That( reloaded[5, 0], Is.False );
	}

	[Test]
	public void Swap_SameLayer_Throws() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> a = compiledAssembly.CreateLayer<int>();

		Assert.That( () => compiledAssembly.Swap( a, a ), Throws.ArgumentException );
	}

	[Test]
	public void Swap_ForeignLayer_Throws() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		using ICompiledGridAssembly<TestCell> other = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> a = compiledAssembly.CreateLayer<int>();
		IGridLayer<int> b = other.CreateLayer<int>();

		Assert.That( () => compiledAssembly.Swap( a, b ), Throws.ArgumentException );
	}

	[Test]
	public void RemoveLayer_BoundLayer_NoLongerCommitted() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<bool> bound = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding() );

		Assert.That( compiledAssembly.RemoveLayer( bound ), Is.True );
		Assert.That( compiledAssembly.Layers, Does.Not.Contain( bound ) );
		Assert.That( compiledAssembly.RemoveLayer( bound ), Is.False );

		bound[5, 0] = false;
		bound.MarkDirty( 0 );
		compiledAssembly.Commit();
		IGridLayer<bool> reloaded = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding() );
		Assert.That( reloaded[5, 0], Is.True );
	}

	[Test]
	public void Values_LocatedCell_MatchesIndexer() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 64, 64, out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> layer = compiledAssembly.CreateLayer<int>();
		ChunkedGridLayout layout = compiledAssembly.ChunkLayout;
		Assert.That( layout.TryLocate( 40, 37, out int chunkIndex, out int localIndex ), Is.True );

		layer[40, 37] = 42;

		using( Assert.EnterMultipleScope() ) {
			Assert.That( layer.Layout, Is.SameAs( layout ) );
			Assert.That( localIndex, Is.EqualTo( ( ( 37 & layout.ChunkMask ) << layout.ChunkShift ) + ( 40 & layout.ChunkMask ) ) );
			Assert.That( layer.Values.GetValues( chunkIndex )[localIndex], Is.EqualTo( 42 ) );
		}
	}

	[TestCase( 32 )]
	[TestCase( 64 )]
	[TestCase( 128 )]
	public void CreateVacuumLayer_IrregularShape_MatchesOccupancy(
		int chunkSize
	) {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 160, 70, out PlacementId placement );
		Point[] holes = [new Point( 10, 10 ), new Point( 31, 31 ), new Point( 32, 31 ), new Point( 63, 32 ), new Point( 64, 33 ), new Point( 127, 5 ), new Point( 128, 6 ), new Point( 50, 64 )];
		foreach( Point hole in holes ) {
			Assert.That( assembly.TryRemoveCell( placement, hole.X, hole.Y ), Is.True );
		}
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, chunkSize );
		ChunkedGridLayout layout = compiledAssembly.ChunkLayout;

		IGridLayer<Direction> vacuum = compiledAssembly.CreateVacuumLayer();

		Assert.That( compiledAssembly.Layers, Does.Contain( vacuum ) );
		Direction[] directions = [Direction.North, Direction.NorthEast, Direction.East, Direction.SouthEast, Direction.South, Direction.SouthWest, Direction.West, Direction.NorthWest];
		for( int row = 0; row < 70; row++ ) {
			for( int column = 0; column < 160; column++ ) {
				if( !layout.IsPopulated( column, row ) ) {
					continue;
				}
				Direction expected = Direction.None;
				foreach( Direction direction in directions ) {
					Point offset = direction.ToOffset();
					if( !layout.IsPopulated( column + offset.X, row + offset.Y ) ) {
						expected |= direction;
					}
				}
				Assert.That( vacuum[column, row], Is.EqualTo( expected ), $"( {column}, {row} )" );
			}
		}
	}

	[Test]
	public void CreateVacuumLayer_UnoccupiedCell_None() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out PlacementId placement );
		Assert.That( assembly.TryRemoveCell( placement, 10, 0 ), Is.True );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );

		IGridLayer<Direction> vacuum = compiledAssembly.CreateVacuumLayer();

		Assert.That( compiledAssembly.ChunkLayout.TryGetChunk( 0, 0, out int chunkIndex ), Is.True );
		Assert.That( vacuum.Values.GetValues( chunkIndex )[10], Is.EqualTo( Direction.None ) );
		Assert.That( vacuum[9, 0], Is.EqualTo( Direction.North | Direction.NorthEast | Direction.East | Direction.SouthEast | Direction.South | Direction.SouthWest | Direction.NorthWest ) );
	}

	private static List<int> GetAllValues(
		IGridLayer<int> layer
	) {
		List<int> values = [];
		foreach( int chunkIndex in layer.Layout.EnumerateChunks() ) {
			values.AddRange( layer.Values.GetValues( chunkIndex ).ToArray() );
		}
		return values;
	}

	/// <summary>
	/// Creates an assembly of a single fully occupied 64x1 source, which
	/// spans two chunks at the default chunk size.
	/// </summary>
	private static GridAssembly<TestCell> CreateFilledAssembly(
		out PlacementId placement
	) {
		return CreateFilledAssembly( 64, 1, out placement );
	}

	private static GridAssembly<TestCell> CreateFilledAssembly(
		int width,
		int height,
		out PlacementId placement
	) {
		DenseGridSource<TestCell> source = new DenseGridSource<TestCell>( width, height );
		for( int row = 0; row < height; row++ ) {
			for( int column = 0; column < width; column++ ) {
				_ = source.TrySetCell( column, row, new TestCell( true, false, false ) );
			}
		}
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		placement = new PlacementId( 1 );
		_ = assembly.TryAttach( source, placement, 0, 0 );
		return assembly;
	}
}
