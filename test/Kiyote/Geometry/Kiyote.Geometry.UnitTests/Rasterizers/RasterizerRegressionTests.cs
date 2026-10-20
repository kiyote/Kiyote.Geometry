using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Rasterizers.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class RasterizerRegressionTests {

	private readonly IRasterizer _rasterizer = new IntegerRasterizer();
	private static readonly Point[] Notch = [
		new Point( 0, 0 ), new Point( 6, 0 ), new Point( 6, 6 ), new Point( 4, 6 ),
		new Point( 4, 2 ), new Point( 2, 2 ), new Point( 2, 6 ), new Point( 0, 6 )
	];

	[TestCase( "span" )]
	[TestCase( "array" )]
	[TestCase( "list" )]
	[TestCase( "immutable" )]
	[TestCase( "opaque" )]
	[TestCase( "delegate" )]
	public void Rasterize_FilledConcaveInput_RejectsBeforeAnyEmission( string source ) {
		Recording output = new Recording();
		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => Rasterize( Notch, output, source, true ), Throws.ArgumentException );
			Assert.That( output.Points, Is.Empty );
			Assert.That( output.Runs, Is.Empty );
		}
	}

	[TestCaseSource( nameof( InvalidBoundaries ) )]
	public void Rasterize_FilledInvalidBoundary_RejectsBeforeAnyEmission( Point[] points ) {
		Recording output = new Recording();
		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => _rasterizer.Rasterize( points.AsSpan(), new Recorder( output ) ), Throws.ArgumentException );
			Assert.That( output.Points, Is.Empty );
			Assert.That( output.Runs, Is.Empty );
		}
	}

	private static IEnumerable<TestCaseData> InvalidBoundaries() {
		Point[][] boundaries = [
			[new Point( 1, 1 )],
			[new Point( 1, 1 ), new Point( 5, 1 )],
			[new Point( 1, 1 ), new Point( 1, 5 )],
			[new Point( 0, 0 ), new Point( 2, 2 ), new Point( 4, 4 )],
			[new Point( 0, 0 ), new Point( 4, 4 ), new Point( 0, 4 ), new Point( 4, 0 )],
			[new Point( 0, 0 ), new Point( 4, 0 ), new Point( 2, 0 ), new Point( 4, 4 ), new Point( 0, 4 )],
			[new Point( 0, 0 ), new Point( 4, 0 ), new Point( 4, 4 ), new Point( 0, 4 ),
			 new Point( 0, 0 ), new Point( 4, 0 ), new Point( 4, 4 ), new Point( 0, 4 )],
			[new Point( 0, 3 ), new Point( 3, -2 ), new Point( -3, 1 ), new Point( 3, 1 ), new Point( -3, -2 )]
		];
		foreach( Point[] points in boundaries ) {
			yield return new TestCaseData( new object[] { points } );
		}
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Rasterize_EmptyInputs_EmitsNothing( bool filled ) {
		Recording output = new Recording();
		foreach( string source in new[] { "span", "array", "list", "immutable", "opaque", "delegate" } ) {
			Rasterize( [], output, source, filled );
		}
		_rasterizer.Rasterize( Polygon.None, new Recorder( output ), filled );
		_rasterizer.Rasterize( Polygon.None, ( x, y ) => output.Points.Add( new Point( x, y ) ), filled );
		Assert.That( output.Points, Is.Empty );
		Assert.That( output.Runs, Is.Empty );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Rasterize_OutOfDomainRawVertex_RejectsBeforeAnyEmission( bool filled ) {
		Point[] points = [new Point( 0, 0 ), new Point( 4, 0 ), new Point( 4, 4 ), new Point( -32769, 4 )];
		foreach( string source in new[] { "span", "array", "list", "immutable", "opaque", "delegate" } ) {
			Recording output = new Recording();
			using( Assert.EnterMultipleScope() ) {
				Assert.That( () => Rasterize( points, output, source, filled ), Throws.TypeOf<ArgumentOutOfRangeException>(), source );
				Assert.That( output.Points, Is.Empty, source );
				Assert.That( output.Runs, Is.Empty, source );
			}
		}
	}

	[TestCase( -32769, 0 )]
	[TestCase( 32768, 0 )]
	[TestCase( 0, -32769 )]
	[TestCase( 0, 32768 )]
	public void Rasterize_OutOfDomainLineEndpoint_RejectsBeforeFirstPixel( int x, int y ) {
		foreach( bool reverse in new[] { false, true } ) {
			int emissions = 0;
			Point a = new Point( 0, 0 ), b = new Point( x, y );
			using( Assert.EnterMultipleScope() ) {
				Assert.That( () => _rasterizer.Rasterize( reverse ? b : a, reverse ? a : b, ( _, _ ) => emissions++ ),
					Throws.TypeOf<ArgumentOutOfRangeException>() );
				Assert.That( emissions, Is.Zero );
			}
		}
	}

	[Test]
	public void Rasterize_DegenerateFilledTriangle_RejectsBeforeEmission() {
		Recording output = new Recording();
		Triangle triangle = new Triangle( new Point( 0, 0 ), new Point( 2, 2 ), new Point( 4, 4 ) );
		using( Assert.EnterMultipleScope() ) {
			Assert.That( () => _rasterizer.Rasterize( triangle, new Recorder( output ) ), Throws.ArgumentException );
			Assert.That( output.Points, Is.Empty );
			Assert.That( output.Runs, Is.Empty );
		}
	}

	[Test]
	public void Rasterize_ConcaveOutline_EmitsBoundaryWithoutFillingNotch() {
		Recording output = new Recording();
		_rasterizer.Rasterize( Notch.AsSpan(), new Recorder( output ), false );
		HashSet<Point> expected = [];
		for( int i = 0; i < Notch.Length; i++ ) {
			Point a = Notch[i], b = Notch[( i + 1 ) % Notch.Length];
			for( int x = Math.Min( a.X, b.X ); x <= Math.Max( a.X, b.X ); x++ ) {
				for( int y = Math.Min( a.Y, b.Y ); y <= Math.Max( a.Y, b.Y ); y++ ) {
					expected.Add( new Point( x, y ) );
				}
			}
		}
		Assert.That( output.Points.Distinct(), Is.EquivalentTo( expected ) );
		Assert.That( output.Points, Does.Not.Contain( new Point( 3, 4 ) ) );
		Assert.That( output.Runs, Is.Empty );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Rasterize_FilledRedundantBoundary_EmitsExactRunsInEitherWinding( bool reverse ) {
		Point[] points = [.. Enumerable.Range( 0, 1001 ).Select( x => new Point( x, 0 ) ),
			new Point( 1000, 2 ), new Point( 0, 2 ), new Point( 0, 0 ), new Point( 0, 0 )];
		if( reverse ) {
			Array.Reverse( points );
		}
		Recording output = new Recording();
		_rasterizer.Rasterize( points.AsSpan(), new Recorder( output ) );
		Assert.That( output.Points, Is.Empty );
		Assert.That( output.Runs, Is.EqualTo( new[] { (0, 1000, 0), (0, 1000, 1), (0, 1000, 2) } ) );
		Recording validated = new Recording();
		_rasterizer.Rasterize( new Polygon( points ), new Recorder( validated ) );
		Assert.That( validated.Runs, Is.EqualTo( output.Runs ) );
	}

	[TestCase( Polygon.MaximumVertices )]
	[TestCase( Polygon.MaximumVertices + 1 )]
	public void Rasterize_FilledBoundary_EnforcesNormalizedVertexLimit( int count ) {
		Point[] points = new Point[count];
		for( int i = 0; i < count; i++ ) {
			double angle = 2 * Math.PI * i / count;
			points[i] = new Point( (int)Math.Round( 10000 * Math.Cos( angle ) ), (int)Math.Round( 10000 * Math.Sin( angle ) ) );
		}
		Recording output = new Recording();
		if( count > Polygon.MaximumVertices ) {
			using( Assert.EnterMultipleScope() ) {
				Assert.That( () => _rasterizer.Rasterize( points.AsSpan(), new Recorder( output ) ), Throws.ArgumentException );
				Assert.That( output.Runs, Is.Empty );
			}
		} else {
			_rasterizer.Rasterize( points.AsSpan(), new Recorder( output ) );
			Assert.That( output.Runs, Has.Count.EqualTo( 20001 ) );
		}
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Rasterize_FullDomainFilledPolygon_Emits65536RunsWithoutPerPixelFallback( bool validated ) {
		Span<long> counts = stackalloc long[4];
		counts.Clear();
		Point[] points = [new Point( -32768, -32768 ), new Point( 32767, -32768 ),
			new Point( 32767, 32767 ), new Point( -32768, 32767 )];
		if( validated ) {
			_rasterizer.Rasterize( new Polygon( points ), new FullDomainSink( counts ) );
		} else {
			_rasterizer.Rasterize( points.AsSpan(), new FullDomainSink( counts ) );
		}
		Assert.That( counts[0], Is.Zero );
		Assert.That( counts[1], Is.EqualTo( 65536 ) );
		Assert.That( counts[2], Is.EqualTo( 4294967296L ) );
		Assert.That( counts[3], Is.Zero );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void Rasterize_FullDomainLine_EmitsEveryDiagonalPixelWithRefStructSink( bool reverse ) {
		Span<long> counts = stackalloc long[4];
		counts.Clear();
		Point a = new Point( -32768, -32768 ), b = new Point( 32767, 32767 );
		_rasterizer.Rasterize( reverse ? b : a, reverse ? a : b, new FullDomainSink( counts ) );
		Assert.That( counts[0], Is.EqualTo( 65536 ) );
		Assert.That( counts[1], Is.Zero );
		Assert.That( counts[3], Is.Zero );
	}

	[Test]
	public void Rasterize_SinkThrows_PropagatesExceptionAndPreservesSubsequentPooledCalls() {
		Point[] points = [new Point( 1, 1 ), new Point( 8, 1 ), new Point( 8, 8 ), new Point( 1, 8 )];
		for( int i = 0; i < 5; i++ ) {
			Assert.That( () => _rasterizer.Rasterize( new OpaquePoints( points ),
				( _, _ ) => throw new InvalidOperationException( "sink failure" ) ),
				Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo( "sink failure" ) );
			Recording output = new Recording();
			Rasterize( points, output, "opaque", true );
			Assert.That( output.Runs, Is.EqualTo( Enumerable.Range( 1, 8 ).Select( y => (1, 8, y) ) ) );
		}
	}

	private void Rasterize( Point[] points, Recording output, string source, bool filled ) {
		Recorder sink = new Recorder( output );
		switch( source ) {
			case "span": _rasterizer.Rasterize( points.AsSpan(), sink, filled ); break;
			case "array": _rasterizer.Rasterize( (IReadOnlyList<Point>)points, sink, filled ); break;
			case "list": _rasterizer.Rasterize( points.ToList(), sink, filled ); break;
			case "immutable": _rasterizer.Rasterize( (IReadOnlyList<Point>)points.ToImmutableArray(), sink, filled ); break;
			case "opaque": _rasterizer.Rasterize( new OpaquePoints( points ), sink, filled ); break;
			case "delegate": _rasterizer.Rasterize( points.AsSpan(), ( x, y ) => output.Points.Add( new Point( x, y ) ), filled ); break;
			default: throw new ArgumentException( "Unknown test input adapter.", nameof( source ) );
		}
	}

	private sealed class Recording {
		public List<Point> Points { get; } = [];
		public List<(int XMin, int XMax, int Y)> Runs { get; } = [];
	}

	private readonly struct Recorder( Recording output ) : IPixelOperation {
		public void Pixel( int x, int y ) => output.Points.Add( new Point( x, y ) );
		public void PixelSpan( int xMin, int xMax, int y ) => output.Runs.Add( (xMin, xMax, y) );
	}

	private readonly ref struct FullDomainSink( Span<long> counts ) : IPixelOperation {
		private readonly Span<long> _counts = counts;

		public void Pixel( int x, int y ) {
			if( x != -32768 + _counts[0] || y != x ) {
				_counts[3]++;
			}
			_counts[0]++;
		}
		public void PixelSpan( int xMin, int xMax, int y ) {
			if( xMin != -32768 || xMax != 32767 || y != -32768 + _counts[1] ) {
				_counts[3]++;
			}
			_counts[1]++;
			_counts[2] += (long)xMax - xMin + 1;
		}
	}

	private sealed class OpaquePoints( IReadOnlyList<Point> points ) : IReadOnlyList<Point> {
		public int Count => points.Count;
		public Point this[int index] => points[index];
		public IEnumerator<Point> GetEnumerator() => points.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
