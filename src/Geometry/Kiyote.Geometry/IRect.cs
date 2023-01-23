namespace Kiyote.Geometry;

public interface IRect: IEquatable<IRect> {
	public IPoint TopLeft { get; }

	public IPoint BottomRight { get; }

	public int Width { get; }

	public int Height { get; }

	public bool Contains( int x, int y );

	public bool Contains( IPoint point );

	public bool Contains( IRect rect );

	public bool Intersects( IRect rect );
}
