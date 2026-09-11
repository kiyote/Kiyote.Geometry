namespace Kiyote.Geometry.Grids;

public sealed class CompositeGrid<T> : IGrid<T> {

	private readonly List<Attachment> _attachments;
	private int _left;
	private int _top;
	private int _right;
	private int _bottom;
	private int _width;
	private int _height;

	public CompositeGrid() {
		_attachments = [];
	}

	T? IGrid<T>.this[int column, int row] {
		get {
			Attachment? attachment = Find( column, row );
			if( attachment is null ) {
				return default;
			}
			return attachment.Grid[column - attachment.Column, row - attachment.Row];
		}
	}

	IGrid<T>? IGrid<T>.GetGrid(
		int column,
		int row
	) {
		return Find( column, row )?.Grid;
	}

	IGrid<T>? IGrid<T>.GetGrid(
		int column,
		int row,
		bool recursive
	) {
		Attachment? attachment = Find( column, row );
		if( attachment is null ) {
			return null;
		}
		if( !recursive ) {
			return attachment.Grid;
		}
		return attachment.Grid.GetGrid( column - attachment.Column, row - attachment.Row, true )
			?? attachment.Grid;
	}

	void IGrid<T>.VisitGrids(
		int column,
		int row,
		Action<IGrid<T>, int, int> visitor
	) {
		ArgumentNullException.ThrowIfNull( visitor );
		Attachment? attachment = Find( column, row );
		if( attachment is null ) {
			return;
		}

		int childColumn = column - attachment.Column;
		int childRow = row - attachment.Row;
		visitor( attachment.Grid, childColumn, childRow );
		attachment.Grid.VisitGrids( childColumn, childRow, visitor );
	}

	int IGrid<T>.Column => _left;

	int IGrid<T>.Row => _top;

	int IGrid<T>.Width => _width;

	int IGrid<T>.Height => _height;

	bool IGrid<T>.TryAttach(
		IGrid<T> grid,
		int column,
		int row
	) {
		ArgumentNullException.ThrowIfNull( grid );
		if( ReferenceEquals( grid, this ) ) {
			return false;
		}

		int left = column + grid.Column;
		int top = row + grid.Row;
		Attachment candidate = new Attachment(
			grid,
			column,
			row,
			left,
			top,
			left + grid.Width,
			top + grid.Height
		);

		foreach( Attachment attachment in _attachments ) {
			if( ReferenceEquals( attachment.Grid, grid ) ) {
				return false;
			}
			if( candidate.Left < attachment.Right
				&& attachment.Left < candidate.Right
				&& candidate.Top < attachment.Bottom
				&& attachment.Top < candidate.Bottom
			) {
				return false;
			}
		}

		_attachments.Add( candidate );
		UpdateBounds();
		return true;
	}

	bool IGrid<T>.TryDetach(
		IGrid<T> grid
	) {
		ArgumentNullException.ThrowIfNull( grid );
		for( int i = 0; i < _attachments.Count; i++ ) {
			if( ReferenceEquals( _attachments[i].Grid, grid ) ) {
				_attachments.RemoveAt( i );
				UpdateBounds();
				return true;
			}
		}
		return false;
	}

	private void UpdateBounds() {
		int left = int.MaxValue;
		int top = int.MaxValue;
		int right = int.MinValue;
		int bottom = int.MinValue;
		foreach( Attachment attachment in _attachments ) {
			if( attachment.Right <= attachment.Left
				|| attachment.Bottom <= attachment.Top
			) {
				continue;
			}
			left = Math.Min( left, attachment.Left );
			top = Math.Min( top, attachment.Top );
			right = Math.Max( right, attachment.Right );
			bottom = Math.Max( bottom, attachment.Bottom );
		}

		if( right <= left || bottom <= top ) {
			_left = 0;
			_top = 0;
			_right = 0;
			_bottom = 0;
			_width = 0;
			_height = 0;
			return;
		}

		_left = left;
		_top = top;
		_right = right;
		_bottom = bottom;
		_width = right - left;
		_height = bottom - top;
	}

	private Attachment? Find(
		int column,
		int row
	) {
		if( column < _left
			|| column >= _right
			|| row < _top
			|| row >= _bottom
		) {
			return null;
		}

		foreach( Attachment attachment in _attachments ) {
			if( column >= attachment.Left
				&& column < attachment.Right
				&& row >= attachment.Top
				&& row < attachment.Bottom
			) {
				return attachment;
			}
		}
		return null;
	}

	/// <summary>
	/// An <see cref="IGrid{T}"/> attached to this grid, along with its cached
	/// position within this grid's coordinate space.
	/// </summary>
	/// <param name="Grid">The attached grid.</param>
	/// <param name="Column">
	/// The column offset supplied to TryAttach; the column in this grid's coordinate
	/// space that the attached grid's own origin (column 0) maps to.  Used to translate
	/// a coordinate in this grid into a coordinate in the attached grid.
	/// </param>
	/// <param name="Row">
	/// The row offset supplied to TryAttach; the row in this grid's coordinate space
	/// that the attached grid's own origin (row 0) maps to.  Used to translate a
	/// coordinate in this grid into a coordinate in the attached grid.
	/// </param>
	/// <param name="Left">
	/// The inclusive left edge of the attached grid's bounds expressed in this grid's
	/// coordinate space, ie. <paramref name="Column"/> plus the column the attached
	/// grid reports from its Column property.  Equal to <paramref name="Column"/> only
	/// when the attached grid's bounds begin at column 0.  Used for hit testing and
	/// bounds math.
	/// </param>
	/// <param name="Top">
	/// The inclusive top edge of the attached grid's bounds expressed in this grid's
	/// coordinate space, ie. <paramref name="Row"/> plus the row the attached grid
	/// reports from its Row property.  Equal to <paramref name="Row"/> only when the
	/// attached grid's bounds begin at row 0.  Used for hit testing and bounds math.
	/// </param>
	/// <param name="Right">
	/// The exclusive right edge of the attached grid's bounds in this grid's coordinate
	/// space, ie. <paramref name="Left"/> plus the reported width.
	/// </param>
	/// <param name="Bottom">
	/// The exclusive bottom edge of the attached grid's bounds in this grid's coordinate
	/// space, ie. <paramref name="Top"/> plus the reported height.
	/// </param>
	private sealed record Attachment(
		IGrid<T> Grid,
		int Column,
		int Row,
		int Left,
		int Top,
		int Right,
		int Bottom
	);
}
