using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.UnitTests.Grids.Connectivity;

namespace Kiyote.Geometry.Grids.Connectivity.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class ConnectivityGridTopologyTests {

	private IConnectivityGrid<bool> _grid;
	private readonly ConstantConnectivityStrategy _strategy;

	public ConnectivityGridTopologyTests() {
		_strategy = new ConstantConnectivityStrategy();
	}

	[SetUp]
	public void SetUp() {
		_grid = new ConnectivityGrid<bool>();
	}

	[Test]
	public void Version_GridAttached_Changes() {
		int before = _grid.Version;

		_ = _grid.TryAttach( new FlatArrayGrid<bool>( 1, 1 ), 0, 0 );

		Assert.That( _grid.Version, Is.Not.EqualTo( before ) );
	}

	[Test]
	public void Version_GridDetached_Changes() {
		FlatArrayGrid<bool> child = new( 1, 1 );
		_ = _grid.TryAttach( child, 0, 0 );
		int before = _grid.Version;

		_ = _grid.TryDetach( child );

		Assert.That( _grid.Version, Is.Not.EqualTo( before ) );
	}

	[Test]
	public void Version_ConnectivityChanged_Changes() {
		_ = _grid.TryAttach( new FlatArrayGrid<bool>( 2, 1 ), 0, 0 );
		int before = _grid.Version;

		bool updated = _grid.UpdateConnectivity( _strategy );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( updated, Is.True );
			Assert.That( _grid.Version, Is.Not.EqualTo( before ) );
		}
	}

	[Test]
	public void Version_ConnectivityUnchanged_DoesNotChange() {
		_ = _grid.TryAttach( new FlatArrayGrid<bool>( 2, 1 ), 0, 0 );
		_ = _grid.UpdateConnectivity( _strategy );
		int before = _grid.Version;

		bool updated = _grid.UpdateConnectivity( _strategy );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( updated, Is.False );
			Assert.That( _grid.Version, Is.EqualTo( before ) );
		}
	}

	[Test]
	public void Version_NestedCompositeChanged_Changes() {
		IGrid<int> outer = new CompositeGrid<int>();
		IGrid<int> inner = new CompositeGrid<int>();
		_ = outer.TryAttach( inner, 0, 0 );
		int before = outer.Version;

		_ = inner.TryAttach( new FlatArrayGrid<int>( 1, 1 ), 0, 0 );

		Assert.That( outer.Version, Is.Not.EqualTo( before ) );
	}

	[Test]
	public void BuildTopology_TwoAdjacentGrids_LeavesAndSeamsBuilt() {
		_ = _grid.TryAttach( new FlatArrayGrid<bool>( 1, 1 ), 0, 0 );
		_ = _grid.TryAttach( new FlatArrayGrid<bool>( 1, 1 ), 1, 0 );
		_ = _grid.UpdateConnectivity( _strategy );

		GridTopology<bool> topology = _grid.BuildTopology();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( topology.Version, Is.EqualTo( _grid.Version ) );
			Assert.That( topology.Leaves.Length, Is.EqualTo( 2 ) );
			Assert.That( topology.CellCount, Is.EqualTo( 2 ) );
			Assert.That( topology.Seams.ToArray(), Is.EquivalentTo( new[] {
				new SeamLink( 0, 1, Direction.East ),
				new SeamLink( 1, 0, Direction.West )
			} ) );
		}
	}

	[Test]
	public void BuildTopology_SingleGrid_ConnectivityCopiedWithoutSeams() {
		_ = _grid.TryAttach( new FlatArrayGrid<bool>( 2, 1 ), 0, 0 );
		_ = _grid.UpdateConnectivity( _strategy );

		GridTopology<bool> topology = _grid.BuildTopology();

		using( Assert.EnterMultipleScope() ) {
			Assert.That( topology.Seams.Length, Is.Zero );
			Assert.That( topology.GetConnectivity( 0 )[0], Is.EqualTo( _grid[0, 0] ) );
			Assert.That( topology.GetConnectivity( 0 )[1], Is.EqualTo( _grid[1, 0] ) );
			Assert.That( topology.TryGetIndex( 1, 0, out int index ), Is.True );
			Assert.That( index, Is.EqualTo( 1 ) );
		}
	}
}
