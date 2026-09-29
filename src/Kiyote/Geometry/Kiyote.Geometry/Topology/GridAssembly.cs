using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IGridAssembly{TCell}"/>.  Maintains a cell-level
/// occupancy index so that attach overlap checks, seam detection and per-cell
/// edits are all local lookups.  Not thread-safe.
/// </summary>
public sealed class GridAssembly<TCell> : IGridAssembly<TCell> {

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

	private readonly Dictionary<PlacementId, GridPlacement<TCell>> _placements;
	private readonly List<IGridPlacement<TCell>> _placementList;
	private readonly Dictionary<Point, PlacementId> _occupancy;
	private readonly Dictionary<(PlacementId A, PlacementId B), List<GridContact>> _seams;
	private readonly List<IGridAssemblyObserver> _observers;
	private List<GridSeam>? _seamCache;
	private int _lastId;

	public GridAssembly() {
		_placements = [];
		_placementList = [];
		_occupancy = [];
		_seams = [];
		_observers = [];
	}

	public int Version { get; private set; }

	public Rect? Bounds {
		get {
			Rect? result = null;
			foreach( IGridPlacement<TCell> placement in _placementList ) {
				result = result is Rect r ? r.Union( placement.Bounds ) : placement.Bounds;
			}
			return result;
		}
	}

	public IReadOnlyList<IGridPlacement<TCell>> Placements => _placementList;

	public IReadOnlyList<GridSeam> Seams {
		get {
			if( _seamCache is null ) {
				_seamCache = new List<GridSeam>( _seams.Count );
				foreach( KeyValuePair<(PlacementId A, PlacementId B), List<GridContact>> seam in _seams ) {
					_seamCache.Add( new GridSeam( seam.Key.A, seam.Key.B, [.. seam.Value] ) );
				}
			}
			return _seamCache;
		}
	}

	public AttachResult TryAttach(
		IGridSource<TCell> source,
		int column,
		int row
	) {
		return TryAttach( source, AllocateId(), column, row );
	}

	public AttachResult TryAttach(
		IGridSource<TCell> source,
		PlacementId id,
		int column,
		int row
	) {
		ArgumentNullException.ThrowIfNull( source );
		if( id.IsNone ) {
			throw new ArgumentException( "The placement ID must not be None.", nameof( id ) );
		}
		if( _placements.ContainsKey( id ) ) {
			throw new ArgumentException( $"Placement {id.Value} is already attached.", nameof( id ) );
		}

		List<GridOverlap> overlaps = FindOverlaps( source, column, row );
		if( overlaps.Count > 0 ) {
			return new AttachResult( PlacementId.None, overlaps, [] );
		}

		GridPlacement<TCell> placement = new GridPlacement<TCell>( id, source, column, row );
		_placements[id] = placement;
		_placementList.Add( placement );

		for( int sourceRow = 0; sourceRow < source.Height; sourceRow++ ) {
			foreach( CellRun run in source.GetOccupiedRuns( sourceRow ) ) {
				for( int c = run.Column; c <= run.LastColumn; c++ ) {
					_occupancy[new Point( c + column, sourceRow + row )] = id;
				}
			}
		}

		HashSet<PlacementId> touched = [];
		List<GridContact> contacts = [];
		for( int sourceRow = 0; sourceRow < source.Height; sourceRow++ ) {
			foreach( CellRun run in source.GetOccupiedRuns( sourceRow ) ) {
				for( int c = run.Column; c <= run.LastColumn; c++ ) {
					contacts.Clear();
					AddContacts( id, c + column, sourceRow + row, contacts );
					foreach( GridContact contact in contacts ) {
						PlacementId first = _occupancy[new Point( contact.Column, contact.Row )];
						PlacementId second = _occupancy[new Point( contact.NeighbourColumn, contact.NeighbourRow )];
						touched.Add( first == id ? second : first );
					}
				}
			}
		}

		List<GridSeam> seams = [];
		foreach( PlacementId other in touched ) {
			(PlacementId A, PlacementId B) key = Order( id, other );
			seams.Add( new GridSeam( key.A, key.B, [.. _seams[key]] ) );
		}

		Version++;
		return new AttachResult( id, [], seams );
	}

