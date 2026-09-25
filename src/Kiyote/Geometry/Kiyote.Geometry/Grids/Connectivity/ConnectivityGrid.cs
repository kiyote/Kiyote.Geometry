namespace Kiyote.Geometry.Grids.Connectivity;

public sealed class ConnectivityGrid<TCell> : IConnectivityGrid<TCell>, IMutableGrid<Direction> {

	private static readonly (int DeltaColumn, int DeltaRow, Direction Direction)[] _neighbours = [
		( 0, -1, Direction.North ),
		( 1, -1, Direction.NorthEast ),
		( 1, 0, Direction.East ),
		( 1, 1, Direction.SouthEast ),
		( 0, 1, Direction.South ),
		( -1, 1, Direction.SouthWest ),
		( -1, 0, Direction.West ),
		( -1, -1, Direction.NorthWest )
	];

	private readonly CompositeGrid<Direction> _composite;
	private readonly List<Source> _sources;

	public ConnectivityGrid() {
		_composite = new CompositeGrid<Direction>();
		_sources = [];
	}

	Direction IGrid<Direction>.this[int column, int row] => ( (IGrid<Direction>)_composite )[column, row];

	Direction IMutableGrid<Direction>.this[int column, int row] {
		get => ( (IMutableGrid<Direction>)_composite )[column, row];
		set => ( (IMutableGrid<Direction>)_composite )[column, row] = value;
	}

	int IGrid<Direction>.Column => ( (IGrid<Direction>)_composite ).Column;

	int IGrid<Direction>.Row => ( (IGrid<Direction>)_composite ).Row;

	int IGrid<Direction>.Width => ( (IGrid<Direction>)_composite ).Width;

	int IGrid<Direction>.Height => ( (IGrid<Direction>)_composite ).Height;

	IGrid<Direction>? IGrid<Direction>.GetGrid(
		int column,
		int row
	) {
		return ( (IGrid<Direction>)_composite ).GetGrid( column, row );
	}

	IGrid<Direction>? IGrid<Direction>.GetGrid(
		int column,
		int row,
		bool recursive
	) {
		return ( (IGrid<Direction>)_composite ).GetGrid( column, row, recursive );
	}

	void IGrid<Direction>.VisitGrids(
		int column,
		int row,
		Action<IGrid<Direction>, int, int> visitor
	) {
		( (IGrid<Direction>)_composite ).VisitGrids( column, row, visitor );
	}

	bool IGrid<Direction>.TryAttach(
		IGrid<Direction> grid,
		int column,
		int row
	) {
		return ( (IGrid<Direction>)_composite ).TryAttach( grid, column, row );
	}

	bool IGrid<Direction>.TryDetach(
		IGrid<Direction> grid
	) {
		return ( (IGrid<Direction>)_composite ).TryDetach( grid );
	}

	bool IConnectivityGrid<TCell>.TryAttach(
		IGrid<TCell> grid,
		int column,
		int row
	) {
		ArgumentNullException.ThrowIfNull( grid );

		foreach( Source existing in _sources ) {
			if( ReferenceEquals( existing.DataGrid, grid ) ) {
				return false;
			}
		}

		// Cache must be zero-based: CompositeGrid.TryAttach positions a child using
		// `column + grid.Column`, so if Cache already declared its own origin as
		// (column, row) it would end up placed twice as far from the origin.
		RaggedArrayGrid<Direction> cache = new( grid.Width, grid.Height );
		if( !( (IGrid<Direction>)_composite ).TryAttach( cache, column, row ) ) {
			return false;
		}

		// The grid's own Column/Row is its self-declared coordinate frame, which is
		// not necessarily the same as the outer position it's being attached at.
		// Track the offset between the two so we can translate outer coordinates
		// into the grid's own local coordinates when indexing it.
		int offsetColumn = grid.Column - column;
		int offsetRow = grid.Row - row;
		_sources.Add( new Source( grid, cache, column, row, grid.Width, grid.Height, offsetColumn, offsetRow ) );
		return true;
	}

