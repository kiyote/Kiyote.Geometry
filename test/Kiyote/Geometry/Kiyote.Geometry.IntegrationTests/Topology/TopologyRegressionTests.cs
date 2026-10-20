using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Topology.IntegrationTests;

[TestFixture]
internal sealed class TopologyRegressionTests {

	[TestCase( false )]
	[TestCase( true )]
	public void TryAttach_SourceAlreadyOwned_RejectsWithoutChangingEitherAssembly( bool acrossAssemblies ) {
		CustomSource source = new CustomSource();
		GridAssembly<int> first = new GridAssembly<int>();
		PlacementId id = first.TryAttach( source, 0, 0 ).Placement;
		GridAssembly<int> second = acrossAssemblies ? new GridAssembly<int>() : first;
		int version = second.Version;

		Assert.That( () => second.TryAttach( source, 10, 0 ), Throws.InvalidOperationException );
		Assert.That( second.Version, Is.EqualTo( version ) );
		Assert.That( second.Placements, Has.Count.EqualTo( acrossAssemblies ? 0 : 1 ) );
		Assert.That( first.TryGetPlacementAt( 0, 0, out _ ), Is.True );
		Assert.That( first.TryDetach( id ), Is.True );
		Assert.That( second.TryAttach( source, 10, 0 ).Succeeded, Is.True );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void TrySetCellAndTryClearCell_AttachedSource_RejectDirectShapeChangesButAllowValuesAndCoordinatedEdits( bool clear ) {
		DenseGridSource<int> source = Source( 2, 1 );
		GridAssembly<int> assembly = new GridAssembly<int>();
		PlacementId id = assembly.TryAttach( source, 0, 0 ).Placement;
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		IGridLayer<int> layer = compiled.Bind<int, IntBinding>( default );
		Assert.That( () => clear ? source.TryClearCell( 0, 0 ) : source.TrySetCell( 1, 0, 2 ),
			Throws.InvalidOperationException );
		Assert.That( assembly.TryGetPlacementAt( 0, 0, out _ ), Is.True );
		Assert.That( source.IsOccupied( 1, 0 ), Is.False );
		Assert.That( assembly.TryAddCell( id, 1, 0, 3 ), Is.True );
		Assert.That( layer[1, 0], Is.EqualTo( 3 ) );
		Assert.That( assembly.TryRemoveCell( id, 1, 0 ), Is.True );
		Assert.That( compiled.ChunkLayout.IsPopulated( 1, 0 ), Is.False );

		source.GetCell( 0, 0 ) = 4;
		Assert.That( source.TrySetCell( 0, 0, 5 ), Is.True );
		layer[0, 0] = 6;
		Assert.That( compiled.ChunkLayout.TryLocate( 0, 0, out int chunk, out _ ), Is.True );
		layer.MarkDirty( chunk );
		compiled.Commit();
		Assert.That( source.GetCell( 0, 0 ), Is.EqualTo( 6 ) );
		Assert.That( assembly.TryDetach( id ), Is.True );
		Assert.That( source.TryClearCell( 0, 0 ), Is.True );
		Assert.That( source.TrySetCell( 1, 0, 7 ), Is.True );
	}

	[Test]
	public void TryAttach_OverlappingPlacement_RejectsWithoutAcquiringOwnership() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 1, 1 ), 0, 0 );
		DenseGridSource<int> rejected = Source( 1, 1 );
		Assert.That( assembly.TryAttach( rejected, 0, 0 ).Succeeded, Is.False );
		Assert.That( assembly.Version, Is.EqualTo( 1 ) );
		Assert.That( assembly.Placements, Has.Count.EqualTo( 1 ) );
		Assert.That( new GridAssembly<int>().TryAttach( rejected, 0, 0 ).Succeeded, Is.True );
	}

	[Test]
	public void TryAddCell_CustomSourceThrows_PreservesShapeLockAndAllowsSubsequentEdits() {
		CustomSource source = new CustomSource();
		GridAssembly<int> assembly = new GridAssembly<int>();
		PlacementId id = assembly.TryAttach( source, 0, 0 ).Placement;
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		IGridLayer<int> layer = compiled.Bind<int, IntBinding>( default );
		source.ThrowOnSet = true;
		Assert.That( () => assembly.TryAddCell( id, 1, 0, 2 ), Throws.InvalidOperationException );
		source.ThrowOnSet = false;
		Assert.That( () => source.TrySetCell( 1, 0, 2 ), Throws.InvalidOperationException );
		Assert.That( assembly.TryGetPlacementAt( 1, 0, out _ ), Is.False );
		Assert.That( assembly.TryAddCell( id, 1, 0, 3 ), Is.True );
		Assert.That( layer[1, 0], Is.EqualTo( 3 ) );
		Assert.That( new ConnectivityBuilder().Build( compiled, new ConnectedStrategy() )[0, 0], Is.EqualTo( Direction.East ) );
		layer[1, 0] = 8;
		layer.MarkDirty( 0 );
		compiled.Commit();
		Assert.That( source.GetCell( 1, 0 ), Is.EqualTo( 8 ) );
	}

	[TestCase( 65536, 65536 )]
	[TestCase( 50000, 50000 )]
	public void Ctor_DenseSourceDimensionsExceedArrayCapacity_RejectsBeforeAllocation( int width, int height ) {
		Assert.That( () => new DenseGridSource<int>( width, height ), Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void TryAttach_OverflowingPlacement_RejectsBeforeOverlapScanAndLeavesSourceAvailable() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 1, 1 ), int.MinValue, 0 );
		DenseGridSource<int> source = Source( 2, 1 );
		source.TrySetCell( 1, 0, 2 );
		Assert.That( () => assembly.TryAttach( source, int.MaxValue, 0 ), Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( assembly.Version, Is.EqualTo( 1 ) );
		Assert.That( assembly.Placements, Has.Count.EqualTo( 1 ) );
		Assert.That( assembly.TryAttach( source, 100, 0 ).Succeeded, Is.True );
	}

	[Test]
	public void TryAttach_InvalidCustomRuns_RejectsBeforeMutatingAssembly() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		CustomSource source = new CustomSource { InvalidRuns = true };
		Assert.That( () => assembly.TryAttach( source, 0, 0 ), Throws.ArgumentException );
		Assert.That( assembly.Version, Is.Zero );
		Assert.That( assembly.Placements, Is.Empty );
		Assert.That( assembly.TryGetPlacementAt( 0, 0, out _ ), Is.False );
		source.InvalidRuns = false;
		Assert.That( assembly.TryAttach( source, 0, 0 ).Succeeded, Is.True );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void TryAttach_IntExtremeNeighbours_DoesNotCreateWraparoundSeams( bool vertical ) {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 1, 1 ), vertical ? 0 : int.MinValue, vertical ? int.MinValue : 0 );
		_ = assembly.TryAttach( Source( 1, 1 ), vertical ? 0 : int.MaxValue, vertical ? int.MaxValue : 0 );
		Assert.That( assembly.Seams, Is.Empty );
		Assert.That( () => assembly.Bounds, Throws.TypeOf<ArgumentOutOfRangeException>() );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		Assert.That( compiled.SeamLinks.Length, Is.Zero );
		IGridLayer<Direction> connections = new ConnectivityBuilder().Build( compiled, new ConnectedStrategy() );
		IGridLayer<Direction> vacuum = compiled.CreateVacuumLayer();
		Assert.That( connections[vertical ? 0 : int.MaxValue, vertical ? int.MaxValue : 0], Is.EqualTo( Direction.None ) );
		Assert.That( vacuum[vertical ? 0 : int.MaxValue, vertical ? int.MaxValue : 0], Is.EqualTo( (Direction)255 ) );
	}

	[TestCase( int.MinValue, int.MinValue )]
	[TestCase( int.MaxValue - 1, int.MaxValue - 1 )]
	[TestCase( int.MinValue, 10 )]
	[TestCase( 10, int.MaxValue - 1 )]
	public void Update_ConnectivityAtDomainEdges_RecalculatesWithoutOverflow( int x, int y ) {
		DenseGridSource<int> source = Source( 2, 2 );
		source.TrySetCell( 1, 0, 1 );
		source.TrySetCell( 0, 1, 1 );
		source.TrySetCell( 1, 1, 1 );
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( source, x, y );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		ConnectivityBuilder builder = new ConnectivityBuilder();
		IGridLayer<Direction> layer = builder.Build( compiled, new ConnectedStrategy() );
		Assert.That( layer[x, y], Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South ) );
		source.GetCell( 0, 0 ) = 0;
		builder.Update( compiled, layer, new ConnectedStrategy(), new Rect( x, y, 1, 1 ) );
		Assert.That( layer[x, y], Is.EqualTo( Direction.None ) );
		Assert.That( layer[x + 1, y], Is.EqualTo( Direction.South | Direction.SouthWest ) );
	}

	[Test]
	public void Update_EmptyConnectivityArea_LeavesLayerUnchanged() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 1, 1 ), 0, 0 );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		ConnectivityBuilder builder = new ConnectivityBuilder();
		IGridLayer<Direction> layer = builder.Build( compiled, new ConnectedStrategy() );
		layer[0, 0] = Direction.West;
		builder.Update( compiled, layer, new ConnectedStrategy(), default );
		Assert.That( layer[0, 0], Is.EqualTo( Direction.West ) );
	}

	[Test]
	public void Update_OverflowingConnectivityScratch_RejectsBeforeChangingValues() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 1, 1 ), 0, 0 );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		ConnectivityBuilder builder = new ConnectivityBuilder();
		IGridLayer<Direction> layer = builder.Build( compiled, new ConnectedStrategy() );
		layer[0, 0] = Direction.West;
		Assert.That( () => builder.Update( compiled, layer, new ConnectedStrategy(), new Rect( 0, 0, 65532, 65532 ) ),
			Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( layer[0, 0], Is.EqualTo( Direction.West ) );
	}

	[Test]
	public void Compile_AlternateAssembly_RejectsBeforeCreatingSpace() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 2, 1 ), 0, 0 );
		Assert.That( () => Compile( new AssemblyProxy( assembly ) ), Throws.ArgumentException );
	}

	[TestCase( "commit", false )]
	[TestCase( "bind", false )]
	[TestCase( "create", false )]
	[TestCase( "remove", false )]
	[TestCase( "swap", false )]
	[TestCase( "vacuum", false )]
	[TestCase( "build", false )]
	[TestCase( "update", false )]
	[TestCase( "commit", true )]
	[TestCase( "bind", true )]
	[TestCase( "create", true )]
	[TestCase( "remove", true )]
	[TestCase( "swap", true )]
	[TestCase( "vacuum", true )]
	[TestCase( "build", true )]
	[TestCase( "update", true )]
	public void CompiledOperations_InvalidLifetime_RejectWithoutWritingReusedSource( string operation, bool disposed ) {
		GridAssembly<int> assembly = new GridAssembly<int>();
		DenseGridSource<int> source = Source( 1, 1 );
		PlacementId id = assembly.TryAttach( source, 0, 0 ).Placement;
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		IGridLayer<int> bound = compiled.Bind<int, IntBinding>( default );
		IGridLayer<int> scratch = compiled.CreateLayer<int>();
		ConnectivityBuilder builder = new ConnectivityBuilder();
		IGridLayer<Direction> directions = builder.Build( compiled, new ConnectedStrategy() );
		bound[0, 0] = 7;
		bound.MarkDirty( 0 );
		if( disposed ) {
			compiled.Dispose();
		} else {
			Assert.That( assembly.TryDetach( id ), Is.True );
			Assert.That( new GridAssembly<int>().TryAttach( source, 10, 10 ).Succeeded, Is.True );
			source.GetCell( 0, 0 ) = 99;
		}
		Action action = operation switch {
			"commit" => compiled.Commit,
			"bind" => () => compiled.Bind<int, IntBinding>( default ),
			"create" => () => compiled.CreateLayer<int>(),
			"remove" => () => compiled.RemoveLayer( bound ),
			"swap" => () => compiled.Swap( bound, scratch ),
			"vacuum" => () => compiled.CreateVacuumLayer(),
			"build" => () => builder.Build( compiled, new ConnectedStrategy() ),
			"update" => () => builder.Update( compiled, directions, new ConnectedStrategy(), new Rect( 0, 0, 1, 1 ) ),
			_ => throw new ArgumentException( "Unknown operation.", nameof( operation ) )
		};
		Assert.That( action, disposed ? Throws.TypeOf<ObjectDisposedException>() : Throws.TypeOf<InvalidOperationException>() );
		Assert.That( source.GetCell( 0, 0 ), Is.EqualTo( disposed ? 1 : 99 ) );
	}

	[Test]
	public void Commit_PlacementAttachedAfterCompilation_RejectsBeforeAnyWrite() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		DenseGridSource<int> source = Source( 1, 1 );
		_ = assembly.TryAttach( source, 0, 0 );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		IGridLayer<int> layer = compiled.Bind<int, IntBinding>( default );
		layer[0, 0] = 9;
		layer.MarkDirty( 0 );
		_ = assembly.TryAttach( Source( 1, 1 ), 10, 0 );
		Assert.That( () => compiled.Commit(), Throws.InvalidOperationException );
		Assert.That( source.GetCell( 0, 0 ), Is.EqualTo( 1 ) );
	}

	[Test]
	public void PlacementsAndSeamsAndLayers_PublicAccess_ExposesReadOnlyCollections() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 1, 1 ), 0, 0 );
		_ = assembly.TryAttach( Source( 1, 1 ), 1, 0 );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		_ = compiled.CreateLayer<int>();
		using( Assert.EnterMultipleScope() ) {
			AssertReadOnly( assembly.Placements );
			AssertReadOnly( assembly.Seams );
			AssertReadOnly( assembly.Seams[0].Contacts );
			AssertReadOnly( compiled.Layers );
		}
	}

	[Test]
	public void ChunkLayout_PublicAccess_PreventsSourceAndLayerOccupancyDesynchronization() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( Source( 1, 1 ), 0, 0 );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		Assert.That( typeof( GridLayout ).GetMethod( "ClearPopulated" ), Is.Null );
		Assert.That( typeof( GridLayout ).GetMethod( "GetPresence" )!.ReturnType, Is.EqualTo( typeof( ReadOnlySpan<ulong> ) ) );
		Assert.That( typeof( IGridLayer<int> ).GetProperty( "Values" ), Is.Null );
		Assert.That( compiled.ChunkLayout.IsPopulated( 0, 0 ), Is.True );
		Assert.That( assembly.TryGetPlacementAt( 0, 0, out _ ), Is.True );
	}

	[Test]
	public void GetValuesAndCopyPadded_ReadOnlyLayout_PreserveWritableSpansAndPaddedChunkAccess() {
		DenseGridSource<int> source = Source( 2, 1 );
		source.TrySetCell( 1, 0, 2 );
		GridAssembly<int> assembly = new GridAssembly<int>();
		_ = assembly.TryAttach( source, 31, 0 );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		IGridLayer<int> layer = compiled.Bind<int, IntBinding>( default );
		Assert.That( compiled.ChunkLayout.TryLocate( 31, 0, out int chunk, out int local ), Is.True );
		Assert.That( layer.Layout, Is.SameAs( compiled.ChunkLayout ) );
		Span<int> padded = stackalloc int[34 * 34];
		layer.CopyPadded( chunk, 1, padded );
		Assert.That( padded[34 + 32], Is.EqualTo( 1 ) );
		Assert.That( padded[34 + 33], Is.EqualTo( 2 ) );
		Span<int> row = stackalloc int[34];
		layer.CopyPaddedRow( chunk, 0, 1, row );
		Assert.That( row[32], Is.EqualTo( 1 ) );
		Assert.That( row[33], Is.EqualTo( 2 ) );
		Assert.That( layer.GetRowValues( chunk, 0 )[31], Is.EqualTo( 1 ) );
		layer.GetValues( chunk )[local] = 5;
		Assert.That( layer[31, 0], Is.EqualTo( 5 ) );
		Assert.That( layer.Layout.GetPresenceRow( chunk, 0 )[0], Is.EqualTo( 1UL << 31 ) );
		Assert.That( source.GetCell( 0, 0 ), Is.EqualTo( 1 ) );
		layer.MarkDirty( chunk );
		compiled.Commit();
		Assert.That( source.GetCell( 0, 0 ), Is.EqualTo( 5 ) );
	}

	[Test]
	public void GetLengthAndLastColumn_CapacityAndRunBoundaries_RejectOverflowWithoutLargeAllocations() {
		Assert.That( GridCapacity.GetLength( Array.MaxLength, "length" ), Is.EqualTo( Array.MaxLength ) );
		Assert.That( GridCapacity.GetLength( 0, "length" ), Is.Zero );
		Assert.That( () => GridCapacity.GetLength( (long)Array.MaxLength + 1, "length" ), Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( () => GridCapacity.GetLength( -1, "length" ), Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( new CellRun( int.MaxValue, 0, 1 ).LastColumn, Is.EqualTo( int.MaxValue ) );
		Assert.That( () => new CellRun( int.MaxValue, 0, 2 ).LastColumn, Throws.TypeOf<OverflowException>() );
	}

	[Test]
	public void TryAttach_EmptySource_PreservesExclusiveOwnershipAndInvalidatesPriorCompilation() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		DenseGridSource<int> source = new DenseGridSource<int>( 2, 1 );
		PlacementId id = assembly.TryAttach( source, 0, 0 ).Placement;
		Assert.That( source.Ownership.IsAttached, Is.True );
		Assert.That( () => new GridAssembly<int>().TryAttach( source, 0, 0 ), Throws.InvalidOperationException );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		_ = assembly.TryAttach( Source( 1, 1 ), 10, 0 );
		Assert.That( assembly.TryAddCell( id, 1, 0, 3 ), Is.True );
		Assert.That( compiled.IsStale, Is.True );
		Assert.That( compiled.SourceRuns.Length, Is.Zero );
		Assert.That( compiled.ChunkLayout.IsEmpty, Is.True );
		Assert.That( assembly.TryDetach( id ), Is.True );
		Assert.That( source.Ownership.IsAttached, Is.False );
		Assert.That( source.TryClearCell( 1, 0 ), Is.True );
	}

	[Test]
	public void BuildAndUpdate_ThrowingStrategy_AvoidRegisteringFailedLayersAndPreserveSubsequentPooledCalls() {
		GridAssembly<int> assembly = new GridAssembly<int>();
		DenseGridSource<int> source = Source( 2, 1 );
		source.TrySetCell( 1, 0, 1 );
		_ = assembly.TryAttach( source, 0, 0 );
		using ICompiledGridAssembly<int> compiled = Compile( assembly );
		ConnectivityBuilder builder = new ConnectivityBuilder();
		for( int i = 0; i < 5; i++ ) {
			Assert.That( () => builder.Build( compiled, new ThrowingStrategy() ), Throws.InvalidOperationException );
			Assert.That( compiled.Layers, Is.Empty );
			IGridLayer<Direction> layer = builder.Build( compiled, new ConnectedStrategy() );
			Assert.That( layer[0, 0], Is.EqualTo( Direction.East ) );
			Assert.That( () => builder.Update( compiled, layer, new ThrowingStrategy(), new Rect( 0, 0, 1, 1 ) ), Throws.InvalidOperationException );
			builder.Update( compiled, layer, new ConnectedStrategy(), new Rect( 0, 0, 1, 1 ) );
			Assert.That( layer[1, 0], Is.EqualTo( Direction.West ) );
			Assert.That( compiled.RemoveLayer( layer ), Is.True );
		}
	}

	private static void AssertReadOnly<T>( IReadOnlyList<T> values ) {
		if( values is IList<T> list ) {
			Assert.That( () => list[0] = list[0], Throws.TypeOf<NotSupportedException>() );
		}
	}

	private static DenseGridSource<int> Source( int width, int height ) {
		DenseGridSource<int> source = new DenseGridSource<int>( width, height );
		source.TrySetCell( 0, 0, 1 );
		return source;
	}

	private static ICompiledGridAssembly<int> Compile( IGridAssembly<int> assembly ) =>
		new GridCompiler().Compile( assembly, GridCompiler.DefaultChunkSize );

	private readonly struct IntBinding : IGridLayerBinding<int, int> {
		public int Extract( in int cell ) => cell;
		public void Commit( ref int cell, int value ) => cell = value;
	}

	private readonly struct ConnectedStrategy : IConnectivityStrategy<int> {
		public bool Evaluate( in TopologyCell<int> source, in TopologyCell<int> destination, Direction direction,
			in TopologyCell<int> orthogonalA, in TopologyCell<int> orthogonalB, bool isSeam ) => source.Cell > 0 && destination.Cell > 0;
	}

	private readonly struct ThrowingStrategy : IConnectivityStrategy<int> {
		public bool Evaluate( in TopologyCell<int> source, in TopologyCell<int> destination, Direction direction,
			in TopologyCell<int> orthogonalA, in TopologyCell<int> orthogonalB, bool isSeam ) =>
			throw new InvalidOperationException( "strategy failed" );
	}

	private sealed class CustomSource : IGridSource<int> {
		private readonly DenseGridSource<int> _inner = Source( 2, 1 );
		private readonly CellRun[] _invalid = [new CellRun( 0, 0, 3 )];
		public bool ThrowOnSet { get; set; }
		public bool InvalidRuns { get; set; }
		public GridSourceOwnership Ownership => _inner.Ownership;
		public int Width => _inner.Width;
		public int Height => _inner.Height;
		public ReadOnlySpan<CellRun> GetOccupiedRuns( int row ) => InvalidRuns ? _invalid : _inner.GetOccupiedRuns( row );
		public bool IsOccupied( int column, int row ) => _inner.IsOccupied( column, row );
		public ref int GetCell( int column, int row ) => ref _inner.GetCell( column, row );
		public bool TryGetRow( int row, out Span<int> cells ) { cells = []; return false; }
		public bool TrySetCell( int column, int row, in int cell ) {
			if( ThrowOnSet ) {
				throw new InvalidOperationException( "custom source edit failed" );
			}
			return _inner.TrySetCell( column, row, in cell );
		}
		public bool TryClearCell( int column, int row ) => _inner.TryClearCell( column, row );
	}

	private sealed class AssemblyProxy( GridAssembly<int> inner ) : IGridAssembly<int> {
		public int Version => inner.Version;
		public Rect? Bounds => inner.Bounds;
		public IReadOnlyList<IGridPlacement<int>> Placements => inner.Placements;
		public IReadOnlyList<GridSeam> Seams => inner.Seams;
		public AttachResult TryAttach( IGridSource<int> source, int column, int row ) => inner.TryAttach( source, column, row );
		public AttachResult TryAttach( IGridSource<int> source, PlacementId id, int column, int row ) => inner.TryAttach( source, id, column, row );
		public bool TryDetach( PlacementId placement ) => inner.TryDetach( placement );
		public bool TryGetPlacement( PlacementId placement, [MaybeNullWhen( false )] out IGridPlacement<int> result ) => inner.TryGetPlacement( placement, out result );
		public bool TryGetPlacementAt( int column, int row, [MaybeNullWhen( false )] out IGridPlacement<int> result ) => inner.TryGetPlacementAt( column, row, out result );
		public bool TryAddCell( PlacementId placement, int column, int row, in int cell ) => inner.TryAddCell( placement, column, row, in cell );
		public bool TryRemoveCell( PlacementId placement, int column, int row ) => inner.TryRemoveCell( placement, column, row );
	}
}
