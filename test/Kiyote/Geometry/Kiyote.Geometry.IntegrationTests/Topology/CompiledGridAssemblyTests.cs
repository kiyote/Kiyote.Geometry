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
		IGridLayer<bool> walkability = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding(), 0 );

		Assert.That( () => walkability[20, 1], Throws.TypeOf<ArgumentOutOfRangeException>() );
		compiledAssembly.Assembly.TryAddCell( placement, 20, 1, new TestCell( true, false, false ) );
		Assert.That( walkability[20, 1], Is.True );
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

	[TestCase( 1 )]
	[TestCase( 2 )]
	[TestCase( 3 )]
	[TestCase( 4 )]
	public void ExchangeHalos_WideHalo_HaloMatchesNeighbours(
		int halo
	) {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 96, 96, out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> layer = compiledAssembly.CreateLayer<int>( halo );
		for( int row = 0; row < 96; row++ ) {
			for( int column = 0; column < 96; column++ ) {
				layer[column, row] = Encode( column, row );
			}
		}

		layer.ExchangeHalos();

		IGridChunkLayout space = compiledAssembly.ChunkLayout;
		int size = space.ChunkSize;
		for( int slot = 0; slot < space.SlotCount; slot++ ) {
			Point origin = space.GetOrigin( slot );
			for( int localRow = -halo; localRow < size + halo; localRow++ ) {
				for( int localColumn = -halo; localColumn < size + halo; localColumn++ ) {
					int column = origin.X + localColumn;
					int row = origin.Y + localRow;
					int expected = column >= 0 && column < 96 && row >= 0 && row < 96
						? Encode( column, row )
						: 0;
					Assert.That( layer.Cells[layer.IndexOf( slot, localColumn, localRow )], Is.EqualTo( expected ), $"slot {slot} ( {localColumn}, {localRow} )" );
				}
			}
		}
	}

	[Test]
	public void CreateLayer_HaloGreaterThanChunkSize_Throws() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );

		Assert.That( () => compiledAssembly.CreateLayer<int>( GridCompiler.DefaultChunkSize + 1 ), Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void CreateLayer_CellEdits_ResizedResetAndNotCommitted() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 128, 1, out PlacementId placement );
		for( int column = 64; column < 128; column++ ) {
			Assert.That( assembly.TryRemoveCell( placement, column, 0 ), Is.True );
		}
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> scratch = compiledAssembly.CreateLayer<int>( 1 );

		Assert.That( scratch.Cells.ToArray(), Is.All.Zero );
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
		IGridLayer<bool> bound = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding(), 1 );
		IGridLayer<bool> scratch = compiledAssembly.CreateLayer<bool>( 1 );
		scratch.MarkDirty( 0 );
		scratch.MarkDirty( 1 );

		compiledAssembly.Swap( bound, scratch );
		compiledAssembly.Commit();

		Assert.That( scratch[5, 0], Is.True );
		Assert.That( bound[5, 0], Is.False );
		IGridLayer<bool> reloaded = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding(), 0 );
		Assert.That( reloaded[5, 0], Is.False );
	}

	[Test]
	public void Swap_DifferentHalo_Throws() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> a = compiledAssembly.CreateLayer<int>( 1 );
		IGridLayer<int> b = compiledAssembly.CreateLayer<int>( 2 );

		Assert.That( () => compiledAssembly.Swap( a, b ), Throws.ArgumentException );
	}

	[Test]
	public void Swap_ForeignLayer_Throws() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		using ICompiledGridAssembly<TestCell> other = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> a = compiledAssembly.CreateLayer<int>( 1 );
		IGridLayer<int> b = other.CreateLayer<int>( 1 );

		Assert.That( () => compiledAssembly.Swap( a, b ), Throws.ArgumentException );
	}

	[Test]
	public void RemoveLayer_BoundLayer_NoLongerCommitted() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<bool> bound = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding(), 0 );

		Assert.That( compiledAssembly.RemoveLayer( bound ), Is.True );
		Assert.That( compiledAssembly.Layers, Does.Not.Contain( bound ) );
		Assert.That( compiledAssembly.RemoveLayer( bound ), Is.False );

		bound[5, 0] = false;
		bound.MarkDirty( 0 );
		compiledAssembly.Commit();
		IGridLayer<bool> reloaded = compiledAssembly.Bind<bool, WalkabilityBinding>( new WalkabilityBinding(), 0 );
		Assert.That( reloaded[5, 0], Is.True );
	}

	[Test]
	public void GetChunkReference_LayoutFormula_MatchesIndexer() {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 64, 64, out _ );
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridLayer<int> layer = compiledAssembly.CreateLayer<int>( 2 );
		Assert.That( compiledAssembly.ChunkLayout.TryGetCell( 40, 37, out int slot, out _ ), Is.True );
		Point origin = compiledAssembly.ChunkLayout.GetOrigin( slot );
		int column = 40 - origin.X;
		int row = 37 - origin.Y;

		layer[40, 37] = 42;

		int offset = ( ( row + layer.Halo ) * layer.Stride ) + column + layer.Halo;
		Assert.That( layer.ChunkLength, Is.EqualTo( layer.Stride * layer.Stride ) );
		Assert.That( System.Runtime.CompilerServices.Unsafe.Add( ref layer.GetChunkReference( slot ), offset ), Is.EqualTo( 42 ) );
		Assert.That( layer.Cells[( slot * layer.ChunkLength ) + offset], Is.EqualTo( 42 ) );
		Assert.That( layer.IndexOf( slot, column, row ), Is.EqualTo( ( slot * layer.ChunkLength ) + offset ) );
	}

	[TestCase( 0 )]
	[TestCase( 1 )]
	public void CreateVacuumLayer_IrregularShape_MatchesOccupancy(
		int halo
	) {
		GridAssembly<TestCell> assembly = CreateFilledAssembly( 96, 70, out PlacementId placement );
		Point[] holes = [new Point( 10, 10 ), new Point( 31, 31 ), new Point( 32, 31 ), new Point( 63, 32 ), new Point( 50, 64 )];
		foreach( Point hole in holes ) {
			Assert.That( assembly.TryRemoveCell( placement, hole.X, hole.Y ), Is.True );
		}
		using ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		IGridChunkLayout space = compiledAssembly.ChunkLayout;

		IGridLayer<Direction> vacuum = compiledAssembly.CreateVacuumLayer( halo );

		Assert.That( compiledAssembly.Layers, Does.Contain( vacuum ) );
		Direction[] directions = [Direction.North, Direction.NorthEast, Direction.East, Direction.SouthEast, Direction.South, Direction.SouthWest, Direction.West, Direction.NorthWest];
		for( int row = 0; row < 70; row++ ) {
			for( int column = 0; column < 96; column++ ) {
				if( !space.TryGetCell( column, row, out _, out _ ) ) {
					continue;
				}
				Direction expected = Direction.None;
				foreach( Direction direction in directions ) {
					Point offset = direction.ToOffset();
					if( !space.TryGetCell( column + offset.X, row + offset.Y, out _, out _ ) ) {
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

		IGridLayer<Direction> vacuum = compiledAssembly.CreateVacuumLayer( 1 );

		Assert.That( compiledAssembly.ChunkLayout.TryGetSlot( 0, 0, out int slot ), Is.True );
		Assert.That( vacuum.Cells[vacuum.IndexOf( slot, 10, 0 )], Is.EqualTo( Direction.None ) );
		Assert.That( vacuum[9, 0], Is.EqualTo( Direction.North | Direction.NorthEast | Direction.East | Direction.SouthEast | Direction.South | Direction.SouthWest | Direction.NorthWest ) );
	}

	private static int Encode(
		int column,
		int row
	) {
		return ( row * 1000 ) + column + 1;
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
