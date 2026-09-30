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

	/// <summary>
	/// Creates an assembly of a single fully occupied 64x1 source, which
	/// spans two chunks at the default chunk size.
	/// </summary>
	private static GridAssembly<TestCell> CreateFilledAssembly(
		out PlacementId placement
	) {
		DenseGridSource<TestCell> source = new DenseGridSource<TestCell>( 64, 1 );
		for( int column = 0; column < source.Width; column++ ) {
			_ = source.TrySetCell( column, 0, new TestCell( true, false, false ) );
		}
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		placement = new PlacementId( 1 );
		_ = assembly.TryAttach( source, placement, 0, 0 );
		return assembly;
	}
}
