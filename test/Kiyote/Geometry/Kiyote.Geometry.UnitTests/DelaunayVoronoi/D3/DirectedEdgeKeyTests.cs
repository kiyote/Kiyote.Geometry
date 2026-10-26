using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.UnitTests.DelaunayVoronoi.D3;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DirectedEdgeKeyTests {
	[TestCase( -32768, -32768, 32767, 32767 )]
	[TestCase( 32767, -32768, -32768, 32767 )]
	[TestCase( -1, 0, 0, -1 )]
	[TestCase( 0, 0, 0, 0 )]
	public void ToEdge_BoundedCoordinates_RoundTripsExactly( int ax, int ay, int bx, int by ) {
		Edge edge = new Edge( ax, ay, bx, by );
		DirectedEdgeKey key = new DirectedEdgeKey( edge );
		Assert.That( key.ToEdge(), Is.EqualTo( edge ) );
		Assert.That( key, Is.EqualTo( new DirectedEdgeKey( edge ) ) );
		Assert.That( key.GetHashCode(), Is.EqualTo( new DirectedEdgeKey( edge ).GetHashCode() ) );
	}

	[Test]
	public void Equals_ReversedEndpoints_PreservesDirection() {
		DirectedEdgeKey forward = new DirectedEdgeKey( new Edge( -32768, 32767, 32767, -32768 ) );
		DirectedEdgeKey reverse = new DirectedEdgeKey( new Edge( 32767, -32768, -32768, 32767 ) );
		Assert.That( forward, Is.Not.EqualTo( reverse ) );
		Assert.That( new HashSet<DirectedEdgeKey> { forward, reverse, forward }, Has.Count.EqualTo( 2 ) );
	}
}
