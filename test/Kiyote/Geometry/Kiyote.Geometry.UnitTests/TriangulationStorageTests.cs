using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class TriangulationStorageTests {
	[TestCase( 3 )]
	[TestCase( 17 )]
	[TestCase( 400 )]
	[TestCase( 4096 )]
	public void Create_DirtyOversizedScratch_PreservesOwnedResultsAndAllocatesOnlyOutputs( int count ) {
		double[] coords = CreateCoordinates( count );
		MapboxDelaunator first = MapboxDelaunatorFactory.Create( coords, true );
		int[] triangles = first.Triangles.ToArray();
		int[] halfEdges = first.HalfEdges.ToArray();
		int[] hull = first.Hull.ToArray();
		int scratchLength = (4 * count) + (int)Math.Ceiling( Math.Sqrt( count ) );
		int[] scratch = ArrayPool<int>.Shared.Rent( scratchLength );
		double[] distances = ArrayPool<double>.Shared.Rent( count );
		scratch.AsSpan().Fill( int.MinValue );
		distances.AsSpan().Fill( double.NaN );
		ArrayPool<int>.Shared.Return( scratch );
		ArrayPool<double>.Shared.Return( distances );

		long before = GC.GetAllocatedBytesForCurrentThread();
		MapboxDelaunator second = MapboxDelaunatorFactory.Create( coords, true );
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( second.Triangles.ToArray(), Is.EqualTo( triangles ) );
		Assert.That( second.HalfEdges.ToArray(), Is.EqualTo( halfEdges ) );
		Assert.That( second.Hull, Is.EqualTo( hull ) );
		Assert.That( bytes, Is.LessThanOrEqualTo( (8L * MapboxDelaunatorFactory.GetTriangleCapacity( count )) + (4L * hull.Length) + 256 ) );

		Parallel.For( 0, 16, _ => {
			MapboxDelaunator next = MapboxDelaunatorFactory.Create( coords, true );
			Assert.That( next.Triangles.ToArray(), Is.EqualTo( triangles ) );
			Assert.That( next.HalfEdges.ToArray(), Is.EqualTo( halfEdges ) );
		} );
		Assert.That( first.Triangles.ToArray(), Is.EqualTo( triangles ) );
		Assert.That( first.HalfEdges.ToArray(), Is.EqualTo( halfEdges ) );
		Assert.That( first.Hull, Is.EqualTo( hull ) );
	}

	[Test]
	public void Create_AllHullSites_ExposesOnlyLogicalMeshLengths() {
		const int Count = 32;
		double[] coords = new double[Count * 2];
		for( int i = 0; i < Count; i++ ) {
			coords[2 * i] = Math.Round( Math.Cos( i * 2 * Math.PI / Count ) * 10000 );
			coords[(2 * i) + 1] = Math.Round( Math.Sin( i * 2 * Math.PI / Count ) * 10000 );
		}
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( coords, true );
		Assert.That( mesh.Hull, Has.Length.EqualTo( Count ) );
		Assert.That( mesh.Triangles.Length, Is.EqualTo( 3 * ( Count - 2 ) ) );
		Assert.That( mesh.HalfEdges.Length, Is.EqualTo( mesh.Triangles.Length ) );
		for( int i = 0; i < mesh.HalfEdges.Length; i++ ) {
			Assert.That( mesh.Triangles[i], Is.InRange( 0, Count - 1 ) );
			int opposite = mesh.HalfEdges[i];
			if( opposite != -1 ) {
				Assert.That( mesh.HalfEdges[opposite], Is.EqualTo( i ) );
			}
		}
	}

	[Test]
	public void Create_DuplicateRejection_ReturnsBothRentalsAndAllowsReuse() {
		double[] coords = CreateCoordinates( 1000 );
		coords[^2] = coords[0];
		coords[^1] = coords[1];
		for( int i = 0; i < 20; i++ ) {
			ExpectDuplicateRejection( coords );
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		for( int i = 0; i < 100; i++ ) {
			ExpectDuplicateRejection( coords );
		}
		long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		// Losing either rental costs at least 8 KB per call; allow the exception itself.
		Assert.That( bytes, Is.LessThan( 4096 * 100 ) );
		MapboxDelaunator valid = MapboxDelaunatorFactory.Create( CreateCoordinates( 1000 ), true );
		Assert.That( valid.Triangles.Length, Is.GreaterThan( 0 ) );
	}

	private static void ExpectDuplicateRejection( double[] coords ) {
		try {
			MapboxDelaunatorFactory.Create( coords, true );
		} catch( ArgumentException error ) when( error.ParamName == "coords" ) {
			return;
		}
		throw new AssertionException( "Expected duplicate-site rejection." );
	}

	private static double[] CreateCoordinates( int count ) {
		double[] coords = new double[count * 2];
		coords[2] = 1000;
		coords[5] = 1000;
		for( int i = 3; i < count; i++ ) {
			coords[2 * i] = ( i % 64  * 2) + 1;
			coords[(2 * i) + 1] = ( i / 64  * 2) + 1;
		}
		return coords;
	}
}
