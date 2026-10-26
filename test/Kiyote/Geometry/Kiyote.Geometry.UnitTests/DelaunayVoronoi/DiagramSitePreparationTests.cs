using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DiagramSitePreparationTests {
	[TestCase( false )]
	[TestCase( true )]
	public void SnapshotAndConvert_ValidSites_OwnsBothRepresentations( bool useList ) {
		Point[] expected = [new( -32768, -32768 ), new( 32767, -32768 ), new( 0, 32767 )];
		IList<Point> input = useList ? expected.ToList() : expected.ToArray();
		Point[] sites = DiagramSites.SnapshotAndConvert( (IReadOnlyList<Point>)input, false, out double[] coordinates );
		input[0] = default;
		Assert.That( sites, Is.EqualTo( expected ) );
		Assert.That( coordinates, Is.EqualTo( new double[] { -32768, -32768, 32767, -32768, 0, 32767 } ) );
	}

	[Test]
	public void SnapshotAndConvert_SanitizedSites_PreservesOrderAndCoordinatePrefix() {
		Point[] input = [new( 2, 2 ), new( 8, 2 ), new( 2, 2 ), new( 99, 99 ), new( 2, 8 )];
		Point[] sites = DiagramSites.SnapshotAndConvert( input, true, out double[] coordinates, new Rect( 0, 0, 11, 11 ) );
		Assert.That( sites, Is.EqualTo( new Point[] { new( 2, 2 ), new( 8, 2 ), new( 2, 8 ) } ) );
		Assert.That( coordinates, Is.EqualTo( new double[] { 2, 2, 8, 2, 2, 8 } ) );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void SnapshotAndConvert_InvalidSiteAfterNoncollinearPrefix_StillValidates( bool sanitize ) {
		Point[] input = [new( 0, 0 ), new( 10, 0 ), new( 0, 10 ), new( 32768, 0 )];
		Assert.That( () => DiagramSites.SnapshotAndConvert( input, sanitize, out _ ),
			Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[TestCase( false )]
	[TestCase( true )]
	public void SnapshotAndConvert_HeavySanitization_AllocatesCoordinatesOnlyForAcceptedSites( bool outsideBounds ) {
		Point[] triangle = [new( 0, 0 ), new( 10, 0 ), new( 0, 10 )];
		Point[] input = new Point[10000];
		triangle.CopyTo( input, 0 );
		for( int i = triangle.Length; i < input.Length; i++ ) {
			input[i] = outsideBounds ? new Point( 1000, 1000 ) : triangle[i % triangle.Length];
		}
		Rect bounds = new Rect( -20, -20, 41, 41 );
		_ = DiagramSites.SnapshotAndConvert( input, true, out _, bounds );
		long before = GC.GetAllocatedBytesForCurrentThread();
		Point[] sites = DiagramSites.SnapshotAndConvert( input, true, out double[] coordinates, bounds );
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( sites, Is.EqualTo( triangle ) );
		Assert.That( coordinates, Has.Length.EqualTo( triangle.Length * 2 ) );
		Assert.That( allocated, Is.LessThanOrEqualTo( (8L * input.Length) + 2048 ) );
	}
}
