namespace Kiyote.Geometry.Trees;

/// <summary>
/// An inclusive-bounds quadtree. Item bounds must remain unchanged while indexed.
/// Each insertion is a separate entry, including repeated values.
/// </summary>
public class SimpleQuadTreeNode<T> where T : IRect {

	public const int SmallestAllowedArea = 10;

	private readonly List<T> _contents;
	private readonly List<SimpleQuadTreeNode<T>> _nodes;
	private IReadOnlyList<T>? _readOnlyContents;
	private int _count;

	/// <summary>Constructs a node with an owned snapshot of nonempty bounds.</summary>
	/// <exception cref="InvalidOperationException">The bounds are empty.</exception>
	public SimpleQuadTreeNode(
		IRect bounds
	) {
		ArgumentNullException.ThrowIfNull( bounds );
		Bounds = new Rect( bounds.X1, bounds.Y1, bounds.Width, bounds.Height );
		if( Bounds.IsEmpty ) {
			throw new InvalidOperationException( "Cannot define a quadtree with empty bounds." );
		}
		_contents = [];
		_nodes = new List<SimpleQuadTreeNode<T>>( 4 );
	}

	/// <summary>True when neither this node nor any descendant contains an entry.</summary>
	public bool IsEmpty => _count == 0;

	public Rect Bounds { get; }

	/// <summary>Total inserted entries in this node and its descendants.</summary>
	public int Count => _count;

	/// <summary>Returns a snapshot of entries in this node and its descendants.</summary>
	public IReadOnlyList<T> GetSubTreeContents() {
		if( IsEmpty ) {
			return Array.Empty<T>();
		}
		List<T> results = new List<T>( _count );
		FillContents( results );
		return results;
	}

	/// <summary>A read-only live view of entries stored directly in this node.</summary>
	public IReadOnlyList<T> Contents => _readOnlyContents ??= _contents.AsReadOnly();

	/// <summary>Returns each entry sharing an occupied cell with the query; empty queries return no entries.</summary>
	public IReadOnlyList<T> Query( IRect queryArea ) {
		ArgumentNullException.ThrowIfNull( queryArea );
		if( IsEmpty || queryArea.IsEmpty ) {
			return Array.Empty<T>();
		}
		Rect query = new Rect( queryArea.X1, queryArea.Y1, queryArea.Width, queryArea.Height );
		return Bounds.Overlaps( query ) ? QueryCore( query ) : Array.Empty<T>();
	}

	private IReadOnlyList<T> QueryCore( Rect queryArea ) {
		List<T> results = [];
		foreach( T item in _contents ) {
			if( queryArea.Overlaps( item ) ) {
				results.Add( item );
			}
		}
		foreach( SimpleQuadTreeNode<T> node in _nodes ) {
			if( node.IsEmpty ) {
				continue;
			}
			if( node.Bounds.Contains( queryArea ) ) {
				results.AddRange( node.QueryCore( queryArea ) );
				break;
			}
			if( queryArea.Contains( node.Bounds ) ) {
				node.FillContents( results );
				continue;
			}
			if( node.Bounds.Overlaps( queryArea ) ) {
				results.AddRange( node.QueryCore( queryArea ) );
			}
		}
		return results;
	}

	private void FillContents(
		List<T> contents
	) {
		for( int i = 0; i < _nodes.Count; i++ ) {
			_nodes[i].FillContents( contents );
		}
		contents.AddRange( _contents );
	}

	/// <summary>Inserts a nonempty item completely contained by this node's bounds.</summary>
	/// <exception cref="ArgumentException">The item is empty or extends outside the node; the tree is unchanged.</exception>
	/// <exception cref="OverflowException">The total entry count cannot fit a signed 32-bit integer.</exception>
	public void Insert( T item ) {
		if( item is null ) {
			throw new ArgumentNullException( nameof( item ) );
		}
		if( !Bounds.Contains( item ) ) {
			throw new ArgumentException( "Items must be nonempty and completely contained by the quadtree bounds.", nameof( item ) );
		}
		InsertContained( item );
	}

	private void InsertContained( T item ) {
		int count = checked( _count + 1 );
		if( _nodes.Count == 0 ) {
			CreateSubNodes();
		}
		foreach( SimpleQuadTreeNode<T> node in _nodes ) {
			if( node.Bounds.Contains( item ) ) {
				node.InsertContained( item );
				_count = count;
				return;
			}
		}
		_contents.Add( item );
		_count = count;
	}

	private void CreateSubNodes() {
		int width = Bounds.Width, height = Bounds.Height;
		if( width < 2 || height < 2 || (long)width * height <= SmallestAllowedArea ) {
			return;
		}

		int leftWidth = width / 2, topHeight = height / 2;
		int rightX = Bounds.X1 + leftWidth, bottomY = Bounds.Y1 + topHeight;
		_nodes.Add( new SimpleQuadTreeNode<T>( new Rect( Bounds.X1, Bounds.Y1, leftWidth, topHeight ) ) );
		_nodes.Add( new SimpleQuadTreeNode<T>( new Rect( Bounds.X1, bottomY, leftWidth, height - topHeight ) ) );
		_nodes.Add( new SimpleQuadTreeNode<T>( new Rect( rightX, Bounds.Y1, width - leftWidth, topHeight ) ) );
		_nodes.Add( new SimpleQuadTreeNode<T>( new Rect( rightX, bottomY, width - leftWidth, height - topHeight ) ) );
	}
}