	public bool TryDetach(
		PlacementId placement
	) {
		if( !_placements.Remove( placement, out GridPlacement<TCell>? existing ) ) {
			return false;
		}
		_placementList.Remove( existing );

		IGridSource<TCell> source = existing.Source;
		for( int sourceRow = 0; sourceRow < source.Height; sourceRow++ ) {
			foreach( CellRun run in source.GetOccupiedRuns( sourceRow ) ) {
				for( int c = run.Column; c <= run.LastColumn; c++ ) {
					_occupancy.Remove( new Point( c + existing.Column, sourceRow + existing.Row ) );
				}
			}
		}

		List<(PlacementId A, PlacementId B)> remove = [];
		foreach( (PlacementId A, PlacementId B) key in _seams.Keys ) {
			if( key.A == placement || key.B == placement ) {
				remove.Add( key );
			}
		}
		foreach( (PlacementId A, PlacementId B) key in remove ) {
			_seams.Remove( key );
		}
		_seamCache = null;

		Version++;
		return true;
	}

	public bool TryGetPlacement(
		PlacementId placement,
		[MaybeNullWhen( false )] out IGridPlacement<TCell> result
	) {
		if( _placements.TryGetValue( placement, out GridPlacement<TCell>? found ) ) {
			result = found;
			return true;
		}
		result = null;
		return false;
	}

	public bool TryGetPlacementAt(
		int column,
		int row,
		[MaybeNullWhen( false )] out IGridPlacement<TCell> result
	) {
		if( _occupancy.TryGetValue( new Point( column, row ), out PlacementId id ) ) {
			result = _placements[id];
			return true;
		}
		result = null;
		return false;
	}

	public bool TryAddCell(
		PlacementId placement,
		int column,
		int row,
		in TCell cell
	) {
		if( !_placements.TryGetValue( placement, out GridPlacement<TCell>? existing )
			|| (uint)column >= (uint)existing.Source.Width
			|| (uint)row >= (uint)existing.Source.Height
			|| existing.Source.IsOccupied( column, row )
		) {
			return false;
		}
		IGridSource<TCell> source = existing.Source;

		Point location = new Point( column + existing.Column, row + existing.Row );
		if( _occupancy.ContainsKey( location )
			|| !source.TrySetCell( column, row, in cell )
		) {
			return false;
		}
		_occupancy[location] = placement;

		List<GridContact> contacts = [];
		AddContacts( placement, location.X, location.Y, contacts );

		foreach( IGridAssemblyObserver observer in _observers ) {
			observer.OnCellAdded( placement, location.X, location.Y, CollectionsMarshal.AsSpan( contacts ) );
		}
		return true;
	}

	public bool TryRemoveCell(
		PlacementId placement,
		int column,
		int row
	) {
		if( !_placements.TryGetValue( placement, out GridPlacement<TCell>? existing )
			|| !existing.Source.IsOccupied( column, row )
		) {
			return false;
		}
		IGridSource<TCell> source = existing.Source;

		Point location = new Point( column + existing.Column, row + existing.Row );
		if( !source.TryClearCell( column, row ) ) {
			return false;
		}
		_occupancy.Remove( location );
		RemoveContacts( placement, location );

		foreach( IGridAssemblyObserver observer in _observers ) {
			observer.OnCellRemoved( placement, location.X, location.Y );
		}
		return true;
	}

	internal void AddObserver(
		IGridAssemblyObserver observer
	) {
		_observers.Add( observer );
	}

	internal void RemoveObserver(
		IGridAssemblyObserver observer
	) {
		_observers.Remove( observer );
	}

	/// <summary>
	/// Returns an identifier not currently used by this assembly.
	/// </summary>
	private PlacementId AllocateId() {
		PlacementId id;
		do {
			_lastId = _lastId == int.MaxValue ? 1 : _lastId + 1;
			id = new PlacementId( _lastId );
		} while( _placements.ContainsKey( id ) );
		return id;
	}

