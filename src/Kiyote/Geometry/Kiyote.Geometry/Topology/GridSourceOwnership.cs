namespace Kiyote.Geometry.Topology;

/// <summary>
/// Tracks exclusive attachment and authorizes assembly-coordinated shape edits.
/// Each source must own one stable instance. Not thread-safe or reentrant.
/// </summary>
public sealed class GridSourceOwnership {
	private bool _editing;

	public bool IsAttached { get; private set; }

	/// <summary>
	/// Custom sources must call this before changing occupancy. Existing-cell
	/// value edits do not require authorization.
	/// </summary>
	public void EnsureShapeChangeAllowed() {
		if( IsAttached && !_editing ) {
			throw new InvalidOperationException( "Change an attached source's shape through its assembly." );
		}
	}

	internal void Attach() {
		if( IsAttached ) {
			throw new InvalidOperationException( "The source is already attached to an assembly." );
		}
		IsAttached = true;
	}

	internal void Detach() {
		IsAttached = false;
	}

	internal void BeginEdit() {
		if( _editing ) {
			throw new InvalidOperationException( "Source shape edits cannot be reentrant." );
		}
		_editing = true;
	}

	internal void EndEdit() {
		_editing = false;
	}
}
