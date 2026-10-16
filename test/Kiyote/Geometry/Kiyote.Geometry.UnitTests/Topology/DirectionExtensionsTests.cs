using System.Diagnostics.CodeAnalysis;
using Kiyote.Buffers.ChunkedGrids;
using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.UnitTests.Topology;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class DirectionExtensionsTests {

	private static readonly Direction[] AllDirections = [
		Direction.North,
		Direction.NorthEast,
		Direction.East,
		Direction.SouthEast,
		Direction.South,
		Direction.SouthWest,
		Direction.West,
		Direction.NorthWest
	];

	[Test]
	public void ToOffset_North_IsNegativeY() {
		Assert.That( Direction.North.ToOffset(), Is.EqualTo( new Point( 0, -1 ) ) );
	}

	[Test]
	public void ToOffset_None_IsZero() {
		Assert.That( Direction.None.ToOffset(), Is.EqualTo( new Point( 0, 0 ) ) );
	}

	[TestCaseSource( nameof( AllDirections ) )]
	public void FromOffset_ToOffset_RoundTrips(
		Direction direction
	) {
		Assert.That( DirectionExtensions.FromOffset( direction.ToOffset() ), Is.EqualTo( direction ) );
	}

	[TestCaseSource( nameof( AllDirections ) )]
	public void Opposite_AnyDirection_OffsetIsNegated(
		Direction direction
	) {
		Assert.That( direction.Opposite().ToOffset(), Is.EqualTo( -direction.ToOffset() ) );
	}

	[TestCaseSource( nameof( AllDirections ) )]
	public void ToNeighbourIndex_AnyDirection_SelectsLayoutNeighbour(
		Direction direction
	) {
		ChunkedGridLayout layout = new ChunkedGridLayout( 32 );
		int centre = layout.GetOrCreateChunk( 0, 0 );
		Point offset = direction.ToOffset();
		int neighbour = layout.GetOrCreateChunk( offset.X, offset.Y );

		Assert.That( layout.GetNeighbours( centre )[direction.ToNeighbourIndex()], Is.EqualTo( neighbour ) );
	}

	[Test]
	public void ToNeighbourIndex_None_IsCentre() {
		Assert.That( Direction.None.ToNeighbourIndex(), Is.EqualTo( 4 ) );
	}

	[Test]
	public void FromOffset_NotNeighbour_ReturnsNone() {
		Assert.That( DirectionExtensions.FromOffset( new Point( 2, 0 ) ), Is.EqualTo( Direction.None ) );
		Assert.That( DirectionExtensions.FromOffset( new Point( 0, 0 ) ), Is.EqualTo( Direction.None ) );
	}

	[TestCaseSource( nameof( AllDirections ) )]
	public void TryGetOrthogonals_AnyDirection_SumsToDiagonal(
		Direction direction
	) {
		bool result = direction.TryGetOrthogonals( out Direction first, out Direction second );

		Assert.That( result, Is.EqualTo( direction.IsDiagonal() ) );
		if( result ) {
			Assert.That( first.ToOffset() + second.ToOffset(), Is.EqualTo( direction.ToOffset() ) );
			Assert.That( first is Direction.North or Direction.South, Is.True );
		} else {
			Assert.That( first, Is.EqualTo( Direction.None ) );
			Assert.That( second, Is.EqualTo( Direction.None ) );
		}
	}
}
