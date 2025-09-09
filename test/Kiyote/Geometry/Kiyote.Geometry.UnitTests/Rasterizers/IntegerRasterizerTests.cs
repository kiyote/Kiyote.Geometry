using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kiyote.Geometry.Rasterizers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1814:Prefer jagged arrays over multidimensional", Justification = "Simplicity for test" )]
[TestFixture]
public sealed class IntegerRasterizerTests {

	private IRasterizer _rasterizer;

	[SetUp]
	public void SetUp() {
		_rasterizer = new IntegerRasterizer();
	}

	[Test]
	public void Rasterize_LineAlternatingDirections_PointsMatch() {
		Point p1 = new Point( 3, 2 );
		Point p2 = new Point( 8, 8 );

		bool[,] ltr = new bool[10, 10];
		_rasterizer.Rasterize( p1, p2, ( x, y ) => {
			ltr[x, y] = true;
		} );

		bool[,] rtl = new bool[10, 10];
		_rasterizer.Rasterize( p2, p1, ( x, y ) => {
			rtl[x, y] = true;
		} );

		for( int i = 0; i < 10; i++ ) {
			for( int j = 0; j < 10; j++ ) {
				Assert.That( ltr[i, j], Is.EqualTo( rtl[i, j] ), $"Rasterize mismatch: {i},{j}, ltr: {ltr[i, j]}, rtl: {rtl[i, j]}" );
			}
		}
	}

