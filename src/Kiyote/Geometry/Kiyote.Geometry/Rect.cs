namespace Kiyote.Geometry;

/// <summary>
/// An inclusive rectangle of occupied cells in signed 32-bit coordinate space.
/// Either zero dimension normalizes to the canonical empty value, <c>default(Rect)</c>.
/// Negative dimensions and unrepresentable bounds/dimensions are rejected.
/// </summary>
public readonly struct Rect : IRect, ISize, IEquatable<Rect> {

	public Rect(
		Point topLeft,
		Point bottomRight
	) {
		X1 = Math.Min( topLeft.X, bottomRight.X );
		X2 = Math.Max( topLeft.X, bottomRight.X );
		Y1 = Math.Min( topLeft.Y, bottomRight.Y );
		Y2 = Math.Max( topLeft.Y, bottomRight.Y );

		long width = (long)X2 - X1 + 1;
		long height = (long)Y2 - Y1 + 1;
		if( width > int.MaxValue || height > int.MaxValue ) {
			throw new ArgumentOutOfRangeException( nameof( bottomRight ), "Rectangle dimensions must fit a signed 32-bit integer." );
		}
		Width = (int)width;
		Height = (int)height;
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
		ArgumentOutOfRangeException.ThrowIfNegative( width );
		ArgumentOutOfRangeException.ThrowIfNegative( height );
		if( width == 0 || height == 0 ) {
			this = default;
			return;
		}
		long x2 = (long)x + width - 1;
		long y2 = (long)y + height - 1;
		if( x2 > int.MaxValue ) {
			throw new ArgumentOutOfRangeException( nameof( width ), "Rectangle X2 must fit a signed 32-bit integer." );
		}
		if( y2 > int.MaxValue ) {
			throw new ArgumentOutOfRangeException( nameof( height ), "Rectangle Y2 must fit a signed 32-bit integer." );
		}
		X1 = x;
		Y1 = y;
		X2 = (int)x2;
		Y2 = (int)y2;
		Width = width;
		Height = height;
	}

	public int X1 { get; }

	public int Y1 { get; }

	public int X2 { get; }

	public int Y2 { get; }

	public int Width { get; }

	public int Height { get; }

	/// <summary>True for the canonical empty rectangle, which has no occupied cells.</summary>
	public bool IsEmpty => Width == 0;

	public bool IsEquivalentTo(
		Rect other
	) {
		return this == other;
	}

	public bool IsEquivalentTo(
		IRect other
	) {
		if( IsEmpty || other.IsEmpty ) {
			return IsEmpty && other.IsEmpty;
		}
		return X1 == other.X1 && Y1 == other.Y1
			&& X2 == other.X2 && Y2 == other.Y2;
	}

	public bool IsEquivalentTo(
		Point topLeft,
		Point bottomRight
	) {
		return IsEquivalentTo(
			topLeft.X,
			topLeft.Y,
			bottomRight.X,
			bottomRight.Y
		);
	}

	public bool IsEquivalentTo(
		int x1,
		int y1,
		int x2,
		int y2
	) {
		int tx1 = Math.Min( x1, x2 );
		int tx2 = Math.Max( x1, x2 );
		int ty1 = Math.Min( y1, y2 );
		int ty2 = Math.Max( y1, y2 );

		return !IsEmpty
			&& X1 == tx1
			&& Y1 == ty1
			&& X2 == tx2
			&& Y2 == ty2;
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
		return !IsEmpty && x >= X1 && x <= X2 && y >= Y1 && y <= Y2;
	}

	public bool Contains(
		Rect rect
	) {
		return !IsEmpty && !rect.IsEmpty
			&& ContainsBounds( rect.X1, rect.Y1, rect.X2, rect.Y2 );
	}

	public bool Contains(
		IRect rect
	) {
		return !IsEmpty && !rect.IsEmpty
			&& ContainsBounds( rect.X1, rect.Y1, rect.X2, rect.Y2 );
	}

	private bool ContainsBounds(
		int x1, int y1, int x2, int y2
	) {
		return X1 <= x1 && Y1 <= y1 && X2 >= x2 && Y2 >= y2;
	}

	public bool Overlaps(
		Rect rect
	) {
		return !IsEmpty && !rect.IsEmpty
			&& OverlapsBounds( rect.X1, rect.Y1, rect.X2, rect.Y2 );
	}

	public bool Overlaps(
		IRect rect
	) {
		return !IsEmpty && !rect.IsEmpty
			&& OverlapsBounds( rect.X1, rect.Y1, rect.X2, rect.Y2 );
	}

	private bool OverlapsBounds(
		int x1, int y1, int x2, int y2
	) {
		return X1 <= x2 && x1 <= X2 && Y1 <= y2 && y1 <= Y2;
	}

	public bool Touches(
		Rect rect
	) {
		return !IsEmpty && !rect.IsEmpty
			&& TouchesBounds( rect.X1, rect.Y1, rect.X2, rect.Y2 );
	}

	public bool Touches(
		IRect rect
	) {
		return !IsEmpty && !rect.IsEmpty
			&& TouchesBounds( rect.X1, rect.Y1, rect.X2, rect.Y2 );
	}

	private bool TouchesBounds(
		int x1, int y1, int x2, int y2
	) {
		return X1 <= (long)x2 + 1 && x1 <= (long)X2 + 1
			&& Y1 <= (long)y2 + 1 && y1 <= (long)Y2 + 1;
	}

	/// <summary>
	/// Computes the cells shared by both rectangles.  Returns false when they
	/// do not overlap; the output is then the canonical empty rectangle.
	/// </summary>
	public static bool TryIntersect(
		Rect a,
		Rect b,
		out Rect intersection
	) {
		if( !a.Overlaps( b ) ) {
			intersection = default;
			return false;
		}

		intersection = new Rect(
			new Point( Math.Max( a.X1, b.X1 ), Math.Max( a.Y1, b.Y1 ) ),
			new Point( Math.Min( a.X2, b.X2 ), Math.Min( a.Y2, b.Y2 ) )
		);
		return true;
	}

	/// <summary>
	/// Computes the cells shared by this rectangle and <paramref name="other"/>.
	/// Returns false and the canonical empty rectangle when they do not overlap.
	/// </summary>
	public bool TryIntersect(
		Rect other,
		out Rect intersection
	) {
		return TryIntersect( this, other, out intersection );
	}

	/// <summary>
	/// Computes the smallest rectangle containing both rectangles.
	/// Empty is the identity; unrepresentable dimensions are rejected.
	/// </summary>
	public static Rect Union(
		Rect a,
		Rect b
	) {
		if( a.IsEmpty ) {
			return b;
		}
		if( b.IsEmpty ) {
			return a;
		}
		return new Rect(
			new Point( Math.Min( a.X1, b.X1 ), Math.Min( a.Y1, b.Y1 ) ),
			new Point( Math.Max( a.X2, b.X2 ), Math.Max( a.Y2, b.Y2 ) )
		);
	}

	/// <summary>
	/// Computes the smallest rectangle containing this rectangle and
	/// <paramref name="other"/>.
	/// </summary>
	public Rect Union(
		Rect other
	) {
		return Union( this, other );
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Rect r && this == r;
	}

	public override int GetHashCode() {
		return HashCode.Combine( X1, Y1, X2, Y2, IsEmpty );
	}

	bool IEquatable<Rect>.Equals(
		Rect other
	) {
		return this == other;
	}

	public static bool operator ==(
		Rect left,
		Rect right
	) {
		return left.IsEmpty == right.IsEmpty
			&& left.X1 == right.X1
			&& left.Y1 == right.Y1
			&& left.X2 == right.X2
			&& left.Y2 == right.Y2;
	}

	public static bool operator !=(
		Rect left,
		Rect right
	) {
		return !( left  ==  right );
	}
}
