namespace Kiyote.Geometry.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1814:Prefer jagged arrays over multidimensional", Justification = "Simplicity for test" )]
[TestFixture]
public sealed class RasterizerUnitTests {

	private IRasterizer _rasterizer;

	[SetUp]
	public void SetUp() {
		_rasterizer = new Rasterizer();
	}

	[Test]
	public void Rasterize_HorizontalLine_PointsMatch() {

		List<Point> points = new List<Point>() {
			new Point( 1, 5 ),
			new Point( 8, 5 ),
		};

		bool[,] poly = new bool[10, 10];
		_rasterizer.Rasterize( points, ( int x, int y ) => {
			poly[x, y] = true;
		} );

		bool[,] line = new bool[10, 10];
		_rasterizer.Rasterize( points[0], points[1], ( int x, int y ) => {
			line[x, y] = true;
		} );

		for( int i = 0; i < 10; i++ ) {
			for( int j = 0; j < 10; j++ ) {
				Assert.AreEqual( poly[i, j], line[i, j], $"Rasterize mismatch: {i},{j}, poly: {poly[i, j]}, line: {line[i, j]}" );
			}
		}
	}

	[Test]
	public void Rasterize_VerticalLine_PointsMatch() {

		List<Point> points = new List<Point>() {
			new Point( 5, 1 ),
			new Point( 5, 8 ),
		};

		bool[,] poly = new bool[10, 10];
		_rasterizer.Rasterize( points, ( int x, int y ) => {
			poly[x, y] = true;
		} );

		bool[,] line = new bool[10, 10];
		_rasterizer.Rasterize( points[0], points[1], ( int x, int y ) => {
			line[x, y] = true;
		} );

		for( int i = 0; i < 10; i++ ) {
			for( int j = 0; j < 10; j++ ) {
				Assert.AreEqual( poly[i, j], line[i, j], $"Rasterize mismatch: {i},{j}, poly: {poly[i, j]}, line: {line[i, j]}" );
			}
		}
	}

	[Test]
	public void Rasterize_Box_PointsMatch() {

		List<Point> points = new List<Point>() {
			new Point( 1, 1 ),
			new Point( 8, 1 ),
			new Point( 8, 8 ),
			new Point( 1, 8 ),
		};

		bool[,] poly = new bool[10, 10];
		_rasterizer.Rasterize( points, ( int x, int y ) => {
			poly[x, y] = true;
		} );

		bool[,] line = new bool[10, 10];
		for( int i = 1; i <= 8; i++ ) {
			_rasterizer.Rasterize(
				new Point( 1, i ),
				new Point( 8, i ),
				( int x, int y ) => {
					line[x, y] = true;
				}
			);
		}

		for( int i = 0; i < 10; i++ ) {
			for( int j = 0; j < 10; j++ ) {
				Assert.AreEqual( poly[i, j], line[i, j], $"Rasterize mismatch: {i},{j}, poly: {poly[i, j]}, line: {line[i, j]}" );
			}
		}
	}

	[Test]
	public void Rasterize_SquareRotation_EdgesMatch() {

		const int size = 10;

		List<Point> points = new List<Point>() {
			new Point( 0, 0 ),
			new Point( size-1, 0 ),
			new Point( size-1, size-1 ),
			new Point( 0, size-1 ),
		};

		for( int j = 0; j < size; j++ ) {
			bool[,] line = new bool[size, size];
			// Rasterize the lines
			for( int i = 0; i < points.Count - 1; i++ ) {
				_rasterizer.Rasterize(
					points[i],
					points[i + 1],
					( int x, int y ) => {
						line[x, y] = true;
					}
				);
			}
			_rasterizer.Rasterize(
				points[^1],
				points[0],
				( int x, int y ) => {
					line[x, y] = true;
				}
			);

			// Fill the lines
			for( int y = 0; y < size; y++ ) {
				int minX = int.MaxValue;
				// Find the smallest X
				for( int x = 0; x < size; x++ ) {
					if( line[x, y] ) {
						minX = x;
						break;
					}
				}
				// Find the largest X
				int maxX = int.MinValue;
				for( int x = ( size - 1 ); x >= 0; x-- ) {
					if( line[x, y] ) {
						maxX = x;
						break;
					}
				}

				for( int x = minX; x <= maxX; x++ ) {
					line[x, y] = true;
				}
			}

			bool[,] poly = new bool[size, size];
			_rasterizer.Rasterize( points, ( int x, int y ) => {
				poly[x, y] = true;
			} );

			for( int y = 0; y < size; y++ ) {
				for( int x = 0; x < size; x++ ) {
					Assert.AreEqual( poly[x, y], line[x, y], $"Polygon does not match at {x},{y}: poly {poly[x, y]} vs line {line[x, y]} - iteration {j}." );
				}
			}

			points = new List<Point>() {
				new Point( points[0].X + 1, points[0].Y ),
				new Point( points[1].X, points[1].Y + 1 ),
				new Point( points[2].X - 1, points[2].Y ),
				new Point( points[3].X, points[3].Y - 1 ),
			};
		}
	}

}
