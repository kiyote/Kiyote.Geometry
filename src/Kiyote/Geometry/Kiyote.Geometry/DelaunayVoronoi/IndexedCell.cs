namespace Kiyote.Geometry.DelaunayVoronoi;

internal readonly record struct IndexedCell( int CellIndex, Rect BoundingBox ) : IRect {
	public bool Equals( IndexedCell other ) => CellIndex == other.CellIndex;
	public override int GetHashCode() => CellIndex;

	public bool IsEmpty => BoundingBox.IsEmpty;
	public int X1 => BoundingBox.X1;
	public int X2 => BoundingBox.X2;
	public int Y1 => BoundingBox.Y1;
	public int Y2 => BoundingBox.Y2;
	public int Width => BoundingBox.Width;
	public int Height => BoundingBox.Height;
	public bool Contains( int x, int y ) => BoundingBox.Contains( x, y );
	public bool Contains( Point point ) => BoundingBox.Contains( point );
	public bool Contains( IRect rect ) => BoundingBox.Contains( rect );
	public bool Overlaps( IRect rect ) => BoundingBox.Overlaps( rect );
	public bool Touches( IRect rect ) => BoundingBox.Touches( rect );
	public bool IsEquivalentTo( IRect other ) => BoundingBox.IsEquivalentTo( other );
	public bool IsEquivalentTo( Point topLeft, Point bottomRight ) => BoundingBox.IsEquivalentTo( topLeft, bottomRight );
	public bool IsEquivalentTo( int x1, int y1, int x2, int y2 ) => BoundingBox.IsEquivalentTo( x1, y1, x2, y2 );
}
