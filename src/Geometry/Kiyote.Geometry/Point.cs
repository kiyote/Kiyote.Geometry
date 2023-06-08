﻿using System.Runtime.CompilerServices;

namespace Kiyote.Geometry;

public readonly struct Point : IEquatable<Point>, ISize, IEquatable<ISize> {
	public readonly static Point None = new Point( int.MinValue, int.MinValue );

	public Point(
		int x,
		int y
	) {
		X = x;
		Y = y;
	}

	public readonly int X { get; }
	public readonly int Y { get; }

	int ISize.Width => X;

	int ISize.Height => Y;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool Equals(
		Point other
	) {
		return ( other.X == X && other.Y == Y );
	}

	public override bool Equals(
		object? obj
	) {
		return obj is Point point && Equals( point );
	}

	public override int GetHashCode() {
		return ( X << 16 ) ^ Y;
	}

	bool IEquatable<ISize>.Equals(
		ISize? other
	) {
		if( other is null ) {
			return false;
		}
		return other.Width == X
			&& other.Height == Y;
	}

	public static bool operator ==( Point left, Point right ) {
		return left.Equals( right );
	}

	public static bool operator !=( Point left, Point right ) {
		return !( left == right );
	}
}
