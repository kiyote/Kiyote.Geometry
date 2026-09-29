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
}
