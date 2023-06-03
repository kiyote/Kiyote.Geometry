namespace Kiyote.Geometry;

public sealed class Rect : IEquatable<Rect>, ISize, IRect {

	public Rect(
		Point topLeft,
		Point bottomRight
	) {
		X1 = topLeft.X;
		Y1 = topLeft.Y;
		X2 = bottomRight.X;
		Y2 = bottomRight.Y;
		Width = bottomRight.X - topLeft.X + 1;
		Height = bottomRight.Y - topLeft.Y + 1;
	}

	public Rect(
		int x,
		int y,
		ISize size
	) : this( x, y, size.Width, size.Height ) {
	}

	public Rect(
		int x,
		int y,
		int width,
		int height
	) {
		X1 = x;
		Y1 = y;
		if( width > 0 ) {
			X2 = x + width - 1;
			Y2 = y + height - 1;
		} else if( width == 0 ) {
			X2 = X1;
			Y2 = Y1;
		} else {
			throw new InvalidOperationException( "Rect cannot have negative area." );
		}
		Width = width;
		Height = height;
	}

	public int X1 { get; }
	public int Y1 { get; }

	public int X2 { get; }
	public int Y2 { get; }

	public int Width { get; }

	public int Height { get; }

	public bool Equals(
		Point topLeft,
		Point bottomRight
	) {
		return X1 == topLeft.X
			&& Y1 == topLeft.Y
			&& X2 == bottomRight.X
			&& Y2 == bottomRight.Y;
	}

	public bool Equals(
		Rect? other
	) {
		if( other is null ) {
			return false;
		}

		return ( other.X1 == X1
			&& other.Y1 == Y1
			&& other.X2 == X2
			&& other.Y2 == Y2
		);
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Rect rect && Equals( rect );
	}

	public override int GetHashCode() {
		return HashCode.Combine( X1, Y1, X2, Y2 );
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
		if( x >= X1
			&& x <= X2
			&& y >= Y1
			&& y <= Y2
		) {
			return true;
		}

		return false;
	}

	public bool Intersects(
		IRect rect
	) {
		return X1 + Width >= rect.X1
			 && X1 <= rect.X2
			 && Y1 + Height >= rect.Y1
			 && Y1 <= rect.Y2;
	}

	public bool Contains(
		IRect rect
	) {
		return X1 <= rect.X1
			&& Y1 <= rect.Y1
			&& X2 >= rect.X2
			&& Y2 >= rect.Y2;
	}

	public override string ToString() {
		return $"{X1},{Y1},{X2},{Y2}";
	}
}

