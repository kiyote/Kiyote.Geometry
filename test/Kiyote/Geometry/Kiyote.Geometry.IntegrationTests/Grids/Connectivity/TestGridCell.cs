namespace Kiyote.Geometry.IntegrationTests.Grids.Connectivity;

public sealed class TestGridCell {

	public TestGridCell(
		bool isSolid
	) {
		IsSolid = isSolid;
	}

	public bool IsSolid { get; set; }
}
