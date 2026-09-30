using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IConnectivityBuilder"/>.
/// </summary>
public sealed class ConnectivityBuilder : IConnectivityBuilder {

	private const int WindowCentre = 4;

	private static readonly Neighbour[] _neighbours = CreateNeighbours();

	private readonly HashSet<int> _changed = [];
	private readonly HashSet<int> _affected = [];

	public IGridLayer<Direction> Build<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		TStrategy strategy,
		int halo
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		CompiledGridAssembly<TCell> compiled = GetCompiled( compiledGridAssembly );
		GridChunkLayout chunks = compiled.GridChunkLayout;
		GridLayer<Direction> result = new GridLayer<Direction>( chunks, halo );
		int length = GridLayer<PlacementId>.GetLength( chunks, 1 );
		PlacementId[] ownerBuffer = ArrayPool<PlacementId>.Shared.Rent( length );
		TCell[] cellBuffer = ArrayPool<TCell>.Shared.Rent( length );
		try {
			GridLayer<PlacementId> owners = new GridLayer<PlacementId>( chunks, 1, ownerBuffer );
			GridLayer<TCell> cells = new GridLayer<TCell>( chunks, 1, cellBuffer );
			CalculateAll( ref strategy, compiled, result, owners, cells );
		} finally {
			ArrayPool<PlacementId>.Shared.Return( ownerBuffer );
			ArrayPool<TCell>.Shared.Return( cellBuffer, RuntimeHelpers.IsReferenceOrContainsReferences<TCell>() );
		}

