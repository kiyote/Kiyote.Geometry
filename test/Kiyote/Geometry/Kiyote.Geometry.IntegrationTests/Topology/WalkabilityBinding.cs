namespace Kiyote.Geometry.Topology.IntegrationTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage( "Performance", "CA1815:Override equals and operator equals on value types", Justification = "Operators are not meaningful for this type." )]
public struct WalkabilityBinding : IGridLayerBinding<TestCell, bool> {
	public readonly void Commit( ref TestCell cell, bool value ) {
		cell.IsWalkable = value;
	}

	public readonly bool Extract( in TestCell cell ) {
		return cell.IsWalkable;
	}
}
