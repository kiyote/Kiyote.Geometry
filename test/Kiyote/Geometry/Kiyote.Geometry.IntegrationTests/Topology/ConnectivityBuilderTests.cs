namespace Kiyote.Geometry.Topology.IntegrationTests;

[TestFixture]
internal sealed class ConnectivityBuilderTests {

	private IConnectivityBuilder _builder;

	[SetUp]
	public void SetUp() {
		_builder = new ConnectivityBuilder();
	}

	[Test]
	public void Build_WalkableAssembly_LayerGenerated() {
		using ICompiledGridAssembly<TestCell> compiledAssembly = CreateAssembly();
		WalkabilityStrategy walkabilityStrategy = new WalkabilityStrategy();

		IGridLayer<Direction> connectivity = _builder.Build( compiledAssembly, walkabilityStrategy, 0 );

		Assert.That( connectivity[49, 14], Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South | Direction.SouthWest | Direction.West ) );
	}

	[Test]
	public void Update_WalkableCellClosed_LayerUpdated() {
		using ICompiledGridAssembly<TestCell> compiledAssembly = CreateAssembly();
		WalkabilityStrategy walkabilityStrategy = new WalkabilityStrategy();

		IGridLayer<Direction> connectivity = _builder.Build( compiledAssembly, walkabilityStrategy, 0 );
		Assert.That( connectivity[49, 14], Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South | Direction.SouthWest | Direction.West ) );

		PlacementId source2 = new PlacementId( 2 );
		Assert.That( compiledAssembly.Assembly.TryGetPlacement( source2, out IGridPlacement<TestCell> placement ), Is.True );

		placement.Source.GetCell( 0, 1 ).IsWalkable = false;
		_builder.Update( compiledAssembly, connectivity, walkabilityStrategy, new Rect( 48, 12, 4, 4 ) );
		Assert.That( connectivity[49, 14], Is.EqualTo( Direction.South | Direction.SouthWest | Direction.West ) );
	}

	private struct WalkabilityStrategy : IConnectivityStrategy<TestCell> {
		public readonly bool Evaluate(
			in TopologyCell<TestCell> source,
			in TopologyCell<TestCell> destination,
			Direction direction,
			in TopologyCell<TestCell> orthogonalA,
			in TopologyCell<TestCell> orthogonalB,
			bool isSeam
		) {
			if( direction == Direction.NorthEast
				|| direction == Direction.SouthEast
				|| direction == Direction.SouthWest
				|| direction == Direction.NorthWest
			) {
				return source.Cell.IsWalkable
					&& destination.Cell.IsWalkable
					&& orthogonalA.Cell.IsWalkable
					&& orthogonalB.Cell.IsWalkable;
			}
			return source.Cell.IsWalkable && destination.Cell.IsWalkable;
		}
	}

	private static ICompiledGridAssembly<TestCell> CreateAssembly() {
		DenseGridSource<TestCell> source1 = AsciiMapSource.Create( AsciiMapSource.Map1 );
		DenseGridSource<TestCell> source2 = AsciiMapSource.Create( AsciiMapSource.Map1 );
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		PlacementId placement1 = new PlacementId( 1 );
		PlacementId placement2 = new PlacementId( 2 );
		_ = assembly.TryAttach( source1, placement1, 0, 0 );
		_ = assembly.TryAttach( source2, placement2, 50, 13 );

		GridCompiler compiler = new GridCompiler();

		return compiler.Compile( assembly, GridCompiler.DefaultChunkSize );
	}
}
