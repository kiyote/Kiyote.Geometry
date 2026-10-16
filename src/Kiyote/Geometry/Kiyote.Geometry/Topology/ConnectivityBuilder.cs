using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Kiyote.Buffers.ChunkedGrids;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IConnectivityBuilder"/>.
/// </summary>
public sealed class ConnectivityBuilder : IConnectivityBuilder {

	private const int WindowCentre = 4;

	private static readonly Neighbour[] _neighbours = CreateNeighbours();

	public IGridLayer<Direction> Build<TCell, TStrategy>(
		ICompiledGridAssembly<TCell> compiledGridAssembly,
		TStrategy strategy
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		CompiledGridAssembly<TCell> compiled = GetCompiled( compiledGridAssembly );
		ChunkedGridLayout layout = compiled.ChunkLayout;
		GridLayer<Direction> result = new GridLayer<Direction>( layout );

		// TCell is unconstrained, so owners and cells are gathered into pooled
		// arrays, one chunk-sized block per chunk index, rather than layers.
		int size = layout.ChunkSize;
		int area = size * size;
		int length = layout.ChunkCapacity * area;
		int paddedLength = ChunkPadding.GetPaddedLength( size, 1 );
		bool clearCells = RuntimeHelpers.IsReferenceOrContainsReferences<TCell>();
		PlacementId[] owners = ArrayPool<PlacementId>.Shared.Rent( length );
		TCell[] cells = ArrayPool<TCell>.Shared.Rent( length );
		PlacementId[] paddedOwners = ArrayPool<PlacementId>.Shared.Rent( paddedLength );
		TCell[] paddedCells = ArrayPool<TCell>.Shared.Rent( paddedLength );
		try {
			Array.Clear( owners, 0, length );
			Array.Clear( cells, 0, length );
			Load( compiled, owners, cells, area );
			CalculateAll( ref strategy, layout, result.Values, owners, cells, paddedOwners, paddedCells );
		} finally {
			ArrayPool<PlacementId>.Shared.Return( owners );
			ArrayPool<TCell>.Shared.Return( cells, clearCells );
			ArrayPool<PlacementId>.Shared.Return( paddedOwners );
			ArrayPool<TCell>.Shared.Return( paddedCells, clearCells );
		}

		compiled.AddLayer( result );
		return result;
	}

	private static void CalculateAll<TCell, TStrategy>(
		ref TStrategy strategy,
		ChunkedGridLayout layout,
		ChunkedGridLayer<Direction> result,
		PlacementId[] ownerChunks,
		TCell[] cellChunks,
		Span<PlacementId> owners,
		Span<TCell> cells
	) where TStrategy : struct, IConnectivityStrategy<TCell> {
		int size = layout.ChunkSize;
		int shift = layout.ChunkShift;
		int area = size * size;
		int stride = size + 2;
		int wordsPerRow = layout.PresenceWordsPerRow;
		foreach( int chunkIndex in layout.EnumerateChunks() ) {
			ReadOnlySpan<int> neighbours = layout.GetNeighbours( chunkIndex );
			ChunkNeighbourSpans<PlacementId> ownerSpans = GetNeighbourSpans( ownerChunks, neighbours, area );
			ChunkNeighbourSpans<TCell> cellSpans = GetNeighbourSpans( cellChunks, neighbours, area );
			ChunkPadding.Gather( size, 1, in ownerSpans, owners );
			ChunkPadding.Gather( size, 1, in cellSpans, cells );

			Span<Direction> output = result.GetValues( chunkIndex );
			ReadOnlySpan<ulong> presence = layout.GetPresence( chunkIndex );
			int originColumn = layout.GetChunkColumn( chunkIndex ) << shift;
			int originRow = layout.GetChunkRow( chunkIndex ) << shift;
			for( int localRow = 0; localRow < size; localRow++ ) {
				int rowWords = localRow * wordsPerRow;
				for( int word = 0; word < wordsPerRow; word++ ) {
					ulong bits = presence[rowWords + word];
					while( bits != 0 ) {
						int localColumn = ( word << 6 ) + BitOperations.TrailingZeroCount( bits );
						bits &= bits - 1;
						output[( localRow << shift ) + localColumn] = Calculate(
							ref strategy,
							owners,
							cells,
							( ( localRow + 1 ) * stride ) + localColumn + 1,
							stride,
							originColumn + localColumn,
							originRow + localRow
						);
					}
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

		ChunkedGridLayout layout = compiled.ChunkLayout;

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

			ChunkedGridLayer<Direction> output = result.Values;
			for( int row = top + 1; row < top + height - 1; row++ ) {
				for( int column = left + 1; column < left + width - 1; column++ ) {
					if( !layout.TryLocate( column, row, out int chunkIndex, out int localIndex ) ) {
						continue;
					}
					int index = ( ( row - top ) * width ) + ( column - left );
					output.GetValues( chunkIndex )[localIndex] = Calculate(
						ref strategy,
						owners,
						cells,
						index,
						width,
						column,
						row
					);
				}
			}
		} finally {
			ArrayPool<PlacementId>.Shared.Return( owners );
			ArrayPool<TCell>.Shared.Return( cells, RuntimeHelpers.IsReferenceOrContainsReferences<TCell>() );
		}
	}

	/// <summary>
	/// Copies every source run into <paramref name="owners"/> and
	/// <paramref name="cells"/>, where chunk <c>i</c> occupies the
	/// <paramref name="area"/> elements starting at <c>i * area</c>.
	/// </summary>
	private static void Load<TCell>(
		CompiledGridAssembly<TCell> compiled,
		Span<PlacementId> owners,
		Span<TCell> cells,
		int area
	) {
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
			int start = ( run.ChunkIndex * area ) + run.LocalIndex;
			owners.Slice( start, run.Length ).Fill( run.Placement );
			Span<TCell> target = cells.Slice( start, run.Length );
			if( placement.Source.TryGetRow( run.SourceRow, out Span<TCell> row ) ) {
				row.Slice( run.SourceColumn, run.Length ).CopyTo( target );
			} else {
				for( int i = 0; i < run.Length; i++ ) {
					target[i] = placement.Source.GetCell( run.SourceColumn + i, run.SourceRow );
				}
			}
		}
	}

	/// <summary>
	/// Gets the blocks of a chunk and its neighbours from an array holding one
	/// <paramref name="area"/>-sized block per chunk index; absent chunks are empty.
	/// </summary>
	private static ChunkNeighbourSpans<T> GetNeighbourSpans<T>(
		T[] chunks,
		ReadOnlySpan<int> neighbours,
		int area
	) {
		return new ChunkNeighbourSpans<T> {
			NorthWest = GetChunk( chunks, neighbours[0], area ),
			North = GetChunk( chunks, neighbours[1], area ),
			NorthEast = GetChunk( chunks, neighbours[2], area ),
			West = GetChunk( chunks, neighbours[3], area ),
			Centre = GetChunk( chunks, neighbours[4], area ),
			East = GetChunk( chunks, neighbours[5], area ),
			SouthWest = GetChunk( chunks, neighbours[6], area ),
			South = GetChunk( chunks, neighbours[7], area ),
			SouthEast = GetChunk( chunks, neighbours[8], area )
		};
	}

	private static ReadOnlySpan<T> GetChunk<T>(
		T[] chunks,
		int chunkIndex,
		int area
	) {
		return chunkIndex < 0
			? []
			: new ReadOnlySpan<T>( chunks, chunkIndex * area, area );
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
