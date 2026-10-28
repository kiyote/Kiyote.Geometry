using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Testing;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DelaunatorOrderingRegressionTests {
	// Exact ordered triangles, halfedges and hull captured before the O1 experiment at ad343a0.
	[TestCase( "Irregular", "3F81437AD85113BA36230F93710D0C7EFD53130671FF2989D326B4D37115B7FB" )]
	[TestCase( "Thin", "333B23A06F6EC25048751EF6EA2BBAD11460643C8A1FAAB9CBB670259390B8C2" )]
	[TestCase( "MostlyHull", "2F74480D5A75B7DBD1F650E8EA1E0347BD6B11308D27ACA9C68737B9E8DFCDED" )]
	[TestCase( "Sanitized", "DF7BAE20E2E6A0228549B4435A3ECFA4FB3C32851C358CB60550DE507C481BC6" )]
	public void Create_ProfileFixtures_PreservesExactMeshOrdering( string workload, string expected ) {
		D3WorkloadFixture fixture = D3WorkloadFixture.Create( workload );
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter( stream );
		WriteMesh( writer, fixture.Sites );
		string actual = Fingerprint( stream );
		Assert.That( actual, Is.EqualTo( expected ), workload );
	}

	[Test]
	public void Create_SeededFullDomainCorpus_PreservesExactMeshOrdering() {
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter( stream );
		for( int seed = 0; seed < 128; seed++ ) {
			Random random = new Random( seed );
			int count = 3 + (seed * 37 % 1022);
			Point[] sites = new Point[count];
			sites[0] = new Point( -32768, -32768 );
			sites[1] = new Point( 32767, -32768 );
			sites[2] = new Point( -32768, 32767 );
			HashSet<Point> distinct = [sites[0], sites[1], sites[2]];
			for( int i = 3; i < count; i++ ) {
				Point point;
				do {
					point = new Point( random.Next( -32768, 32768 ), random.Next( -32768, 32768 ) );
				} while( !distinct.Add( point ) );
				sites[i] = point;
			}
			WriteMesh( writer, sites );
		}
		string actual = Fingerprint( stream );
		Assert.That( actual, Is.EqualTo( "669528DC6A6ACDE7D78C5051D52A5BD472F623151DA59BD1251D8024E08761BF" ) );
	}

	private static void WriteMesh( BinaryWriter writer, Point[] sites ) {
		writer.Write( sites.Length );
		MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( sites.ToCoords(), true );
		WriteIndices( writer, mesh.Triangles );
		WriteIndices( writer, mesh.HalfEdges );
		WriteIndices( writer, mesh.Hull );
	}

	private static void WriteIndices( BinaryWriter writer, ReadOnlySpan<int> indices ) {
		writer.Write( indices.Length );
		foreach( int index in indices ) {
			writer.Write( index );
		}
	}

	private static string Fingerprint( MemoryStream stream ) {
		return Convert.ToHexString( SHA256.HashData( stream.GetBuffer().AsSpan( 0, (int)stream.Length ) ) );
	}
}
