namespace Kiyote.Geometry.Topology;

/// <summary>
/// The default <see cref="IGridChunkLayout"/>.  Slots are allocated on demand and
/// are never released, so a slot whose cells are all removed remains
/// allocated with a state of <see cref="ChunkState.Empty"/>.
/// </summary>
internal sealed class GridChunkLayout : IGridChunkLayout {

	private static readonly Direction[] Neighbours = [
		Direction.North,
		Direction.NorthEast,
		Direction.East,
		Direction.SouthEast,
		Direction.South,
		Direction.SouthWest,
		Direction.West,
		Direction.NorthWest
	];

	private readonly Dictionary<Point, int> _slots;
	private readonly List<Point> _chunks;
	private readonly List<ulong[]> _masks;
	private readonly List<int> _counts;
	private readonly List<int[]> _neighbours;
	private readonly int _wordsPerChunk;
	private readonly int _cellsPerChunk;
	private Rect? _bounds;
	private bool _boundsDirty;

	public GridChunkLayout(
		int chunkSize
	) {
		if( chunkSize < 8 || !int.IsPow2( chunkSize ) ) {
			throw new ArgumentOutOfRangeException( nameof( chunkSize ), "Chunk size must be a power of two of at least 8." );
		}
		ChunkSize = chunkSize;
		ChunkShift = int.Log2( chunkSize );
		_cellsPerChunk = chunkSize * chunkSize;
		_wordsPerChunk = _cellsPerChunk >> 6;
		_slots = [];
		_chunks = [];
		_masks = [];
		_counts = [];
		_neighbours = [];
	}

	public int ChunkSize { get; }

	public int ChunkShift { get; }

	public int SlotCount => _chunks.Count;

	public Rect? Bounds {
		get {
			if( _boundsDirty ) {
				_bounds = CalculateBounds();
				_boundsDirty = false;
			}
			return _bounds;
		}
	}

	public ChunkState GetState(
		int slot
	) {
		int count = _counts[slot];
		return count == 0
			? ChunkState.Empty
			: count == _cellsPerChunk
				? ChunkState.Full
				: ChunkState.Partial;
	}

	public Point GetOrigin(
		int slot
	) {
		Point chunk = _chunks[slot];
		return new Point( chunk.X << ChunkShift, chunk.Y << ChunkShift );
	}

	public ReadOnlySpan<ulong> GetValidityMask(
		int slot
	) {
		return _masks[slot];
	}

	public int GetNeighbour(
		int slot,
		Direction direction
	) {
		int index = Array.IndexOf( Neighbours, direction );
		return index < 0 ? -1 : _neighbours[slot][index];
	}

	/// <summary>
	/// Returns the slots of the 8 neighbouring chunks, or -1 where a chunk is
	/// not allocated, in the order North, NorthEast, East, SouthEast, South,
	/// SouthWest, West, NorthWest.
	/// </summary>
	internal ReadOnlySpan<int> GetNeighbours(
		int slot
	) {
		return _neighbours[slot];
	}

	public bool TryGetSlot(
		int chunkColumn,
		int chunkRow,
		out int slot
	) {
		return _slots.TryGetValue( new Point( chunkColumn, chunkRow ), out slot );
	}

	public bool TryGetCell(
		int column,
		int row,
		out int slot,
		out int localIndex
	) {
		if( !TryGetSlot( column >> ChunkShift, row >> ChunkShift, out slot ) ) {
			localIndex = -1;
			return false;
		}
		localIndex = GetLocalIndex( column, row );
		if( !IsSet( slot, localIndex ) ) {
			slot = -1;
			localIndex = -1;
			return false;
		}
		return true;
	}

	internal int GetLocalIndex(
		int column,
		int row
	) {
		int mask = ChunkSize - 1;
		return ( ( row & mask ) << ChunkShift ) + ( column & mask );
	}

	/// <summary>
	/// Returns the slot for the chunk containing the supplied cell, allocating
	/// it and linking its neighbours if required.
	/// </summary>
	internal int GetOrAllocateSlot(
		int column,
		int row
	) {
		Point chunk = new Point( column >> ChunkShift, row >> ChunkShift );
		if( _slots.TryGetValue( chunk, out int slot ) ) {
			return slot;
		}

		slot = _chunks.Count;
		_slots[chunk] = slot;
		_chunks.Add( chunk );
		_masks.Add( new ulong[_wordsPerChunk] );
		_counts.Add( 0 );
		int[] links = new int[Neighbours.Length];
		_neighbours.Add( links );

		for( int i = 0; i < Neighbours.Length; i++ ) {
			Point offset = Neighbours[i].ToOffset();
			if( _slots.TryGetValue( new Point( chunk.X + offset.X, chunk.Y + offset.Y ), out int other ) ) {
				links[i] = other;
				_neighbours[other][Array.IndexOf( Neighbours, Neighbours[i].Opposite() )] = slot;
			} else {
				links[i] = -1;
			}
		}
		return slot;
	}

	internal void SetOccupied(
		int slot,
		int localIndex,
		bool occupied
	) {
		ulong bit = 1UL << localIndex;
		ref ulong word = ref _masks[slot][localIndex >> 6];
		bool current = ( word & bit ) != 0;
		if( current == occupied ) {
			return;
		}
		if( occupied ) {
			word |= bit;
			_counts[slot]++;
		} else {
			word &= ~bit;
			_counts[slot]--;
		}
		_boundsDirty = true;
	}

	private bool IsSet(
		int slot,
		int localIndex
	) {
		return ( _masks[slot][localIndex >> 6] & ( 1UL << localIndex ) ) != 0;
	}

	private Rect? CalculateBounds() {
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;
		for( int slot = 0; slot < _chunks.Count; slot++ ) {
			if( _counts[slot] == 0 ) {
				continue;
			}
			Point origin = GetOrigin( slot );
			for( int local = 0; local < _cellsPerChunk; local++ ) {
				if( !IsSet( slot, local ) ) {
					continue;
				}
				int x = origin.X + ( local & ( ChunkSize - 1 ) );
				int y = origin.Y + ( local >> ChunkShift );
				minX = Math.Min( minX, x );
				minY = Math.Min( minY, y );
				maxX = Math.Max( maxX, x );
				maxY = Math.Max( maxY, y );
			}
		}
		return minX == int.MaxValue
			? null
			: new Rect( new Point( minX, minY ), new Point( maxX, maxY ) );
	}
}
