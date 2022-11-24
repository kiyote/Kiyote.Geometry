﻿namespace Kiyote.Geometry.Randomization;

/*
 * Derived from code found here:
 * https://observablehq.com/@jrus/bridson-fork/2
 */

internal sealed class FastPoissonDiscPointFactory : IPointFactory {

	public const int K = 13;
	public const int M = 5;
	public const double Sqrt1_2 = 0.7071067811865476;
	public const double Epsilon = 0.0000001;
	private readonly IRandom _random;

	public FastPoissonDiscPointFactory(
		IRandom random
	) {
		_random = random;
	}

	IReadOnlyList<IPoint> IPointFactory.Fill(
		IBounds bounds,
		int distanceApart
	) {
		List<Point> result = new List<Point>();

		int radius2 = distanceApart * distanceApart;
		double cellSize = distanceApart * Sqrt1_2;
		int gridWidth = (int)Math.Ceiling( bounds.Width / cellSize );
		int gridHeight = (int)Math.Ceiling( bounds.Height / cellSize );
		Point?[] grid = new Point[gridWidth * gridHeight];
		List<Point> candidates = new List<Point>();
		double rotx = Math.Cos( 2 * Math.PI * M / K );
		double roty = Math.Sin( 2 * Math.PI * M / K );

		result.Add(
			Sample(
				bounds.Width / 2,
				bounds.Height / 2,
				gridWidth,
				cellSize,
				grid,
				candidates
			)
		);

		while( candidates.Any() ) {
			int i = _random.NextInt( candidates.Count );
			Point parent = candidates[i];
			double t = TanPi2( ( 2.0 * _random.NextDouble() ) - 1.0 );
			double q = 1.0 / ( 1.0 + ( t * t ) );

			double dx = ( 1.0 - ( t * t ) ) * q;
			double dy = 2.0 * t * q;

			bool added = false;
			for( int j = 0; j < K; j++ ) {
				double dw = ( dx * rotx ) - ( dy * roty );
				dy = ( dx * roty ) + ( dy * rotx );
				dx = dw;
				double r = distanceApart * ( 1.0 + ( Epsilon + ( 0.65 * _random.NextDouble() * _random.NextDouble() ) ) );
				int x = (int)( parent.X + ( r * dx ) );
				int y = (int)( parent.Y + ( r * dy ) );

				if( 0 <= x
					&& x < bounds.Width
					&& 0 <= y
					&& y < bounds.Height
					&& Far( x, y, radius2, cellSize, gridWidth, gridHeight, grid )
				) {
					Point p = Sample(
							x,
							y,
							gridWidth,
							cellSize,
							grid,
							candidates
						);
					result.Add( p );
					added = true;
					break;
				}
			}
			if( !added ) {
				int index = candidates.Count - 1;
				Point pr = candidates[index];
				candidates.RemoveAt( index );
				if( i < candidates.Count ) {
					candidates[i] = pr;
				}
			}
		}

		return result;
	}

	private static bool Far(
		int x,
		int y,
		int radius2,
		double cellSize,
		int gridWidth,
		int gridHeight,
		Point?[] grid
	) {
		int di = (int)( x / cellSize );
		int dj = (int)( y / cellSize );
		int i0 = Math.Max( di - 2, 0 );
		int j0 = Math.Max( dj - 2, 0 );
		int i1 = Math.Min( di + 3, gridWidth );
		int j1 = Math.Min( dj + 3, gridHeight );
		for( int j = j0; j < j1; j++ ) {
			int o = j * gridWidth;
			for( int i = i0; i < i1; i++ ) {
				Point? s = grid[o + i];
				if( s is not null ) {
					int dx = s.X - x;
					int dy = s.Y - y;
					if( ( dx * dx ) + ( dy * dy ) < radius2 ) {
						return false;
					}
				}
			}
		}
		return true;
	}

	private static double TanPi2(
		double a
	) {
		double b = 1.0 - ( a * a );
		return a * ( ( -0.0187108 * b ) + 0.31583526 + ( 1.27365776 / b ) );
	}

	private static Point Sample(
		int x,
		int y,
		int gridWidth,
		double cellSize,
		Point?[] grid,
		List<Point> candidates
	) {
		Point s = new Point( x, y );
		int index = ( gridWidth * (int)( y / cellSize ) ) + (int)( x / cellSize );
		grid[index] = s;
		candidates.Add( s );
		return s;

	}
}
