namespace Kiyote.Geometry.Topology.IntegrationTests;

[TestFixture]
internal sealed class GridAssemblyTests {

	[Test]
	public void TryAttach_TwoGridsNoOverlap_Succeeds() {
		DenseGridSource<TestCell> source1 = AsciiMapSource.Create( AsciiMapSource.Map1 );
		DenseGridSource<TestCell> source2 = AsciiMapSource.Create( AsciiMapSource.Map1 );
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		PlacementId placement1 = new PlacementId( 1 );
		PlacementId placement2 = new PlacementId( 2 );
		AttachResult result1 = assembly.TryAttach( source1, placement1, 0, 0 );
		AttachResult result2 = assembly.TryAttach( source2, placement2, 50, 13 );

		using( Assert.EnterMultipleScope() ) {
			Assert.That( result1.Succeeded, Is.True );
			Assert.That( result2.Succeeded, Is.True );
			Assert.That( assembly.Placements.Count, Is.EqualTo( 2 ) );
			Assert.That( result1.Seams, Is.Empty );
			Assert.That( result2.Seams, Has.Count.EqualTo( 1 ) );

			GridSeam seam = result2.Seams[0];
			Assert.That( seam.A, Is.EqualTo( placement1 ) );
			Assert.That( seam.B, Is.EqualTo( placement2 ) );

			Assert.That( seam.Contacts, Is.EquivalentTo( [
				new GridContact( 49, 13, 50, 13, Direction.East ),
				new GridContact( 49, 13, 50, 14, Direction.SouthEast ),

				new GridContact( 49, 14, 50, 13, Direction.NorthEast ),
				new GridContact( 49, 14, 50, 14, Direction.East ),
				new GridContact( 49, 14, 50, 15, Direction.SouthEast ),

				new GridContact( 49, 15, 50, 14, Direction.NorthEast ),
				new GridContact( 49, 15, 50, 15, Direction.East ),
				new GridContact( 49, 15, 50, 16, Direction.SouthEast ),

				new GridContact( 49, 16, 50, 15, Direction.NorthEast ),
				new GridContact( 49, 16, 50, 16, Direction.East ),
				new GridContact( 49, 16, 50, 17, Direction.SouthEast ),

				new GridContact( 49, 17, 50, 16, Direction.NorthEast ),
				new GridContact( 49, 17, 50, 17, Direction.East ),
				new GridContact( 49, 17, 50, 18, Direction.SouthEast ),

				new GridContact( 49, 18, 50, 17, Direction.NorthEast ),
				new GridContact( 49, 18, 50, 18, Direction.East ),
				new GridContact( 49, 18, 50, 19, Direction.SouthEast )
			] ) );
		}
	}

	[Test]
	public void TryAddCell_AddCellToSource_NewCellAdded() {
		DenseGridSource<TestCell> source = AsciiMapSource.Create( AsciiMapSource.Map1 );
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		PlacementId placement = new PlacementId( 1 );
		_ = assembly.TryAttach( source, placement, 0, 0 );

		Assert.That( source.IsOccupied( 20, 1 ), Is.False );
		assembly.TryAddCell( placement, 20, 1, new TestCell( false, false, false ) );
		Assert.That( source.IsOccupied( 20, 1 ), Is.True );
	}

	[Test]
	public void TryAttach_AddCellToSource_NewCellAdded() {
		DenseGridSource<TestCell> source = AsciiMapSource.Create( AsciiMapSource.Map1 );
		GridAssembly<TestCell> assembly = new GridAssembly<TestCell>();
		PlacementId placement = new PlacementId( 1 );
		_ = assembly.TryAttach( source, placement, 0, 0 );
		assembly.TryAddCell( placement, 20, 1, new TestCell( false, false, false ) );

		Assert.That( source.IsOccupied( 20, 1 ), Is.True );
		assembly.TryRemoveCell( placement, 20, 1 );
		Assert.That( source.IsOccupied( 20, 1 ), Is.False );
	}
}
