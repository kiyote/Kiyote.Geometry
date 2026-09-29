namespace Kiyote.Geometry.Topology;

/// <summary>
/// Identifies a single placement of an <see cref="IGridSource{TCell}"/> within an
/// <see cref="IGridAssembly{TCell}"/>.  Identifiers are only meaningful within
/// the assembly that holds them.
/// </summary>
/// <param name="Value">The raw identifier value.</param>
public readonly record struct PlacementId(
	int Value
) {
	public static readonly PlacementId None = new( 0 );

	public bool IsNone => Value == 0;
}