	bool IConnectivityGrid<TCell>.TryDetach(
		IGrid<TCell> grid
	) {
		ArgumentNullException.ThrowIfNull( grid );

		for( int i = 0; i < _sources.Count; i++ ) {
			if( ReferenceEquals( _sources[i].DataGrid, grid ) ) {
				( (IGrid<Direction>)_composite ).TryDetach( _sources[i].Cache );
				_sources.RemoveAt( i );
				return true;
			}
		}
		return false;
	}

	bool IConnectivityGrid<TCell>.UpdateConnectivity<TConnectivityStrategy>(
		TConnectivityStrategy connectivity
	) {
		bool updated = false;
		foreach( Source source in _sources ) {
			updated |= UpdateConnectivity(
				source,
				connectivity,
				source.Column,
				source.Row,
				source.Column + source.Width,
				source.Row + source.Height
			);
		}
		return updated;
	}

	bool IConnectivityGrid<TCell>.UpdateConnectivity<TConnectivityStrategy>(
		TConnectivityStrategy connectivity,
		int startColumn,
		int startRow,
		int endColumn,
		int endRow
	) {
		bool updated = false;
		foreach( Source source in _sources ) {
			updated |= UpdateConnectivity( source, connectivity, startColumn, startRow, endColumn, endRow );
		}
		return updated;
	}

	bool IConnectivityGrid<TCell>.IsConnectedTo<TConnectivityStrategy>(
		TConnectivityStrategy connectivity,
		int sourceColumn,
		int sourceRow,
		int destinationColumn,
		int destinationRow
	) {
		int deltaColumn = destinationColumn - sourceColumn;
		int deltaRow = destinationRow - sourceRow;
		if( Math.Abs( deltaColumn ) > 1
			|| Math.Abs( deltaRow ) > 1
			|| ( deltaColumn == 0 && deltaRow == 0 )
		) {
			return false;
		}

		Direction direction = Direction.None;
		foreach( (int neighbourDeltaColumn, int neighbourDeltaRow, Direction flag) in _neighbours ) {
			if( neighbourDeltaColumn == deltaColumn && neighbourDeltaRow == deltaRow ) {
				direction = flag;
				break;
			}
		}

		GridCell<TCell> sourceCell = new( sourceColumn, sourceRow, FindCell( sourceColumn, sourceRow ) );
		GridCell<TCell> destinationCell = new( destinationColumn, destinationRow, FindCell( destinationColumn, destinationRow ) );
		(GridCell<TCell> orthogonalA, GridCell<TCell> orthogonalB) = FindOrthogonals( sourceColumn, sourceRow, deltaColumn, deltaRow );
		return connectivity.Evaluate( sourceCell, destinationCell, direction, orthogonalA, orthogonalB );
	}

	private TCell? FindCell(
		int column,
		int row
	) {
		foreach( Source source in _sources ) {
			if( Contains( source, column, row ) ) {
				(int localColumn, int localRow) = ToLocalCoordinate( source, column, row );
				return source.DataGrid[localColumn, localRow];
			}
		}
		return default;
	}

	/// <summary>
	/// For a diagonal move described by <paramref name="deltaColumn"/>/<paramref name="deltaRow"/>,
	/// finds the two cells adjacent to <paramref name="column"/>/<paramref name="row"/> along
	/// the orthogonal directions that make up the diagonal. Returns default cells when
	/// the move is not diagonal.
	/// </summary>
	private (GridCell<TCell> OrthogonalA, GridCell<TCell> OrthogonalB) FindOrthogonals(
		int column,
		int row,
		int deltaColumn,
		int deltaRow
	) {
		if( deltaColumn == 0 || deltaRow == 0 ) {
			return (default, default);
		}

		int columnA = column + deltaColumn;
		int rowA = row;
		int columnB = column;
		int rowB = row + deltaRow;

		GridCell<TCell> orthogonalA = new( columnA, rowA, FindCell( columnA, rowA ) );
		GridCell<TCell> orthogonalB = new( columnB, rowB, FindCell( columnB, rowB ) );
		return (orthogonalA, orthogonalB);
	}