		result.ExchangeHalos();
		compiled.AddLayer( result );
		return result;
	}

	private static void CalculateAll<TCell, TStrategy>(
		ref TStrategy strategy,
		CompiledGridAssembly<TCell> compiled,
		GridLayer<Direction> result,
		GridLayer<PlacementId> owners,
		GridLayer<TCell> cells
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		GridChunkLayout chunks = compiled.GridChunkLayout;
		Load( compiled, owners, cells );
		owners.ExchangeHalos();
		cells.ExchangeHalos();

		ReadOnlySpan<PlacementId> ownerCells = owners.Cells;
		ReadOnlySpan<TCell> cellValues = cells.Cells;
		Span<Direction> output = result.Cells;
		int size = chunks.ChunkSize;
		int shift = chunks.ChunkShift;
		int stride = owners.Stride;
		for( int slot = 0; slot < chunks.SlotCount; slot++ ) {
			Point origin = chunks.GetOrigin( slot );
			ReadOnlySpan<ulong> mask = chunks.GetValidityMask( slot );
			int baseIndex = slot * owners.ChunkLength;
			for( int word = 0; word < mask.Length; word++ ) {
				ulong bits = mask[word];
				while( bits != 0 ) {
					int local = ( word << 6 ) + BitOperations.TrailingZeroCount( bits );
					bits &= bits - 1;
					int localColumn = local & ( size - 1 );
					int localRow = local >> shift;
					int index = baseIndex + ( ( localRow + 1 ) * stride ) + localColumn + 1;
					output[result.IndexOf( slot, local )] = Calculate(
						ref strategy,
						ownerCells,
						cellValues,
						index,
						stride,
						origin.X + localColumn,
						origin.Y + localRow
					);
				}
			}
		}
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

		// A cell affects its neighbours' connections, including diagonals it
		// flanks, so everything within one cell of the area is recalculated,
		// which in turn requires the cells within two of the area.
		int left = area.X1 - 2;
		int top = area.Y1 - 2;
		int width = area.X2 - area.X1 + 5;
		int height = area.Y2 - area.Y1 + 5;
		int length = width * height;

		PlacementId[] owners = ArrayPool<PlacementId>.Shared.Rent( length );
		TCell[] cells = ArrayPool<TCell>.Shared.Rent( length );
		HashSet<int>? changed = result.Halo > 0 ? _changed : null;
		changed?.Clear();
		try {
			int at = 0;
			for( int row = top; row < top + height; row++ ) {
				for( int column = left; column < left + width; column++ ) {
					if( compiled.Assembly.TryGetPlacementAt( column, row, out IGridPlacement<TCell>? placement ) ) {
						owners[at] = placement.Id;
						cells[at] = placement.Source.GetCell( column - placement.Column, row - placement.Row );
					} else {
						owners[at] = PlacementId.None;
						cells[at] = default!;
					}
					at++;
				}
			}

			Span<Direction> output = result.Cells;
			for( int row = top + 1; row < top + height - 1; row++ ) {
				for( int column = left + 1; column < left + width - 1; column++ ) {
					if( !chunks.TryGetCell( column, row, out int slot, out int localIndex ) ) {
						continue;
					}
					int index = ( ( row - top ) * width ) + ( column - left );
					output[result.IndexOf( slot, localIndex )] = Calculate(
						ref strategy,
						owners,
						cells,
						index,
						width,
						column,
						row
					);
					changed?.Add( slot );
				}
			}
		} finally {
			ArrayPool<PlacementId>.Shared.Return( owners );
			ArrayPool<TCell>.Shared.Return( cells, RuntimeHelpers.IsReferenceOrContainsReferences<TCell>() );
		}

		if( changed is not null ) {
			RefreshHalos( chunks, result, changed, _affected );
		}
	}

	private static void Load<TCell>(
		CompiledGridAssembly<TCell> compiled,
		GridLayer<PlacementId> owners,
		GridLayer<TCell> cells
	) {
		Span<PlacementId> ownerCells = owners.Cells;
		Span<TCell> cellValues = cells.Cells;
		PlacementId currentId = PlacementId.None;
		IGridPlacement<TCell>? placement = null;
		foreach( SourceRun run in compiled.SourceRuns ) {
			if( placement is null || run.Placement != currentId ) {
				if( !compiled.Assembly.TryGetPlacement( run.Placement, out placement ) ) {
					placement = null;
					continue;
				}
				currentId = run.Placement;
			}
			int start = owners.IndexOf( run.Slot, run.LocalIndex );
			ownerCells.Slice( start, run.Length ).Fill( run.Placement );
			Span<TCell> target = cellValues.Slice( start, run.Length );
			if( placement.Source.TryGetRow( run.SourceRow, out Span<TCell> row ) ) {
				row.Slice( run.SourceColumn, run.Length ).CopyTo( target );
			} else {
				for( int i = 0; i < run.Length; i++ ) {
					target[i] = placement.Source.GetCell( run.SourceColumn + i, run.SourceRow );
				}
			}
		}
	}

	private static void RefreshHalos(
		GridChunkLayout chunks,
		GridLayer<Direction> layer,
		HashSet<int> changed,
		HashSet<int> affected
	) {
		affected.Clear();
		foreach( int slot in changed ) {
			affected.Add( slot );
			foreach( int neighbour in chunks.GetNeighbours( slot ) ) {
				if( neighbour >= 0 ) {
					affected.Add( neighbour );
				}
			}
		}
		foreach( int slot in affected ) {
			layer.ExchangeHalo( slot );
		}
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
	/// <c>isSeam</c> set.  <paramref name="index"/> locates the cell within
	/// <paramref name="owners"/> and <paramref name="cells"/>, which must
	/// contain its eight neighbours at a row pitch of <paramref name="stride"/>.
	/// </summary>
	private static Direction Calculate<TCell, TStrategy>(
		ref TStrategy strategy,
		ReadOnlySpan<PlacementId> owners,
		ReadOnlySpan<TCell> cells,
		int index,
		int stride,
		int column,
		int row
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		Window<TCell> window = default;
		int w = 0;
		for( int dy = -1; dy <= 1; dy++ ) {
			int rowIndex = index + ( dy * stride );
			for( int dx = -1; dx <= 1; dx++ ) {
				window[w++] = new TopologyCell<TCell>( column + dx, row + dy, owners[rowIndex + dx], cells[rowIndex + dx] );
			}
		}

		TopologyCell<TCell> none = default;
		ref TopologyCell<TCell> source = ref window[WindowCentre];
		Direction connected = Direction.None;
		foreach( Neighbour neighbour in _neighbours ) {
			ref TopologyCell<TCell> destination = ref window[neighbour.Destination];
			if( destination.Placement.IsNone ) {
				continue;
			}
			bool isSeam = destination.Placement != source.Placement;
			bool result = neighbour.OrthogonalA < 0
				? strategy.Evaluate( in source, in destination, neighbour.Direction, in none, in none, isSeam )
				: strategy.Evaluate( in source, in destination, neighbour.Direction, in window[neighbour.OrthogonalA], in window[neighbour.OrthogonalB], isSeam );
			if( result ) {
				connected |= neighbour.Direction;
			}
		}
		return connected;
	}

	private static Neighbour[] CreateNeighbours() {
		Direction[] directions = [
			Direction.North,
			Direction.NorthEast,
			Direction.East,
			Direction.SouthEast,
			Direction.South,
			Direction.SouthWest,
			Direction.West,
			Direction.NorthWest
		];
		Neighbour[] result = new Neighbour[directions.Length];
		for( int i = 0; i < directions.Length; i++ ) {
			Direction direction = directions[i];
			int a = -1;
			int b = -1;
			if( direction.TryGetOrthogonals( out Direction first, out Direction second ) ) {
				a = ToWindowIndex( first.ToOffset() );
				b = ToWindowIndex( second.ToOffset() );
			}
			result[i] = new Neighbour( direction, ToWindowIndex( direction.ToOffset() ), a, b );
		}
		return result;
	}

	private static int ToWindowIndex(
		Point offset
	) {
		return ( ( offset.Y + 1 ) * 3 ) + offset.X + 1;
	}

	private readonly record struct Neighbour(
		Direction Direction,
		int Destination,
		int OrthogonalA,
		int OrthogonalB
	);

	[InlineArray( 9 )]
	private struct Window<TCell> {
		private TopologyCell<TCell> _element;
	}
}
