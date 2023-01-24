namespace Kiyote.Geometry;

public sealed class Rect : IRect {

	public Rect(
		Point topLeft,
		Point bottomRight
	) {
		TopLeft = topLeft;
		BottomRight = bottomRight;
		Width = bottomRight.X - topLeft.X;
		Height = bottomRight.Y - topLeft.Y;
	}

	public Rect(
		int topX,
		int topY,
		int bottomX,
		int bottomY
	) : this( new Point( topX, topY ), new Point( bottomX, bottomY ) ) {
	}

	public Point TopLeft { get; }

	public Point BottomRight { get; }

	public int Width { get; }

	public int Height { get; }

	public bool Equals(
		Point topLeft,
		Point bottomRight
	) {
		return TopLeft == topLeft && BottomRight == bottomRight;
	}

	public bool Equals(
		IRect? other
	) {
		if( other is null ) {
			return false;
		}

		return Equals( other.TopLeft, other.BottomRight );
	}

	public override bool Equals(
		object? obj
	) {
		if( obj is null ) {
			return false;
		}

		return Equals( obj as IRect );
	}

	public override int GetHashCode() {
		return HashCode.Combine( TopLeft, BottomRight );
	}

	public bool Contains(
		Point point
	) {
		return Contains( point.X, point.Y );
	}

	public bool Contains(
		int x,
		int y
	) {
		if( x >= TopLeft.X
			&& x <= BottomRight.X
			&& y >= TopLeft.Y
			&& y <= BottomRight.Y
		) {
			return true;
		}

		return false;
	}

	public bool Intersects(
		IRect rect
	) {
		return TopLeft.X + Width >= rect.TopLeft.X
			 && TopLeft.X <= rect.TopLeft.X + rect.Width
			 && TopLeft.Y + Height >= rect.TopLeft.Y
			 && TopLeft.Y <= rect.TopLeft.Y + rect.Height;
	}

	public bool Contains(
		IRect rect
	) {
		return TopLeft.X <= rect.TopLeft.X
			&& TopLeft.Y <= rect.TopLeft.Y
			&& BottomRight.X >= rect.BottomRight.X
			&& BottomRight.Y >= rect.BottomRight.Y;
	}
}


