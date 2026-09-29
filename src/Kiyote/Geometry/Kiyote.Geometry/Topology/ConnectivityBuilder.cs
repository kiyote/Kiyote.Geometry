namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IConnectivityBuilder"/>.
/// </summary>
public sealed class ConnectivityBuilder : IConnectivityBuilder {

	private static readonly Direction[] _neighbours = [
		Direction.North,
		Direction.NorthEast,
		Direction.East,
		Direction.SouthEast,
		Direction.South,
		Direction.SouthWest,
		Direction.West,
		Direction.NorthWest
	];

	public IGridLayer<Direction> Build<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		TStrategy strategy,
		int halo
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		CompiledGridAssembly<TCell> compiled = GetCompiled( compiledGridAssembly );
		GridLayer<Direction> result = new GridLayer<Direction>( compiled.GridChunkLayout, halo );
		Context<TCell> context = new Context<TCell>( compiled );

		foreach( SourceRun run in compiled.SourceRuns ) {
			compiled.Assembly.TryGetPlacement( run.Placement, out IGridPlacement<TCell>? placement );
			int row = placement!.Row + run.SourceRow;
			for( int i = 0; i < run.Length; i++ ) {
				int column = placement.Column + run.SourceColumn + i;
				result.Cells[result.IndexOf( run.Slot, run.LocalIndex + i )] = Calculate( ref strategy, context, column, row );
			}
		}

		result.ExchangeHalos();
		compiled.AddLayer( result );
		return result;
	}

	public void Update<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		IGridLayer<Direction> layer,
		TStrategy strategy,
		Rect area
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		CompiledGridAssembly<TCell> compiled = GetCompiled( compiledGridAssembly );
		ArgumentNullException.ThrowIfNull( layer );
		if( layer is not GridLayer<Direction> result
			|| !compiled.Layers.Contains( layer )
		) {
			throw new ArgumentException( "The layer does not belong to the compiled assembly.", nameof( layer ) );
		}

		GridChunkLayout chunks = compiled.GridChunkLayout;
		Context<TCell> context = new Context<TCell>( compiled );

		// A cell affects its neighbours' connections, including diagonals it
		// flanks, so everything within one cell of the area is recalculated.
		for( int row = area.Y1 - 1; row <= area.Y2 + 1; row++ ) {
			for( int column = area.X1 - 1; column <= area.X2 + 1; column++ ) {
				if( !chunks.TryGetCell( column, row, out int slot, out int localIndex ) ) {
					continue;
				}
				result.Cells[result.IndexOf( slot, localIndex )] = Calculate( ref strategy, context, column, row );
			}
		}

		result.ExchangeHalos();
	}

	private static CompiledGridAssembly<TCell> GetCompiled<TCell>(
		ICompiledGridAssembly<TCell> compiledGridAssembly
	) {
		ArgumentNullException.ThrowIfNull( compiledGridAssembly );
		if( compiledGridAssembly is not CompiledGridAssembly<TCell> compiled ) {
			throw new ArgumentException( $"Only assemblies compiled by {nameof( GridCompiler )} are supported.", nameof( compiledGridAssembly ) );
		}
		if( compiled.IsStale ) {
			throw new InvalidOperationException( "The assembly has been attached to or detached from since it was compiled; recompile it." );
		}
		return compiled;
	}

	/// <summary>
	/// Returns the connected directions of an occupied cell.  Neighbours in
	/// other placements are seam contacts and are evaluated with
	/// <c>isSeam</c> set.
	/// </summary>
	private static Direction Calculate<TCell, TStrategy>(
		ref TStrategy strategy,
		Context<TCell> context,
		int column,
		int row
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		TopologyCell<TCell> source = context.GetCell( column, row );
		Direction connected = Direction.None;
		foreach( Direction direction in _neighbours ) {
			Point offset = direction.ToOffset();
			TopologyCell<TCell> destination = context.GetCell( column + offset.X, row + offset.Y );
			if( destination.Placement.IsNone ) {
				continue;
			}
			bool isSeam = destination.Placement != source.Placement;
			if( Evaluate( ref strategy, context, source, destination, direction, isSeam ) ) {
				connected |= direction;
			}
		}
		return connected;
	}

	private static bool Evaluate<TCell, TStrategy>(
		ref TStrategy strategy,
		Context<TCell> context,
		in TopologyCell<TCell> source,
		in TopologyCell<TCell> destination,
		Direction direction,
		bool isSeam
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		TopologyCell<TCell> orthogonalA = default;
		TopologyCell<TCell> orthogonalB = default;
		if( direction.TryGetOrthogonals( out Direction first, out Direction second ) ) {
			Point a = first.ToOffset();
			Point b = second.ToOffset();
			orthogonalA = context.GetCell( source.Column + a.X, source.Row + a.Y );
			orthogonalB = context.GetCell( source.Column + b.X, source.Row + b.Y );
		}
		return strategy.Evaluate( in source, in destination, direction, in orthogonalA, in orthogonalB, isSeam );
	}

	private sealed class Context<TCell>(
		CompiledGridAssembly<TCell> compiled
	) {

		public TopologyCell<TCell> GetCell(
			int column,
			int row
		) {
			if( !compiled.Assembly.TryGetPlacementAt( column, row, out IGridPlacement<TCell>? placement ) ) {
				return new TopologyCell<TCell>( column, row, PlacementId.None, default! );
			}
			TCell cell = placement.Source.GetCell( column - placement.Column, row - placement.Row );
			return new TopologyCell<TCell>( column, row, placement.Id, cell );
		}
	}
}