	private List<GridOverlap> FindOverlaps(
		IGridSource<TCell> source,
		int column,
		int row
	) {
		Dictionary<PlacementId, List<CellRun>> cells = [];
		for( int sourceRow = 0; sourceRow < source.Height; sourceRow++ ) {
			foreach( CellRun run in source.GetOccupiedRuns( sourceRow ) ) {
				for( int c = run.Column; c <= run.LastColumn; c++ ) {
					int x = c + column;
					int y = sourceRow + row;
					if( !_occupancy.TryGetValue( new Point( x, y ), out PlacementId other ) ) {
						continue;
					}
					if( !cells.TryGetValue( other, out List<CellRun>? runs ) ) {
						runs = [];
						cells[other] = runs;
					}
					if( runs.Count > 0
						&& runs[^1].Row == y
						&& runs[^1].LastColumn == x - 1
					) {
						CellRun last = runs[^1];
						runs[^1] = last with { Length = last.Length + 1 };
					} else {
						runs.Add( new CellRun( x, y, 1 ) );
					}
				}
			}
		}

		List<GridOverlap> overlaps = new List<GridOverlap>( cells.Count );
		foreach( KeyValuePair<PlacementId, List<CellRun>> entry in cells ) {
			Rect bounds = RunBounds( entry.Value[0] );
			for( int i = 1; i < entry.Value.Count; i++ ) {
				bounds = bounds.Union( RunBounds( entry.Value[i] ) );
			}
			overlaps.Add( new GridOverlap( entry.Key, bounds, entry.Value ) );
		}
		return overlaps;
	}

	private static Rect RunBounds(
		CellRun run
	) {
		return new Rect( run.Column, run.Row, run.Length, 1 );
	}

	/// <summary>
	/// Records a seam contact for every occupied neighbour of the supplied
	/// cell that belongs to a different placement.  Contacts are stored from
	/// the lower placement's side and appended to <paramref name="added"/> in
	/// that form.
	/// </summary>
	private void AddContacts(
		PlacementId placement,
		int column,
		int row,
		List<GridContact> added
	) {
		foreach( Direction direction in _neighbours ) {
			Point offset = direction.ToOffset();
			int nx = column + offset.X;
			int ny = row + offset.Y;
			if( !_occupancy.TryGetValue( new Point( nx, ny ), out PlacementId other )
				|| other == placement
			) {
				continue;
			}

			(PlacementId A, PlacementId B) key = Order( placement, other );
			GridContact contact = key.A == placement
				? new GridContact( column, row, nx, ny, direction )
				: new GridContact( nx, ny, column, row, direction.Opposite() );

			if( !_seams.TryGetValue( key, out List<GridContact>? contacts ) ) {
				contacts = [];
				_seams[key] = contacts;
			}
			contacts.Add( contact );
			added.Add( contact );
		}
		_seamCache = null;
	}

	private void RemoveContacts(
		PlacementId placement,
		Point location
	) {
		List<(PlacementId A, PlacementId B)> empty = [];
		foreach( KeyValuePair<(PlacementId A, PlacementId B), List<GridContact>> seam in _seams ) {
			if( seam.Key.A != placement && seam.Key.B != placement ) {
				continue;
			}
			seam.Value.RemoveAll( c =>
				( c.Column == location.X && c.Row == location.Y )
				|| ( c.NeighbourColumn == location.X && c.NeighbourRow == location.Y )
			);
			if( seam.Value.Count == 0 ) {
				empty.Add( seam.Key );
			}
		}
		foreach( (PlacementId A, PlacementId B) key in empty ) {
			_seams.Remove( key );
		}
		_seamCache = null;
	}

	private static (PlacementId A, PlacementId B) Order(
		PlacementId a,
		PlacementId b
	) {
		return a.Value < b.Value ? (a, b) : (b, a);
	}
}