	/// <summary>
	/// Determines whether the supplied outer/connectivity-space coordinate falls
	/// within the bounds <paramref name="source"/> was attached at.
	/// </summary>
	private static bool Contains(
		Source source,
		int column,
		int row
	) {
		return column >= source.Column
			&& column < source.Column + source.Width
			&& row >= source.Row
			&& row < source.Row + source.Height;
	}

	/// <summary>
	/// Translates a coordinate expressed in this grid's outer/connectivity space
	/// into the equivalent coordinate in <paramref name="source"/>'s own
	/// (potentially differently-offset) coordinate frame, suitable for indexing
	/// <see cref="Source.DataGrid"/> directly.
	/// </summary>
	private static (int Column, int Row) ToLocalCoordinate(
		Source source,
		int column,
		int row
	) {
		return (column + source.OffsetColumn, row + source.OffsetRow);
	}

	/// <summary>
	/// Translates a coordinate expressed in this grid's outer/connectivity space
	/// into the zero-based coordinate needed to index <paramref name="source"/>'s
	/// own <see cref="Source.Cache"/> directly.
	/// </summary>
	private static (int Column, int Row) ToCacheCoordinate(
		Source source,
		int column,
		int row
	) {
		return (column - source.Column, row - source.Row);
	}

	private bool UpdateConnectivity<TConnectivityStrategy>(
		Source source,
		TConnectivityStrategy connectivity,
		int startColumn,
		int startRow,
		int endColumn,
		int endRow
	) where TConnectivityStrategy : IConnectivityStrategy<TCell> {
		bool updated = false;
		IGrid<TCell> dataGrid = source.DataGrid;

		int left = Math.Max( startColumn, source.Column );
		int top = Math.Max( startRow, source.Row );
		int right = Math.Min( endColumn, source.Column + source.Width );
		int bottom = Math.Min( endRow, source.Row + source.Height );

		for( int row = top; row < bottom; row++ ) {
			for( int column = left; column < right; column++ ) {
				// column/row are in outer/connectivity space; translate to the
				// source's own coordinate frame to read its cell.
				(int localColumn, int localRow) = ToLocalCoordinate( source, column, row );
				GridCell<TCell> sourceCell = new( column, row, dataGrid[localColumn, localRow] );
				Direction direction = Direction.None;

				// For every neighbour of the cell we're examining, check if it's connected to the source cell
				foreach( (int deltaColumn, int deltaRow, Direction flag) in _neighbours ) {
					int neighbourColumn = column + deltaColumn;
					int neighbourRow = row + deltaRow;

					// Find the destination cell, either from the same source or another attached source
					TCell? cell;
					if( Contains( source, neighbourColumn, neighbourRow ) ) {
						(int neighbourLocalColumn, int neighbourLocalRow) = ToLocalCoordinate( source, neighbourColumn, neighbourRow );
						cell = dataGrid[neighbourLocalColumn, neighbourLocalRow];
					} else {
						cell = FindCell( neighbourColumn, neighbourRow );
					}

					GridCell<TCell> destination = new( neighbourColumn, neighbourRow, cell );
					(GridCell<TCell> orthogonalA, GridCell<TCell> orthogonalB) = FindOrthogonals( column, row, deltaColumn, deltaRow );

					// Perform the connectivity evaluation and update the direction if connected
					if( connectivity.Evaluate( sourceCell, destination, flag, orthogonalA, orthogonalB ) ) {
						direction |= flag;
					}
				}

				(int cacheColumn, int cacheRow) = ToCacheCoordinate( source, column, row );
				Direction existing = source.Cache[cacheColumn, cacheRow];
				if( existing != direction ) {
					source.Cache[cacheColumn, cacheRow] = direction;
					updated = true;
				}
			}
		}

		return updated;
	}

	private sealed record Source(
		IGrid<TCell> DataGrid,
		IMutableGrid<Direction> Cache,
		int Column,
		int Row,
		int Width,
		int Height,
		int OffsetColumn,
		int OffsetRow
	);
}
