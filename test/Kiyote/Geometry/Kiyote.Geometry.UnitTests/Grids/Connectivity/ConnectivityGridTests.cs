using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.UnitTests.Grids.Connectivity;

namespace Kiyote.Geometry.Grids.Connectivity.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class ConnectivityGridTests {

	private IConnectivityGrid<bool> _grid;
	private readonly ConstantConnectivityStrategy _strategy;

	public ConnectivityGridTests() {
		_strategy = new ConstantConnectivityStrategy();
	}
	

	[SetUp]
	public void SetUp() {
		_grid = new ConnectivityGrid<bool>();
	}

	[Test]
	public void TryAttach_FreshGrid_ReturnsTrue() {
		RaggedArrayGrid<bool> childGrid = new RaggedArrayGrid<bool>( 1, 1 );

		bool actual = _grid.TryAttach( childGrid, 0, 0 );

		Assert.That( actual, Is.True );
	}

	[Test]
	public void TryAttach_SameGridTwice_ReturnsFalse() {
		RaggedArrayGrid<bool> childGrid = new RaggedArrayGrid<bool>( 1, 1 );
		_ = _grid.TryAttach( childGrid, 0, 0 );

		bool actual = _grid.TryAttach( childGrid, 0, 0 );

		Assert.That( actual, Is.False );
	}

	[Test]
	public void TryAttach_OverlappingGrids_ReturnsFalse() {
		RaggedArrayGrid<bool> childGrid1 = new RaggedArrayGrid<bool>( 2, 2 );
		RaggedArrayGrid<bool> childGrid2 = new RaggedArrayGrid<bool>( 2, 2 );
		_ = _grid.TryAttach( childGrid1, 0, 0 );

		bool actual = _grid.TryAttach( childGrid2, 1, 1 );

		Assert.That( actual, Is.False );
	}

	[Test]
	public void TryAttach_GridAttached_GridDimensionsUpdate() {
		RaggedArrayGrid<bool> childGrid = new RaggedArrayGrid<bool>( 1, 2 );
		_ = _grid.TryAttach( childGrid, 1, 1 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( _grid.Column, Is.EqualTo( 1 ) );
			Assert.That( _grid.Row, Is.EqualTo( 1 ) );
			Assert.That( _grid.Width, Is.EqualTo( 1 ) );
			Assert.That( _grid.Height, Is.EqualTo( 2 ) );
		}
	}

	[Test]
	public void TryAttach_GridsAttached_GridDimensionsUpdate() {
		RaggedArrayGrid<bool> childGrid1 = new RaggedArrayGrid<bool>( 2, 2 );
		RaggedArrayGrid<bool> childGrid2 = new RaggedArrayGrid<bool>( 3, 2 );
		_ = _grid.TryAttach( childGrid1, 1, 1 );
		_ = _grid.TryAttach( childGrid2, 6, 6 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( _grid.Column, Is.EqualTo( 1 ) );
			Assert.That( _grid.Row, Is.EqualTo( 1 ) );
			Assert.That( _grid.Width, Is.EqualTo( 8 ) );
			Assert.That( _grid.Height, Is.EqualTo( 7 ) );
		}
	}

	[Test]
	public void TryDetach_GridsAttached_GridDimensionsUpdate() {
		RaggedArrayGrid<bool> childGrid1 = new RaggedArrayGrid<bool>( 2, 2 );
		RaggedArrayGrid<bool> childGrid2 = new RaggedArrayGrid<bool>( 3, 2 );
		_ = _grid.TryAttach( childGrid1, 1, 1 );
		_ = _grid.TryAttach( childGrid2, 6, 6 );

		_ = _grid.TryDetach( childGrid1 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( _grid.Column, Is.EqualTo( 6 ) );
			Assert.That( _grid.Row, Is.EqualTo( 6 ) );
			Assert.That( _grid.Width, Is.EqualTo( 3 ) );
			Assert.That( _grid.Height, Is.EqualTo( 2 ) );
		}
	}

	[Test]
	public void TryDetach_NothingAttached_ReturnsFalse() {
		RaggedArrayGrid<bool> childGrid = new RaggedArrayGrid<bool>( 1, 1 );

		bool actual = _grid.TryDetach( childGrid );

		Assert.That( actual, Is.False );
	}

	[Test]
	public void TryDetach_GridAttached_ReturnsTrue() {
		RaggedArrayGrid<bool> childGrid = new RaggedArrayGrid<bool>( 1, 1 );
		_ = _grid.TryAttach( childGrid, 0, 0 );

		bool actual = _grid.TryDetach( childGrid );

		Assert.That( actual, Is.True );
	}


	[Test]
	public void TryDetach_DetachDetachedGrid_ReturnsFalse() {
		RaggedArrayGrid<bool> childGrid = new RaggedArrayGrid<bool>( 1, 1 );
		_ = _grid.TryAttach( childGrid, 0, 0 );
		_ = _grid.TryDetach( childGrid );

		bool actual = _grid.TryDetach( childGrid );

		Assert.That( actual, Is.False );
	}
	
	[Test]
	public void IsConnectedTo_AdjacentRealCells_ReturnsTrue () {
		RaggedArrayGrid<bool> childGrid = new RaggedArrayGrid<bool>( 2, 1 );
		_ = _grid.TryAttach( childGrid, 0, 0 );

		Assert.That( _grid.IsConnectedTo( _strategy, 0, 0, 1, 0 ), Is.True );
		Assert.That( _grid.IsConnectedTo( _strategy, 1, 0, 0, 0 ), Is.True );
	}
}
