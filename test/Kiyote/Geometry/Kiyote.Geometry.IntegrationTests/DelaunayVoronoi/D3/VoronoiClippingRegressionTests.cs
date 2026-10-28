using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.IntegrationTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class VoronoiClippingRegressionTests {
	// Exact ordered output captured from the list-based clipper at 44fd182.
	[TestCase( "Grid", "8DFA431DF05282B0C34A641A9335A44083FDE64C9BB3BDA3CD736085E514CCD1" )]
	[TestCase( "Jittered", "297B8FA00E00ABA2FA2861BB9D7BA83334270F3CC321471B551765E4DEA73E2F" )]
	[TestCase( "Minimum", "D0E18CF718D4C30D849AAF634EC79FFF17214114CBDAD8202899EA342C92BEB9" )]
	[TestCase( "Maximum", "82CF6104686EF0E92F247B7BB8C39F2223945BE7B60A4CDE988AC7DC010C235A" )]
	[TestCase( "Thin", "DB34F3404D4D348A6CFA2E182780B154B41E8102FFFAA1AF75F7FEC0C298A2B3" )]
	[TestCase( "Triangle", "920B1560E4C66501CFFD56C3085836B64D2264CBEF845E83623BB965C098F88A" )]
	[TestCase( "ClosedLimit", "A8A191CE2314FFAFE7FD12B0CBF6B16542F9721CC3006BDECF27C9619495A86C" )]
	[TestCase( "OpenLimit", "EECE9F3876EE5EF3D6DE57779A436F70E236B32C8FDAD1B20A3BACF60BA93A9E" )]
	public void Create_ReferenceClippingFixtures_PreservesExactOutput( string fixture, string expected ) {
		(Rect bounds, Point[] sites) = Fixture( fixture );
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( bounds, sites );
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter( stream );
		writer.Write( diagram.Cells.Length );
		for( int i = 0; i < diagram.Cells.Length; i++ ) {
			ref readonly Cell cell = ref diagram.Cells[i];
			WritePoint( writer, diagram.Points[i] );
			writer.Write( cell.IsOpen );
			writer.Write( cell.BoundingBox.X1 );
			writer.Write( cell.BoundingBox.Y1 );
			writer.Write( cell.BoundingBox.X2 );
			writer.Write( cell.BoundingBox.Y2 );
			ReadOnlySpan<Point> vertices = diagram.GetCellVertices( i );
			writer.Write( vertices.Length );
			foreach( Point point in vertices ) {
				WritePoint( writer, point );
			}
			ReadOnlySpan<int> neighbours = diagram.GetNeighbourIndices( i );
			writer.Write( neighbours.Length );
			foreach( int neighbour in neighbours ) {
				writer.Write( neighbour );
			}
		}
		writer.Write( diagram.Edges.Count );
		foreach( Edge edge in diagram.Edges ) {
			WritePoint( writer, edge.A );
			WritePoint( writer, edge.B );
		}
		string actual = Convert.ToHexString( SHA256.HashData( stream.GetBuffer().AsSpan( 0, (int)stream.Length ) ) );
		Assert.That( actual, Is.EqualTo( expected ), fixture );
	}

	private static void WritePoint( BinaryWriter writer, Point point ) {
		writer.Write( point.X );
		writer.Write( point.Y );
	}

	private static (Rect Bounds, Point[] Sites) Fixture( string fixture ) {
		if( fixture == "Triangle" ) {
			return (new Rect( -20, -20, 41, 41 ), [new( 0, 0 ), new( 9, 2 ), new( 2, 9 )]);
		}
		if( fixture is "ClosedLimit" or "OpenLimit" ) {
			bool open = fixture == "OpenLimit";
			int count = IVoronoiFactory.MaximumCellVertices + (open ? 1 : 0);
			Point[] ring = new Point[count + 1];
			for( int i = 0; i < count; i++ ) {
				double angle = open ? -Math.PI / 2 + (Math.PI * i / (count - 1)) : 2 * Math.PI * i / count;
				ring[i + 1] = new Point(
					(int)Math.Round( 10000 * Math.Cos( angle ) ),
					(int)Math.Round( 10000 * Math.Sin( angle ) )
				);
			}
			return (new Rect( -20000, -20000, 40001, 40001 ), ring);
		}
		Rect bounds = fixture switch {
			"Grid" or "Jittered" => new Rect( -600, -500, 1201, 1001 ),
			"Minimum" => new Rect( -32768, -32768, 1201, 1001 ),
			"Maximum" => new Rect( 31567, 31767, 1201, 1001 ),
			"Thin" => new Rect( -25000, -300, 50001, 601 ),
			_ => throw new ArgumentException( "Unknown fixture.", nameof( fixture ) )
		};
		Random random = new Random( 5291 );
		Point[] sites = new Point[36];
		int stepX = bounds.Width / 7;
		int stepY = bounds.Height / 7;
		for( int i = 0; i < sites.Length; i++ ) {
			int jitterX = fixture == "Grid" ? 0 : random.Next( -stepX / 4, stepX / 4 + 1 );
			int jitterY = fixture == "Grid" ? 0 : random.Next( -stepY / 4, stepY / 4 + 1 );
			sites[i] = new Point( bounds.X1 + ((i % 6 + 1) * stepX) + jitterX,
				bounds.Y1 + ((i / 6 + 1) * stepY) + jitterY );
		}
		sites[0] = new Point( bounds.X1, bounds.Y1 );
		sites[5] = new Point( bounds.X2, bounds.Y1 );
		sites[30] = new Point( bounds.X1, bounds.Y2 );
		sites[35] = new Point( bounds.X2, bounds.Y2 );
		return (bounds, sites);
	}
}
