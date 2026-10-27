using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;
using Kiyote.Geometry.Testing;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class D3ProfileWorkloadTests {
	[TestCase( "Irregular", 4096 )]
	[TestCase( "Thin", 512 )]
	[TestCase( "MostlyHull", 128 )]
	[TestCase( "Sanitized", 400 )]
	public void Create_ProfileWorkload_PreservesCompleteGeometryAndReciprocalAdjacency( string workload, int count ) {
		D3WorkloadFixture fixture = D3WorkloadFixture.Create( workload );
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi result = factory.Create( fixture.Bounds, fixture.Input, fixture.Sanitize );
		Assert.That( result.Cells.Length, Is.EqualTo( count ) );
		fixture.Validate( result );
		int vertexCount = 0;
		for( int i = 0; i < count; i++ ) {
			vertexCount += result.GetCellVertices( i ).Length;
			foreach( Point point in result.GetCellVertices( i ) ) {
				Assert.That( fixture.Bounds.Contains( point ), Is.True );
			}
			foreach( int neighbour in result.GetNeighbourIndices( i ) ) {
				Assert.That( result.GetNeighbourIndices( neighbour ).Contains( i ), Is.True );
			}
		}
		if( workload == "MostlyHull" ) {
			MapboxDelaunator mesh = MapboxDelaunatorFactory.Create( fixture.Sites.ToCoords(), true );
			Assert.That( mesh.Hull.Length, Is.EqualTo( count ) );
			Assert.That( vertexCount, Is.GreaterThan( mesh.Triangles.Length + mesh.Hull.Length ),
				"The all-hull fixture must exercise vertex-builder growth beyond its adjacency-sized initial capacity." );
		}
	}
}
