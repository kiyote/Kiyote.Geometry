namespace Kiyote.Geometry.DelaunayVoronoi;

internal readonly record struct IndexedCell( Cell Cell ) : IRect {
	public bool IsEmpty => Cell.BoundingBox.IsEmpty;
	public int X1 => Cell.BoundingBox.X1;
	public int X2 => Cell.BoundingBox.X2;
	public int Y1 => Cell.BoundingBox.Y1;
	public int Y2 => Cell.BoundingBox.Y2;
	public int Width => Cell.BoundingBox.Width;
	public int Height => Cell.BoundingBox.Height;
	public bool Contains( int x, int y ) => Cell.BoundingBox.Contains( x, y );
	public bool Contains( Point point ) => Cell.BoundingBox.Contains( point );
	public bool Contains( IRect rect ) => Cell.BoundingBox.Contains( rect );
	public bool Overlaps( IRect rect ) => Cell.BoundingBox.Overlaps( rect );
	public bool Touches( IRect rect ) => Cell.BoundingBox.Touches( rect );
	public bool IsEquivalentTo( IRect other ) => Cell.BoundingBox.IsEquivalentTo( other );
	public bool IsEquivalentTo( Point topLeft, Point bottomRight ) => Cell.BoundingBox.IsEquivalentTo( topLeft, bottomRight );
	public bool IsEquivalentTo( int x1, int y1, int x2, int y2 ) => Cell.BoundingBox.IsEquivalentTo( x1, y1, x2, y2 );
}