	[Test]
	public void Rasterize_HorizontalLine_PointsMatch() {

		List<Point> points = [
			new Point( 1, 5 ),
			new Point( 8, 5 ),
		];

		bool[,] poly = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			poly[x, y] = true;
		} );

		bool[,] line = new bool[10, 10];
		_rasterizer.Rasterize( points[0], points[1], ( x, y ) => {
			line[x, y] = true;
		} );

		for( int i = 0; i < 10; i++ ) {
			for( int j = 0; j < 10; j++ ) {
				Assert.That( poly[i, j], Is.EqualTo( line[i, j] ), $"Rasterize mismatch: {i},{j}, poly: {poly[i, j]}, line: {line[i, j]}" );
			}
		}
	}

	[Test]
	public void Rasterize_VerticalLine_PointsMatch() {

		List<Point> points = [
			new Point( 5, 1 ),
			new Point( 5, 8 ),
		];

		bool[,] poly = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			poly[x, y] = true;
		} );

		bool[,] line = new bool[10, 10];
		_rasterizer.Rasterize( points[0], points[1], ( x, y ) => {
			line[x, y] = true;
		} );

		for( int i = 0; i < 10; i++ ) {
			for( int j = 0; j < 10; j++ ) {
				Assert.That( poly[i, j], Is.EqualTo( line[i, j] ), $"Rasterize mismatch: {i},{j}, poly: {poly[i, j]}, line: {line[i, j]}" );
			}
		}
	}

	[Test]
	public void Rasterize_Box_PointsMatch() {

		List<Point> points = [
			new Point( 1, 1 ),
			new Point( 8, 1 ),
			new Point( 8, 8 ),
			new Point( 1, 8 ),
		];

		bool[,] poly = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			poly[x, y] = true;
		} );

		bool[,] line = new bool[10, 10];
		for( int i = 1; i <= 8; i++ ) {
			_rasterizer.Rasterize(
				new Point( 1, i ),
				new Point( 8, i ),
				( x, y ) => {
					line[x, y] = true;
				}
			);
		}

		for( int i = 0; i < 10; i++ ) {
			for( int j = 0; j < 10; j++ ) {
				Assert.That( poly[i, j], Is.EqualTo( line[i, j] ), $"Rasterize mismatch: {i},{j}, poly: {poly[i, j]}, line: {line[i, j]}" );
			}
		}
	}

	[Test]
	public void Rasterize_VoronoiPolygons_EdgesMatch() {
		IPointFactory pointFactory = new FastPoissonDiscPointFactory( new FastRandom() );
		ISize size = new Point( 1000, 1000 );
		IReadOnlyList<Point> voronoiPoints = pointFactory.Fill( size, 5 );
		IVoronoiFactory voronoiFactory = new D3VoronoiFactory();
		IVoronoi voronoi = voronoiFactory.Create( new Rect( 0, 0, size.Width, size.Height ), voronoiPoints, false );
		bool mismatch = false;
		foreach( Cell cell in voronoi.Cells ) {
			int cellWidth = cell.BoundingBox.Width;
			int cellHeight = cell.BoundingBox.Height;
			IReadOnlyList<Point> points = cell.Polygon.Points;
			bool[,] line = new bool[cellWidth, cellHeight];
			// Rasterize the lines
			for( int i = 0; i < points.Count - 1; i++ ) {
				_rasterizer.Rasterize(
					points[i],
					points[i + 1],
					( x, y ) => {
						line[x - cell.BoundingBox.X1, y - cell.BoundingBox.Y1] = true;
					}
				);
			}
			_rasterizer.Rasterize(
				points[^1],
				points[0],
				( x, y ) => {
					line[x - cell.BoundingBox.X1, y - cell.BoundingBox.Y1] = true;
				}
			);

			// Fill the lines
			for( int y = 0; y < cellHeight; y++ ) {
				int minX = int.MaxValue;
				// Find the smallest X
				for( int x = 0; x < cellWidth; x++ ) {
					if( line[x, y] ) {
						minX = x;
						break;
					}
				}
				// Find the largest X
				int maxX = int.MinValue;
				for( int x = cellWidth - 1; x >= 0; x-- ) {
					if( line[x, y] ) {
						maxX = x;
						break;
					}
				}

				if( maxX < minX ) {
					throw new InvalidOperationException();
				}

				for( int x = minX; x <= maxX; x++ ) {
					line[x, y] = true;
				}
			}

			bool[,] poly = new bool[cellWidth, cellHeight];
			_rasterizer.Rasterize( points, ( x, y ) => {
				poly[x - cell.BoundingBox.X1, y - cell.BoundingBox.Y1] = true;
			} );


			for( int y = 0; y < cellHeight; y++ ) {
				for( int x = 0; x < cellWidth; x++ ) {

					if( poly[x, y] != line[x, y] ) {
						using Image<Rgb24> imgLine = new Image<Rgb24>( cell.BoundingBox.Width, cell.BoundingBox.Height );
						using Image<Rgb24> imgPoly = new Image<Rgb24>( cell.BoundingBox.Width, cell.BoundingBox.Height );
						for( int sy = 0; sy < cellHeight; sy++ ) {
							for( int sx = 0; sx < cellWidth; sx++ ) {
								imgLine[sx, sy] = line[sx, sy] ? Color.White : Color.Black;
								imgPoly[sx, sy] = poly[sx, sy] ? Color.White : Color.Black;
							}
						}
						imgLine.SaveAsPng( Path.Combine( "C:\\temp\\Kiyote.Geometry.Visualizer", $"D3DelaunayFactory_Line_{x}-{y}.png" ) );
						imgPoly.SaveAsPng( Path.Combine( "C:\\temp\\Kiyote.Geometry.Visualizer", $"D3DelaunayFactory_Poly_{x}-{y}.png" ) );
						mismatch = true;
						goto next;
					}
					//Assert.AreEqual( poly[x, y], line[x, y], $"Polygon does not match at {x},{y}: poly {poly[x, y]} vs line {line[x, y]}." );
				}
			}
			next:
			poly = null;
			line = null;
		}
		Assert.That( mismatch, Is.False, "Cell mismatches" );
	}

	[Test]
	public void Rasterize_SquareRotation_EdgesMatch() {

		const int size = 50;

		List<Point> points = [
			new Point( 0, 0 ),
			new Point( size - 1, 0 ),
			new Point( size - 1, size - 1 ),
			new Point( 0, size - 1 ),
		];

		for( int j = 0; j < size; j++ ) {
			bool[,] line = new bool[size, size];
			// Rasterize the lines
			for( int i = 0; i < points.Count - 1; i++ ) {
				_rasterizer.Rasterize(
					points[i],
					points[i + 1],
					( x, y ) => {
						line[x, y] = true;
					}
				);
			}
			_rasterizer.Rasterize(
				points[^1],
				points[0],
				( x, y ) => {
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
				for( int x = size - 1; x >= 0; x-- ) {
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
			_rasterizer.Rasterize( points, ( x, y ) => {
				poly[x, y] = true;
			} );

			for( int y = 0; y < size; y++ ) {
				for( int x = 0; x < size; x++ ) {
					Assert.That( poly[x, y], Is.EqualTo( line[x, y] ), $"Polygon does not match at {x},{y}: poly {poly[x, y]} vs line {line[x, y]} - iteration {j}." );
				}
			}

			points = [
				new Point( points[0].X + 1, points[0].Y ),
				new Point( points[1].X, points[1].Y + 1 ),
				new Point( points[2].X - 1, points[2].Y ),
				new Point( points[3].X, points[3].Y - 1 ),
			];
		}
	}

}
