namespace Kiyote.Geometry.Topology.IntegrationTests;

[TestFixture]
internal sealed class GridCompilerTests {

	private IGridCompiler _compiler;

	[SetUp]
	public void SetUp() {
		_compiler = new GridCompiler();
	}

	[Test]
	public void Compile_TwoSourceAssembly_CompilationCreated() {
		IGridAssembly<TestCell> assembly = CreateAssembly();

		ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( compiledAssembly.Assembly, Is.SameAs( assembly ) );
			Assert.That( compiledAssembly.ChunkLayout.SlotCount, Is.EqualTo( 4 ) );
		}
	}

	[Test]
	public void Bind_TwoSourceAssembly_SingleCorrectLayerCreated() {
		IGridAssembly<TestCell> assembly = CreateAssembly();
		ICompiledGridAssembly<TestCell> compiledAssembly = _compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
		WalkabilityBinding walkabilityBinding = new WalkabilityBinding();

		IGridLayer<bool> walkability = compiledAssembly.Bind<bool, WalkabilityBinding>( walkabilityBinding, 0 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( walkability[1, 1], Is.True );
			Assert.That( walkability[98, 30], Is.True );
		}
	}

	private static IGridAssembly<TestCell> CreateAssembly() {
		DenseGridSource<TestCell> source1 = AsciiMapSource.Create( AsciiMapSource.Map1 );
		DenseGridSource<TestCell> source2 = AsciiMapSource.Create( AsciiMapSource.Map1 );
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		PlacementId placement1 = new PlacementId( 1 );
		PlacementId placement2 = new PlacementId( 2 );
		_ = assembly.TryAttach( source1, placement1, 0, 0 );
		_ = assembly.TryAttach( source2, placement2, 50, 13 );

		return assembly;
	}
}
