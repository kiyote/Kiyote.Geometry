namespace Kiyote.Geometry;

public interface IRect: IEquatable<IRect> {
	public Point TopLeft { get; }

	public Point BottomRight { get; }

	public int Width { get; }

	public int Height { get; }

	public bool Contains( int x, int y );

	public bool Contains( Point point );

	public bool Contains( IRect rect );

	public bool Intersects( IRect rect );
}
