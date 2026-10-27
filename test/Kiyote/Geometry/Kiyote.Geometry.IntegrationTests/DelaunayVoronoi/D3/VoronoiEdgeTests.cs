using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class VoronoiEdgeTests {
	[TestCase( -32768, 32767 )]
	[TestCase( -10, 10 )]
	[TestCase( 0, 100 )]
	public void Create_GlobalEdges_MatchesDistinctDirectedPolygonSides( int minimum, int maximum ) {
		Point[] sites = [
			new( minimum, minimum ), new( maximum, minimum ),
			new( minimum, maximum ), new( maximum, maximum )
		];
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( sites[0], sites[3] ), sites );
		HashSet<Edge> expected = [];
		for( int i = 0; i < diagram.Cells.Length; i++ ) {
			ReadOnlySpan<Point> vertices = diagram.GetCellVertices( i );
			for( int j = 0; j < vertices.Length; j++ ) {
				expected.Add( new Edge( vertices[j], vertices[(j + 1) % vertices.Length] ) );
			}
		}
		Assert.That( diagram.Edges, Is.EqualTo( expected.ToArray() ) );
		Assert.That( diagram.Edges.Distinct().Count(), Is.EqualTo( diagram.Edges.Count ) );
		Assert.That( diagram.Edges.Any( edge => expected.Contains( new Edge( edge.B, edge.A ) ) ), Is.True );
	}
}
