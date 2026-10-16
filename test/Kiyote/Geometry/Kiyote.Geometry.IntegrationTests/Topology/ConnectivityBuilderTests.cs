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

		IGridLayer<Direction> connectivity = _builder.Build( compiledAssembly, walkabilityStrategy );

		Assert.That( connectivity[49, 14], Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South | Direction.SouthWest | Direction.West ) );
	}

	[Test]
	public void Update_WalkableCellClosed_LayerUpdated() {
		using ICompiledGridAssembly<TestCell> compiledAssembly = CreateAssembly();
		WalkabilityStrategy walkabilityStrategy = new WalkabilityStrategy();

		IGridLayer<Direction> connectivity = _builder.Build( compiledAssembly, walkabilityStrategy );
		Assert.That( connectivity[49, 14], Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South | Direction.SouthWest | Direction.West ) );

		PlacementId source2 = new PlacementId( 2 );
		Assert.That( compiledAssembly.Assembly.TryGetPlacement( source2, out IGridPlacement<TestCell> placement ), Is.True );

		placement.Source.GetCell( 0, 1 ).IsWalkable = false;
		_builder.Update( compiledAssembly, connectivity, walkabilityStrategy, new Rect( 48, 12, 4, 4 ) );
		Assert.That( connectivity[49, 14], Is.EqualTo( Direction.South | Direction.SouthWest | Direction.West ) );
	}

	[TestCase( 31, 8, TestName = "Update_CellClosedOnChunkBoundary_MatchesBuild" )]
	[TestCase( 63, 14, TestName = "Update_CellClosedOnChunkBoundaryInSecondPlacement_MatchesBuild" )]
	[TestCase( 49, 14, TestName = "Update_CellClosedOnSeam_MatchesBuild" )]
	public void Update_CellClosed_MatchesBuild(
		int column,
		int row
	) {
		using ICompiledGridAssembly<TestCell> compiledAssembly = CreateAssembly();
		WalkabilityStrategy walkabilityStrategy = new WalkabilityStrategy();
		IGridLayer<Direction> connectivity = _builder.Build( compiledAssembly, walkabilityStrategy );

		Assert.That( compiledAssembly.Assembly.TryGetPlacementAt( column, row, out IGridPlacement<TestCell> placement ), Is.True );
		placement.Source.GetCell( column - placement.Column, row - placement.Row ).IsWalkable = false;
		_builder.Update( compiledAssembly, connectivity, walkabilityStrategy, new Rect( column, row, 1, 1 ) );

		IGridLayer<Direction> expected = _builder.Build( compiledAssembly, walkabilityStrategy );
		Assert.That( GetAllValues( connectivity ), Is.EqualTo( GetAllValues( expected ) ) );
	}

	[Test]
	public void Build_TwoPlacements_MatchesPerCellEvaluation() {
		using ICompiledGridAssembly<TestCell> compiledAssembly = CreateAssembly();
		WalkabilityStrategy walkabilityStrategy = new WalkabilityStrategy();
		IGridLayer<Direction> connectivity = _builder.Build( compiledAssembly, walkabilityStrategy );
		IGridLayer<Direction> updated = _builder.Build( compiledAssembly, walkabilityStrategy );

		// Update evaluates each cell from the assembly directly rather than from
		// gathered chunk blocks, so recalculating everything must change nothing.
		Rect bounds = compiledAssembly.Assembly.Bounds.Value;
		_builder.Update( compiledAssembly, updated, walkabilityStrategy, bounds );

		Assert.That( GetAllValues( updated ), Is.EqualTo( GetAllValues( connectivity ) ) );
	}

	private static List<Direction> GetAllValues(
		IGridLayer<Direction> layer
	) {
		List<Direction> values = [];
		foreach( int chunkIndex in layer.Layout.EnumerateChunks() ) {
			values.AddRange( layer.Values.GetValues( chunkIndex ).ToArray() );
		}
		return values;
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
