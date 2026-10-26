using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.UnitTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class VoronoiCellBufferTests {
	[Test]
	public void Insert_OverlappingTail_MovesEachCompletePairInOrder() {
		Span<double> scratch = stackalloc double[8];
		VoronoiCellBuffer buffer = new VoronoiCellBuffer( scratch );
		buffer.Insert( 0, 3, 4 );
		buffer.Insert( 0, 1, 2 );
		buffer.Insert( 4, 7, 8 );
		buffer.Insert( 4, 5, 6 );
		Assert.That( buffer.Coordinates.ToArray(), Is.EqualTo( new double[] { 1, 2, 3, 4, 5, 6, 7, 8 } ) );
		buffer.RemoveAt( 2 );
		Assert.That( buffer.Coordinates.ToArray(), Is.EqualTo( new double[] { 1, 2, 5, 6, 7, 8 } ) );
		buffer.RemoveAt( 4 );
		buffer.RemoveAt( 0 );
		Assert.That( buffer.Coordinates.ToArray(), Is.EqualTo( new double[] { 5, 6 } ) );
	}

	[Test]
	public void Insert_SeededEdits_MatchesListReference() {
		Span<double> scratch = stackalloc double[280];
		scratch.Fill( double.NaN );
		VoronoiCellBuffer buffer = new VoronoiCellBuffer( scratch );
		List<double> reference = new List<double>();
		Random random = new Random( 8917 );
		for( int i = 0; i < 10000; i++ ) {
			int operation = random.Next( 20 );
			if( operation == 0 ) {
				buffer.Clear();
				reference.Clear();
			} else if( buffer.Count > 0 && (operation < 8 || buffer.Count == scratch.Length) ) {
				int index = random.Next( buffer.Count / 2 ) * 2;
				buffer.RemoveAt( index );
				reference.RemoveRange( index, 2 );
			} else {
				int index = random.Next( buffer.Count / 2 + 1 ) * 2;
				double x = random.NextDouble();
				double y = random.NextDouble();
				buffer.Insert( index, x, y );
				reference.Insert( index, x );
				reference.Insert( index + 1, y );
			}
			Assert.That( buffer.Coordinates.ToArray(), Is.EqualTo( reference ), $"Operation {i}" );
		}
	}

	[Test]
	public void Insert_FullBuffer_ThrowsBeforeChangingEitherCoordinate() {
		double[] scratch = [1, 2, 3, 4];
		Assert.That( () => Overflow( scratch ), Throws.InvalidOperationException.With.Message.Contains( "2 vertices" ) );
		Assert.That( scratch, Is.EqualTo( new double[] { 1, 2, 3, 4 } ) );
	}

	[Test]
	public void Insert_MaximumClippingCapacity_PreservesAllVerticesThroughReuse() {
		Span<double> scratch = stackalloc double[280];
		VoronoiCellBuffer buffer = new VoronoiCellBuffer( scratch );
		for( int i = 0; i < 140; i++ ) {
			buffer.Insert( buffer.Count, i, -i );
		}
		for( int i = 0; i < 140; i++ ) {
			Assert.That( buffer[i * 2], Is.EqualTo( i ) );
			Assert.That( buffer[(i * 2) + 1], Is.EqualTo( -i ) );
		}
		buffer.RemoveAt( 0 );
		buffer.Insert( buffer.Count, 140, -140 );
		Assert.That( buffer.Count, Is.EqualTo( 280 ) );
		Assert.That( buffer[0], Is.EqualTo( 1 ) );
		Assert.That( buffer[279], Is.EqualTo( -140 ) );
	}

	[TestCase( -2 )]
	[TestCase( 1 )]
	[TestCase( 4 )]
	public void Insert_InvalidPairOffset_ThrowsBeforeChangingStorage( int index ) {
		double[] scratch = [1, 2, 3, 4];
		Assert.That( () => InsertAt( scratch, index ), Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( scratch, Is.EqualTo( new double[] { 1, 2, 3, 4 } ) );
	}

	[TestCase( -2 )]
	[TestCase( 1 )]
	[TestCase( 2 )]
	public void RemoveAt_InvalidPairOffset_ThrowsBeforeChangingStorage( int index ) {
		double[] scratch = [1, 2, 3, 4];
		Assert.That( () => RemoveAt( scratch, index ), Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( scratch, Is.EqualTo( new double[] { 1, 2, 3, 4 } ) );
	}

	[Test]
	public void Constructor_OddCoordinateCapacity_RejectsIncompletePairs() {
		Assert.That( () => ConstructOddBuffer(), Throws.ArgumentException );
	}

	[Test]
	public void Clear_DirtyScratchReuse_DoesNotExposePreviousCoordinatesOrAllocate() {
		Span<double> scratch = stackalloc double[280];
		scratch.Fill( double.NaN );
		VoronoiCellBuffer buffer = new VoronoiCellBuffer( scratch );
		long before = GC.GetAllocatedBytesForCurrentThread();
		for( int i = 0; i < 1000; i++ ) {
			buffer.Insert( 0, i, -i );
			buffer.Insert( 0, i + 1, -i - 1 );
			buffer.RemoveAt( 2 );
			buffer.Clear();
		}
		buffer.Insert( 0, 1, 2 );
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( allocated, Is.Zero );
		Assert.That( buffer.Coordinates.ToArray(), Is.EqualTo( new double[] { 1, 2 } ) );
	}

	private static void Overflow( double[] scratch ) {
		VoronoiCellBuffer buffer = new VoronoiCellBuffer( scratch );
		buffer.Insert( 0, 1, 2 );
		buffer.Insert( 2, 3, 4 );
		buffer.Insert( 2, 5, 6 );
	}

	private static void InsertAt( double[] scratch, int index ) {
		VoronoiCellBuffer buffer = new VoronoiCellBuffer( scratch );
		buffer.Insert( 0, 1, 2 );
		buffer.Insert( index, 5, 6 );
	}

	private static void ConstructOddBuffer() {
		Span<double> scratch = stackalloc double[3];
		_ = new VoronoiCellBuffer( scratch );
	}

	private static void RemoveAt( double[] scratch, int index ) {
		VoronoiCellBuffer buffer = new VoronoiCellBuffer( scratch );
		buffer.Insert( 0, 1, 2 );
		buffer.RemoveAt( index );
	}
}
