using System.Diagnostics.CodeAnalysis;

namespace Kiyote.Geometry.Grids.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
internal sealed class CompositeGridTests {

	private IGrid<string> _grid;

	[SetUp]
	public void SetUp() {
		_grid = new CompositeGrid<string>();
	}

	[Test]
	public void Bounds_NothingAttached_ReturnsEmpty() {
		Assert.That( _grid.Column, Is.EqualTo( 0 ) );
		Assert.That( _grid.Row, Is.EqualTo( 0 ) );
		Assert.That( _grid.Width, Is.EqualTo( 0 ) );
		Assert.That( _grid.Height, Is.EqualTo( 0 ) );
	}

	[Test]
	public void Indexer_NothingAttached_ReturnsDefault() {
		Assert.That( _grid[0, 0], Is.Null );
	}

	[Test]
	public void GetGrid_NothingAttached_ReturnsNull() {
		Assert.That( _grid.GetGrid( 0, 0 ), Is.Null );
	}

	[Test]
	public void TryAttach_EmptyGrid_ReturnsTrue() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );

		Assert.That( _grid.TryAttach( grid, 0, 0 ), Is.True );
	}

	[Test]
	public void TryAttach_Self_ReturnsFalse() {
		Assert.That( _grid.TryAttach( _grid, 0, 0 ), Is.False );
	}

	[Test]
	public void TryAttach_SameGridTwice_ReturnsFalse() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		_ = _grid.TryAttach( grid, 0, 0 );

		Assert.That( _grid.TryAttach( grid, 100, 100 ), Is.False );
	}

	[Test]
	public void TryAttach_OverlappingGrid_ReturnsFalse() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 0, 0 );

		Assert.That( _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 3, 3 ), Is.False );
	}

	[Test]
	public void TryAttach_OverlappingGrid_BoundsUnchanged() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 0, 0 );
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 3, 3 );

		Assert.That( _grid.Width, Is.EqualTo( 4 ) );
		Assert.That( _grid.Height, Is.EqualTo( 4 ) );
	}

	[Test]
	public void TryAttach_AdjacentGrid_ReturnsTrue() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 0, 0 );

		Assert.That( _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 4, 0 ), Is.True );
	}

	[Test]
	public void TryAttach_OverlapCausedByGridOrigin_ReturnsFalse() {
		// Occupies columns 5..9 because the grid's own bounds start at column 5.
		_ = _grid.TryAttach( new TestGrid<string>( 5, 0, 5, 4 ), 0, 0 );

		// Attached at column 3, but its own origin of 2 pushes it to columns 5..9.
		Assert.That( _grid.TryAttach( new TestGrid<string>( 2, 0, 5, 4 ), 3, 0 ), Is.False );
	}

	[Test]
	public void TryAttach_NullGrid_ThrowsException() {
		Assert.Throws<ArgumentNullException>( () => _grid.TryAttach( null!, 0, 0 ) );
	}

	[Test]
	public void TryDetach_AttachedGrid_ReturnsTrue() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		_ = _grid.TryAttach( grid, 0, 0 );

		Assert.That( _grid.TryDetach( grid ), Is.True );
	}

	[Test]
	public void TryDetach_UnattachedGrid_ReturnsFalse() {
		Assert.That( _grid.TryDetach( new TestGrid<string>( 0, 0, 4, 4 ) ), Is.False );
	}

	[Test]
	public void TryDetach_OnlyGrid_BoundsReturnToEmpty() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		_ = _grid.TryAttach( grid, 10, 10 );

		_ = _grid.TryDetach( grid );

		Assert.That( _grid.Column, Is.EqualTo( 0 ) );
		Assert.That( _grid.Row, Is.EqualTo( 0 ) );
		Assert.That( _grid.Width, Is.EqualTo( 0 ) );
		Assert.That( _grid.Height, Is.EqualTo( 0 ) );
	}

	[Test]
	public void TryDetach_DetachedGrid_SpaceCanBeReused() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		_ = _grid.TryAttach( grid, 0, 0 );
		_ = _grid.TryDetach( grid );

		Assert.That( _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 0, 0 ), Is.True );
	}

	[Test]
	public void TryDetach_NullGrid_ThrowsException() {
		Assert.Throws<ArgumentNullException>( () => _grid.TryDetach( null! ) );
	}

	[Test]
	public void Bounds_SingleGridAttachedAtOffset_ReflectsOffset() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 6 ), 10, 20 );

		Assert.That( _grid.Column, Is.EqualTo( 10 ) );
		Assert.That( _grid.Row, Is.EqualTo( 20 ) );
		Assert.That( _grid.Width, Is.EqualTo( 4 ) );
		Assert.That( _grid.Height, Is.EqualTo( 6 ) );
	}

	[Test]
	public void Bounds_GridWithNonZeroOrigin_IncludesOrigin() {
		_ = _grid.TryAttach( new TestGrid<string>( 2, 3, 4, 4 ), 10, 10 );

		Assert.That( _grid.Column, Is.EqualTo( 12 ) );
		Assert.That( _grid.Row, Is.EqualTo( 13 ) );
		Assert.That( _grid.Width, Is.EqualTo( 4 ) );
		Assert.That( _grid.Height, Is.EqualTo( 4 ) );
	}

	[Test]
	public void Bounds_MultipleGrids_ReturnsUnion() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 0, 0 );
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 10, 20 );

		Assert.That( _grid.Column, Is.EqualTo( 0 ) );
		Assert.That( _grid.Row, Is.EqualTo( 0 ) );
		Assert.That( _grid.Width, Is.EqualTo( 14 ) );
		Assert.That( _grid.Height, Is.EqualTo( 24 ) );
	}

	[Test]
	public void Bounds_NegativeCoordinates_ReturnsUnion() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), -10, -10 );
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 0, 0 );

		Assert.That( _grid.Column, Is.EqualTo( -10 ) );
		Assert.That( _grid.Row, Is.EqualTo( -10 ) );
		Assert.That( _grid.Width, Is.EqualTo( 14 ) );
		Assert.That( _grid.Height, Is.EqualTo( 14 ) );
	}

	[Test]
	public void Bounds_ZeroSizedGridAttached_RemainsEmpty() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 0, 0 ), 5, 5 );

		Assert.That( _grid.Width, Is.EqualTo( 0 ) );
		Assert.That( _grid.Height, Is.EqualTo( 0 ) );
	}

	[Test]
	public void Indexer_CoordinateWithinAttachedGrid_ReturnsValue() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		grid.Set( 1, 2, "value" );
		_ = _grid.TryAttach( grid, 10, 20 );

		Assert.That( _grid[11, 22], Is.EqualTo( "value" ) );
	}

	[Test]
	public void Indexer_CoordinateOutsideAttachedGrid_ReturnsDefault() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		grid.Set( 1, 2, "value" );
		_ = _grid.TryAttach( grid, 10, 20 );

		Assert.That( _grid[0, 0], Is.Null );
	}

	[Test]
	public void Indexer_NegativeCoordinate_ReturnsValue() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		grid.Set( 0, 0, "value" );
		_ = _grid.TryAttach( grid, -10, -10 );

		Assert.That( _grid[-10, -10], Is.EqualTo( "value" ) );
	}

	[Test]
	public void Indexer_CorrectGridSelectedFromMany_ReturnsValue() {
		TestGrid<string> first = new TestGrid<string>( 0, 0, 4, 4 );
		first.Set( 0, 0, "first" );
		TestGrid<string> second = new TestGrid<string>( 0, 0, 4, 4 );
		second.Set( 0, 0, "second" );
		_ = _grid.TryAttach( first, 0, 0 );
		_ = _grid.TryAttach( second, 4, 0 );

		Assert.That( _grid[0, 0], Is.EqualTo( "first" ) );
		Assert.That( _grid[4, 0], Is.EqualTo( "second" ) );
	}

	[Test]
	public void Indexer_GridWithNonZeroOrigin_TranslatesCoordinate() {
		TestGrid<string> grid = new TestGrid<string>( 2, 3, 4, 4 );
		grid.Set( 2, 3, "value" );
		_ = _grid.TryAttach( grid, 10, 10 );

		Assert.That( _grid[12, 13], Is.EqualTo( "value" ) );
	}

	[Test]
	public void Indexer_DetachedGrid_ReturnsDefault() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		grid.Set( 0, 0, "value" );
		_ = _grid.TryAttach( grid, 0, 0 );
		_ = _grid.TryDetach( grid );

		Assert.That( _grid[0, 0], Is.Null );
	}

	[Test]
	public void GetGrid_CoordinateWithinAttachedGrid_ReturnsGrid() {
		TestGrid<string> grid = new TestGrid<string>( 0, 0, 4, 4 );
		_ = _grid.TryAttach( grid, 10, 20 );

		Assert.That( _grid.GetGrid( 11, 21 ), Is.SameAs( grid ) );
	}

	[Test]
	public void GetGrid_CoordinateOutsideAttachedGrid_ReturnsNull() {
		_ = _grid.TryAttach( new TestGrid<string>( 0, 0, 4, 4 ), 10, 20 );

		Assert.That( _grid.GetGrid( 0, 0 ), Is.Null );
	}

	[Test]
	public void GetGrid_NestedGridsNonRecursive_ReturnsDirectChild() {
		TestGrid<string> leaf = new TestGrid<string>( 0, 0, 4, 4 );
		IGrid<string> inner = new CompositeGrid<string>();
		_ = inner.TryAttach( leaf, 0, 0 );
		_ = _grid.TryAttach( inner, 10, 10 );

		Assert.That( _grid.GetGrid( 10, 10, false ), Is.SameAs( inner ) );
	}

	[Test]
	public void GetGrid_NestedGridsRecursive_ReturnsChildMost() {
		TestGrid<string> leaf = new TestGrid<string>( 0, 0, 4, 4 );
		IGrid<string> inner = new CompositeGrid<string>();
		_ = inner.TryAttach( leaf, 0, 0 );
		_ = _grid.TryAttach( inner, 10, 10 );

		Assert.That( _grid.GetGrid( 10, 10, true ), Is.SameAs( leaf ) );
	}

	[Test]
	public void GetGrid_LeafGridRecursive_ReturnsLeaf() {
		TestGrid<string> leaf = new TestGrid<string>( 0, 0, 4, 4 );
		_ = _grid.TryAttach( leaf, 0, 0 );

		Assert.That( _grid.GetGrid( 0, 0, true ), Is.SameAs( leaf ) );
	}

	[Test]
	public void Indexer_NestedGrids_ReturnsValue() {
		TestGrid<string> leaf = new TestGrid<string>( 0, 0, 4, 4 );
		leaf.Set( 1, 1, "value" );
		IGrid<string> inner = new CompositeGrid<string>();
		_ = inner.TryAttach( leaf, 5, 5 );
		_ = _grid.TryAttach( inner, 10, 10 );

		Assert.That( _grid[16, 16], Is.EqualTo( "value" ) );
	}

	[Test]
	public void VisitGrids_NothingAttached_VisitorNotCalled() {
		List<IGrid<string>> visited = [];

		_grid.VisitGrids( 0, 0, ( grid, column, row ) => visited.Add( grid ) );

		Assert.That( visited, Is.Empty );
	}

	[Test]
	public void VisitGrids_NestedGrids_VisitsOutermostFirst() {
		TestGrid<string> leaf = new TestGrid<string>( 0, 0, 4, 4 );
		IGrid<string> inner = new CompositeGrid<string>();
		_ = inner.TryAttach( leaf, 5, 5 );
		_ = _grid.TryAttach( inner, 10, 10 );
		List<IGrid<string>> visited = [];

		_grid.VisitGrids( 16, 16, ( grid, column, row ) => visited.Add( grid ) );

		Assert.That( visited, Is.EqualTo( new List<IGrid<string>> { inner, leaf } ) );
	}

	[Test]
	public void VisitGrids_NestedGrids_SuppliesLocalCoordinates() {
		TestGrid<string> leaf = new TestGrid<string>( 0, 0, 4, 4 );
		IGrid<string> inner = new CompositeGrid<string>();
		_ = inner.TryAttach( leaf, 5, 5 );
		_ = _grid.TryAttach( inner, 10, 10 );
		List<(int Column, int Row)> visited = [];

		_grid.VisitGrids( 16, 16, ( grid, column, row ) => visited.Add( (column, row) ) );

		Assert.That( visited, Is.EqualTo( new List<(int, int)> { (6, 6), (1, 1) } ) );
	}

	[Test]
	public void VisitGrids_NullVisitor_ThrowsException() {
		Assert.Throws<ArgumentNullException>( () => _grid.VisitGrids( 0, 0, null! ) );
	}

}
