using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Rasterizers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1814:Prefer jagged arrays over multidimensional", Justification = "Simplicity for test" )]
[TestFixture]
[ExcludeFromCodeCoverage]
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

	/// <summary>
	/// Records the pixels it is handed.  This is a struct implementing
	/// <see cref="IPixelOperation"/> so that the generic, allocation free path through
	/// the rasterizer is what gets exercised, rather than the delegate wrappers.
	/// </summary>
	private readonly struct GridRecorder(
		bool[,] grid
	) : IPixelOperation {

		public void Pixel(
			int x,
			int y
		) {
			grid[x, y] = true;
		}
	}

	/// <summary>
	/// An <see cref="IReadOnlyList{T}"/> that is deliberately none of the types the
	/// rasterizer has a fast path for, so that the pooled copy fallback is used.
	/// </summary>
	private sealed class OpaquePointList(
		IReadOnlyList<Point> points
	) : IReadOnlyList<Point> {

		public Point this[int index] => points[index];

		public int Count => points.Count;

		public IEnumerator<Point> GetEnumerator() {
			return points.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	private static void AssertGridsMatch(
		bool[,] expected,
		bool[,] actual,
		int size,
		string context
	) {
		for( int x = 0; x < size; x++ ) {
			for( int y = 0; y < size; y++ ) {
				Assert.That( actual[x, y], Is.EqualTo( expected[x, y] ), $"{context} mismatch at {x},{y}: expected {expected[x, y]}, actual {actual[x, y]}." );
			}
		}
	}

	private static List<Point> BoxPoints() {
		return [
			new Point( 1, 1 ),
			new Point( 8, 1 ),
			new Point( 8, 8 ),
			new Point( 1, 8 ),
		];
	}

	[Test]
	public void Rasterize_UnfilledPolygon_MatchesManualEdges() {
		List<Point> points = BoxPoints();

		bool[,] polygon = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			polygon[x, y] = true;
		}, false );

		// Walking the edges by hand is the independent oracle for what an unfilled
		// polygon should produce.
		bool[,] edges = new bool[10, 10];
		for( int i = 0; i < points.Count; i++ ) {
			_rasterizer.Rasterize(
				points[i],
				points[( i + 1 ) % points.Count],
				( x, y ) => {
					edges[x, y] = true;
				}
			);
		}

		AssertGridsMatch( edges, polygon, 10, "Unfilled polygon" );
	}

	[Test]
	public void Rasterize_UnfilledPolygon_DoesNotFillInterior() {
		List<Point> points = BoxPoints();

		bool[,] unfilled = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			unfilled[x, y] = true;
		}, false );

		// The interior of the box must be untouched, otherwise "unfilled" is not
		// actually doing anything.
		Assert.Multiple( () => {
			Assert.That( unfilled[4, 4], Is.False, "Interior pixel should not be set." );
			Assert.That( unfilled[1, 1], Is.True, "Corner pixel should be set." );
			Assert.That( unfilled[8, 8], Is.True, "Corner pixel should be set." );
			Assert.That( unfilled[4, 1], Is.True, "Edge pixel should be set." );
		} );
	}

	[Test]
	public void Rasterize_FilledPolygon_IncludesUnfilledOutline() {
		List<Point> points = BoxPoints();

		bool[,] filled = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			filled[x, y] = true;
		} );

		bool[,] unfilled = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			unfilled[x, y] = true;
		}, false );

		// Every pixel on the outline must also appear in the filled result.
		for( int x = 0; x < 10; x++ ) {
			for( int y = 0; y < 10; y++ ) {
				if( unfilled[x, y] ) {
					Assert.That( filled[x, y], Is.True, $"Filled result is missing outline pixel {x},{y}." );
				}
			}
		}
	}

	[Test]
	public void Rasterize_Triangle_MatchesManualEdges() {
		Point p1 = new Point( 2, 1 );
		Point p2 = new Point( 8, 4 );
		Point p3 = new Point( 4, 8 );
		Triangle triangle = new Triangle( p1, p2, p3 );

		bool[,] fromTriangle = new bool[10, 10];
		_rasterizer.Rasterize( triangle, ( x, y ) => {
			fromTriangle[x, y] = true;
		}, false );

		// This is the call sequence the Triangle overload replaced, so it is the
		// oracle for the collapse being behaviour preserving.
		bool[,] fromLines = new bool[10, 10];
		_rasterizer.Rasterize( p1, p2, ( x, y ) => {
			fromLines[x, y] = true;
		} );
		_rasterizer.Rasterize( p2, p3, ( x, y ) => {
			fromLines[x, y] = true;
		} );
		_rasterizer.Rasterize( p3, p1, ( x, y ) => {
			fromLines[x, y] = true;
		} );

		AssertGridsMatch( fromLines, fromTriangle, 10, "Triangle outline" );
	}

	[Test]
	public void Rasterize_FilledTriangle_MatchesEquivalentPolygon() {
		Point p1 = new Point( 2, 1 );
		Point p2 = new Point( 8, 4 );
		Point p3 = new Point( 4, 8 );

		bool[,] fromTriangle = new bool[10, 10];
		_rasterizer.Rasterize( new Triangle( p1, p2, p3 ), ( x, y ) => {
			fromTriangle[x, y] = true;
		} );

		bool[,] fromPolygon = new bool[10, 10];
		_rasterizer.Rasterize( new List<Point> { p1, p2, p3 }, ( x, y ) => {
			fromPolygon[x, y] = true;
		} );

		AssertGridsMatch( fromPolygon, fromTriangle, 10, "Filled triangle" );
	}

	[Test]
	public void Rasterize_Polygon_MatchesItsPoints() {
		List<Point> points = BoxPoints();
		Polygon polygon = new Polygon( points );

		bool[,] fromPolygon = new bool[10, 10];
		_rasterizer.Rasterize( polygon, ( x, y ) => {
			fromPolygon[x, y] = true;
		} );

		bool[,] fromPoints = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromPoints[x, y] = true;
		} );

		AssertGridsMatch( fromPoints, fromPolygon, 10, "Polygon overload" );
	}

	[Test]
	public void Rasterize_PolygonUnfilled_MatchesItsPoints() {
		List<Point> points = BoxPoints();
		Polygon polygon = new Polygon( points );

		bool[,] fromPolygon = new bool[10, 10];
		_rasterizer.Rasterize( polygon, ( x, y ) => {
			fromPolygon[x, y] = true;
		}, false );

		bool[,] fromPoints = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromPoints[x, y] = true;
		}, false );

		AssertGridsMatch( fromPoints, fromPolygon, 10, "Unfilled polygon overload" );
	}

	[Test]
	public void Rasterize_PointArray_MatchesList() {
		List<Point> points = BoxPoints();

		bool[,] fromArray = new bool[10, 10];
		_rasterizer.Rasterize( points.ToArray(), ( x, y ) => {
			fromArray[x, y] = true;
		} );

		bool[,] fromList = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromList[x, y] = true;
		} );

		AssertGridsMatch( fromList, fromArray, 10, "Array fast path" );
	}

	[Test]
	public void Rasterize_ImmutableArray_MatchesList() {
		List<Point> points = BoxPoints();

		bool[,] fromImmutable = new bool[10, 10];
		_rasterizer.Rasterize( [.. points], ( x, y ) => {
			fromImmutable[x, y] = true;
		} );

		bool[,] fromList = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromList[x, y] = true;
		} );

		AssertGridsMatch( fromList, fromImmutable, 10, "ImmutableArray fast path" );
	}

	[Test]
	public void Rasterize_UnknownListType_MatchesList() {
		List<Point> points = BoxPoints();

		// OpaquePointList has no fast path, so this exercises the pooled copy.
		bool[,] fromOpaque = new bool[10, 10];
		_rasterizer.Rasterize( new OpaquePointList( points ), ( x, y ) => {
			fromOpaque[x, y] = true;
		} );

		bool[,] fromList = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromList[x, y] = true;
		} );

		AssertGridsMatch( fromList, fromOpaque, 10, "Pooled fallback" );
	}

	[Test]
	public void Rasterize_UnknownListTypeReused_MatchesList() {
		List<Point> points = BoxPoints();
		OpaquePointList opaque = new OpaquePointList( points );

		// Rasterizing repeatedly returns and re-rents the pooled buffer, so a stale
		// or incorrectly sized rental would show up here.
		bool[,] fromList = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromList[x, y] = true;
		} );

		for( int i = 0; i < 5; i++ ) {
			bool[,] fromOpaque = new bool[10, 10];
			_rasterizer.Rasterize( opaque, ( x, y ) => {
				fromOpaque[x, y] = true;
			} );

			AssertGridsMatch( fromList, fromOpaque, 10, $"Pooled fallback iteration {i}" );
		}
	}

	[Test]
	public void Rasterize_Span_MatchesList() {
		List<Point> points = BoxPoints();

		bool[,] fromSpan = new bool[10, 10];
		ReadOnlySpan<Point> span = [
			new Point( 1, 1 ),
			new Point( 8, 1 ),
			new Point( 8, 8 ),
			new Point( 1, 8 ),
		];
		_rasterizer.Rasterize( span, ( x, y ) => {
			fromSpan[x, y] = true;
		} );

		bool[,] fromList = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromList[x, y] = true;
		} );

		AssertGridsMatch( fromList, fromSpan, 10, "Span primitive" );
	}

	[Test]
	public void Rasterize_StrucTPixelOperation_MatchesDelegate() {
		List<Point> points = BoxPoints();

		// The struct path is the reason the generic API exists, so it must agree
		// with the delegate path it replaced.
		bool[,] fromStruct = new bool[10, 10];
		_rasterizer.Rasterize( points, new GridRecorder( fromStruct ) );

		bool[,] fromDelegate = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromDelegate[x, y] = true;
		} );

		AssertGridsMatch( fromDelegate, fromStruct, 10, "Struct pixel action" );
	}

	[Test]
	public void Rasterize_StrucTPixelOperationUnfilled_MatchesDelegate() {
		List<Point> points = BoxPoints();

		bool[,] fromStruct = new bool[10, 10];
		_rasterizer.Rasterize( points, new GridRecorder( fromStruct ), false );

		bool[,] fromDelegate = new bool[10, 10];
		_rasterizer.Rasterize( points, ( x, y ) => {
			fromDelegate[x, y] = true;
		}, false );

		AssertGridsMatch( fromDelegate, fromStruct, 10, "Unfilled struct pixel action" );
	}

	[Test]
	public void Rasterize_StrucTPixelOperationLine_MatchesDelegate() {
		Point p1 = new Point( 3, 2 );
		Point p2 = new Point( 8, 8 );

		bool[,] fromStruct = new bool[10, 10];
		_rasterizer.Rasterize( p1, p2, new GridRecorder( fromStruct ) );

		bool[,] fromDelegate = new bool[10, 10];
		_rasterizer.Rasterize( p1, p2, ( x, y ) => {
			fromDelegate[x, y] = true;
		} );

		AssertGridsMatch( fromDelegate, fromStruct, 10, "Struct pixel action line" );
	}

	[Test]
	public void Rasterize_TallPolygon_MatchesManualEdges() {
		// A polygon taller than the small test cases pushes the pooled scanline
		// buffer past the size the earlier tests exercise.
		const int size = 200;
		List<Point> points = [
			new Point( 10, 5 ),
			new Point( 150, 20 ),
			new Point( 120, 190 ),
			new Point( 20, 160 ),
		];

		bool[,] unfilled = new bool[size, size];
		_rasterizer.Rasterize( points, ( x, y ) => {
			unfilled[x, y] = true;
		}, false );

		bool[,] edges = new bool[size, size];
		for( int i = 0; i < points.Count; i++ ) {
			_rasterizer.Rasterize(
				points[i],
				points[( i + 1 ) % points.Count],
				( x, y ) => {
					edges[x, y] = true;
				}
			);
		}

		AssertGridsMatch( edges, unfilled, size, "Tall polygon outline" );
	}

	/// <summary>
	/// Records via an overridden <see cref="IPixelOperation.PixelSpan"/> so the run
	/// based path can be compared against the default per-pixel implementation.
	/// </summary>
	private readonly struct SpanGridRecorder(
		bool[,] grid,
		List<int> runLengths
	) : IPixelOperation {

		public void Pixel(
			int x,
			int y
		) {
			grid[x, y] = true;
		}

		public void PixelSpan(
			int xMin,
			int xMax,
			int y
		) {
			runLengths.Add( xMax - xMin + 1 );
			for( int x = xMin; x <= xMax; x++ ) {
				grid[x, y] = true;
			}
		}
	}

	[Test]
	public void Rasterize_OverriddenPixelSpan_MatchesDefaultPixelSpan() {
		List<Point> points = BoxPoints();

		// A sink that overrides PixelSpan must observe exactly the same pixels as
		// one that inherits the default per-pixel implementation.
		bool[,] fromSpan = new bool[10, 10];
		List<int> runs = [];
		_rasterizer.Rasterize( points, new SpanGridRecorder( fromSpan, runs ) );

		bool[,] fromPixel = new bool[10, 10];
		_rasterizer.Rasterize( points, new GridRecorder( fromPixel ) );

		AssertGridsMatch( fromPixel, fromSpan, 10, "Overridden PixelSpan" );
		Assert.That( runs, Is.Not.Empty, "Filled rasterization should emit runs." );
		Assert.That( runs, Has.All.GreaterThan( 0 ), "Runs should never be empty." );
	}

	[Test]
	public void Rasterize_HorizontalLinePolygon_EmitsSingleRun() {
		// The delta_y == 1 special case should hand over one run rather than
		// looping a pixel at a time.
		List<Point> points = [
			new Point( 1, 5 ),
			new Point( 8, 5 ),
		];

		bool[,] fromSpan = new bool[10, 10];
		List<int> runs = [];
		_rasterizer.Rasterize( points, new SpanGridRecorder( fromSpan, runs ) );

		bool[,] fromPixel = new bool[10, 10];
		_rasterizer.Rasterize( points, new GridRecorder( fromPixel ) );

		AssertGridsMatch( fromPixel, fromSpan, 10, "Horizontal line run" );
		Assert.That( runs, Has.Count.EqualTo( 1 ) );
		Assert.That( runs[0], Is.EqualTo( 8 ) );
	}

	[Test]
	public void Rasterize_UnfilledOverriddenPixelSpan_MatchesDefault() {
		List<Point> points = BoxPoints();

		// Outline tracing walks diagonally, so it must still go through Pixel and
		// produce the same result as the default sink.
		bool[,] fromSpan = new bool[10, 10];
		List<int> runs = [];
		_rasterizer.Rasterize( points, new SpanGridRecorder( fromSpan, runs ), false );

		bool[,] fromPixel = new bool[10, 10];
		_rasterizer.Rasterize( points, new GridRecorder( fromPixel ), false );

		AssertGridsMatch( fromPixel, fromSpan, 10, "Unfilled overridden PixelSpan" );
	}

	[Test]
	public void Rasterize_TallPolygonOverriddenPixelSpan_MatchesDefault() {
		const int size = 200;
		List<Point> points = [
			new Point( 10, 5 ),
			new Point( 150, 20 ),
			new Point( 120, 190 ),
			new Point( 20, 160 ),
		];

		// The pooled scanline path is where the run based fill actually pays off,
		// so it needs the same equivalence guarantee.
		bool[,] fromSpan = new bool[size, size];
		List<int> runs = [];
		_rasterizer.Rasterize( points, new SpanGridRecorder( fromSpan, runs ) );

		bool[,] fromPixel = new bool[size, size];
		_rasterizer.Rasterize( points, new GridRecorder( fromPixel ) );

		AssertGridsMatch( fromPixel, fromSpan, size, "Tall polygon overridden PixelSpan" );
	}
}
