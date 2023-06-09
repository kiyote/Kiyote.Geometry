namespace Kiyote.Geometry.Randomization;

/*
 * Derived from code found here:
 * https://observablehq.com/@jrus/bridson-fork/2
 */

internal sealed class FastPoissonDiscPointFactory : IPointFactory {

	public const int K = 13;
	public const int M = 5;
	public const float Sqrt1_2 = 0.7071067811865476F;
	public const float Epsilon = 0.0000001F;
	private readonly IRandom _random;

	public FastPoissonDiscPointFactory(
		IRandom random
	) {
		_random = random;
	}

	IReadOnlyList<Point> IPointFactory.Fill(
		ISize size,
		int distanceApart
	) {
		List<Point> result = new List<Point>();

		int radius2 = distanceApart * distanceApart;
		float cellSize = distanceApart * Sqrt1_2;
		int gridWidth = (int)Math.Ceiling( size.Width / cellSize );
		int gridHeight = (int)Math.Ceiling( size.Height / cellSize );
		float[] grid = new float[ gridWidth * gridHeight * 2 ];
		List<int> candidates = new List<int>();
		float rotx = (float)Math.Cos( 2 * Math.PI * M / K );
		float roty = (float)Math.Sin( 2 * Math.PI * M / K );

		result.Add(
			Sample(
				size.Width / 2,
				size.Height / 2,
				gridWidth,
				cellSize,
				grid,
				candidates
			)
		);

		while( candidates.Any() ) {
			int i = _random.NextInt( candidates.Count );
			int parent = candidates[i];
			float t = TanPi2( ( 2.0F * _random.NextFloat() ) - 1.0F );
			float q = 1.0F / ( 1.0F + ( t * t ) );

			float dx = ( 1.0F - ( t * t ) ) * q;
			float dy = 2.0F * t * q;

			bool added = false;
			for( int j = 0; j < K; j++ ) {
				float dw = ( dx * rotx ) - ( dy * roty );
				dy = ( dx * roty ) + ( dy * rotx );
				dx = dw;
				float r = distanceApart * ( 1.0F + ( Epsilon + ( 0.65F * _random.NextFloat() * _random.NextFloat() ) ) );
				float x = ( grid[parent + 0] + ( r * dx ) );
				float y = ( grid[parent + 1] + ( r * dy ) );

				if( 0 <= x
					&& x < size.Width
					&& 0 <= y
					&& y < size.Height
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
				int pr = candidates[index];
				candidates.RemoveAt( index );
				if( i < candidates.Count ) {
					candidates[i] = pr;
				}
			}
		}

		return result;
	}

	private static bool Far(
		float x,
		float y,
		int radius2,
		float cellSize,
		int gridWidth,
		int gridHeight,
		ReadOnlySpan<float> grid
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
				int index = (o + i) << 1;
				float dx = grid[index + 0] - x;
				float dy = grid[index + 1] - y;
				if( ( dx * dx ) + ( dy * dy ) < radius2 ) {
					return false;
				}
			}
		}
		return true;
	}

	private static float TanPi2(
		float a
	) {
		float b = 1.0F - ( a * a );
		return a * ( ( -0.0187108F * b ) + 0.31583526F + ( 1.27365776F / b ) );
	}

	private static Point Sample(
		float x,
		float y,
		int gridWidth,
		float cellSize,
		Span<float> grid,
		List<int> candidates
	) {
		Point s = new Point( (int)x, (int)y );
		int index = (( gridWidth * (int)( y / cellSize ) ) + (int)( x / cellSize )) * 2;
		grid[index + 0] = x;
		grid[index + 1] = y;
		candidates.Add( index );
		return s;

	}
}
