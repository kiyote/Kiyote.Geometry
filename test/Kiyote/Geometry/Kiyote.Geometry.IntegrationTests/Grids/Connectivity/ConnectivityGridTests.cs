using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.IntegrationTests.Grids.Connectivity;

namespace Kiyote.Geometry.Grids.Connectivity.IntegrationTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class ConnectivityGridTests {

	private readonly TestGridCellConnectivityStrategy _connectivityStrategy;

	public ConnectivityGridTests() {
		_connectivityStrategy = new TestGridCellConnectivityStrategy();
	}

	[Test]
	public void UpdateConnectivity_ShouldUpdateConnectivity_WhenCalled() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 5, 5 );
		grid.TryAttach( block, 1, 1 );

		bool updated = grid.UpdateConnectivity( _connectivityStrategy );

		Assert.That( updated, Is.True );
	}

	[Test]
	public void UpdateConnectivity_SingleBlock_CornersHaveCorrectConnectivity() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 5, 5 );
		grid.TryAttach( block, 1, 1 );

		bool updated = grid.UpdateConnectivity( _connectivityStrategy );


		using( Assert.EnterMultipleScope() ) {
			Assert.That( updated, Is.True );
			Assert.That( grid[1, 1], Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South ) );
			Assert.That( grid[5, 1], Is.EqualTo( Direction.West | Direction.SouthWest | Direction.South ) );
			Assert.That( grid[5, 5], Is.EqualTo( Direction.West | Direction.NorthWest | Direction.North ) );
			Assert.That( grid[1, 5], Is.EqualTo( Direction.East | Direction.NorthEast | Direction.North ) );
		}
	}

	[Test]
	public void UpdateConnectivity_SingleBlock_CentreHasAllDirections() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 5, 5 );
		grid.TryAttach( block, 1, 1 );

		grid.UpdateConnectivity( _connectivityStrategy );

		Assert.That(
			grid[3, 3],
			Is.EqualTo(
				Direction.North | Direction.NorthEast | Direction.East | Direction.SouthEast
				| Direction.South | Direction.SouthWest | Direction.West | Direction.NorthWest
			)
		);
	}

	[Test]
	public void UpdateConnectivity_NonSolidNeighbour_IsNotConnected() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 3, 3 );
		block[1, 0] = new TestGridCell( false );
		grid.TryAttach( block, 0, 0 );

		grid.UpdateConnectivity( _connectivityStrategy );

		Assert.That(
			grid[1, 1],
			Is.EqualTo(
				Direction.NorthEast | Direction.East | Direction.SouthEast
				| Direction.South | Direction.SouthWest | Direction.West | Direction.NorthWest
			)
		);
	}

	[Test]
	public void UpdateConnectivity_UnconnectedNeighbourOutsideBounds_ReturnsFalse() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 3, 3 );
		grid.TryAttach( block, 0, 0 );

		grid.UpdateConnectivity( _connectivityStrategy );

		Assert.That(
			grid[0, 0],
			Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South )
		);
	}

	[Test]
	public void UpdateConnectivity_WithBounds_OnlyUpdatesRequestedRegion() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 5, 5 );
		grid.TryAttach( block, 0, 0 );

		bool updated = grid.UpdateConnectivity( _connectivityStrategy, 0, 0, 1, 1 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( updated, Is.True );
			Assert.That( grid[0, 0], Is.EqualTo( Direction.East | Direction.SouthEast | Direction.South ) );
			Assert.That( grid[1, 1], Is.EqualTo( Direction.None ) );
		}
	}

	[Test]
	public void UpdateConnectivity_TwoAdjacentBlocks_ConnectsAcrossSources() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> left = CreateBlock( 3, 3 );
		IMutableGrid<TestGridCell> right = CreateBlock( 3, 3 );
		grid.TryAttach( left, 0, 0 );
		grid.TryAttach( right, 3, 0 );

		bool updated = grid.UpdateConnectivity( _connectivityStrategy );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( updated, Is.True );
			Assert.That(
				grid[2, 1],
				Is.EqualTo(
					Direction.North | Direction.NorthEast | Direction.East | Direction.SouthEast
					| Direction.South | Direction.SouthWest | Direction.West | Direction.NorthWest
				)
			);
			Assert.That(
				grid[3, 1],
				Is.EqualTo(
					Direction.North | Direction.NorthWest | Direction.West | Direction.SouthWest
					| Direction.South | Direction.SouthEast | Direction.East | Direction.NorthEast
				)
			);
		}
	}

	[Test]
	public void UpdateConnectivity_ToggledEdgeCell_RegionUpdateReflectsChange() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> left = CreateBlock( 3, 3 );
		IMutableGrid<TestGridCell> right = CreateBlock( 3, 3 );
		grid.TryAttach( left, 0, 0 );
		grid.TryAttach( right, 3, 0 );
		grid.UpdateConnectivity( _connectivityStrategy );

		// Toggle off the touching edge cell of the right block, at outer (3, 1).
		right[0, 1] = new TestGridCell( false );

		bool updated = grid.UpdateConnectivity( _connectivityStrategy, 2, 0, 5, 3 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( updated, Is.True );
			Assert.That( grid[3, 1], Is.EqualTo( Direction.None ) );
			Assert.That(
				grid[2, 1],
				Is.EqualTo(
					Direction.North | Direction.NorthEast | Direction.SouthEast
					| Direction.South | Direction.SouthWest | Direction.West | Direction.NorthWest
				)
			);
			Assert.That(
				grid[4, 1],
				Is.EqualTo(
					Direction.North | Direction.NorthEast | Direction.East
					| Direction.SouthEast | Direction.South | Direction.SouthWest | Direction.NorthWest
				)
			);
		}
	}

	[Test]
	public void IsConnectedTo_NonAdjacentCells_ReturnsFalse() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 5, 5 );
		grid.TryAttach( block, 0, 0 );

		bool isConnected = grid.IsConnectedTo( _connectivityStrategy, 1, 1, 4, 4 );

		Assert.That( isConnected, Is.False );
	}

	[Test]
	public void IsConnectedTo_SameCell_ReturnsFalse() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 3, 3 );
		grid.TryAttach( block, 0, 0 );

		bool isConnected = grid.IsConnectedTo( _connectivityStrategy, 1, 1, 1, 1 );

		Assert.That( isConnected, Is.False );
	}

	[Test]
	public void IsConnectedTo_DestinationOutsideAnyAttachedGrid_ReturnsFalse() {
		IConnectivityGrid<TestGridCell> grid = new ConnectivityGrid<TestGridCell>();
		IMutableGrid<TestGridCell> block = CreateBlock( 3, 3 );
		grid.TryAttach( block, 0, 0 );

		bool isConnected = grid.IsConnectedTo( _connectivityStrategy, 2, 2, 3, 3 );

		Assert.That( isConnected, Is.False );
	}

	private static IMutableGrid<TestGridCell> CreateBlock(
		int width,
		int height
	) {
		IMutableGrid<TestGridCell> block = new ArrayGrid<TestGridCell>( width, height );
		for( int r = 0; r < height; r++ ) {
			for( int c = 0; c < width; c++ ) {
				block[c, r] = new TestGridCell( true );
			}
		}

		return block;
	}
}
