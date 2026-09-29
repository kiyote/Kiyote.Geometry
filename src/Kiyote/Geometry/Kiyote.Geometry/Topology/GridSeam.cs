namespace Kiyote.Geometry.Topology;

/// <summary>
/// The set of contacts between two placements, captured when the later of the
/// two was attached.  <see cref="A"/> and <see cref="B"/> are ordered by
/// <see cref="PlacementId.Value"/>, so <see cref="A"/> is always the placement
/// with the lower identifier, regardless of which was attached first.
/// </summary>
/// <param name="A">The placement with the lower identifier.  Contacts are expressed from this placement's side.</param>
/// <param name="B">The placement with the higher identifier.</param>
/// <param name="Contacts">
/// Every neighbouring cell pair between the two placements.  The list is a
/// snapshot; later attaches, detaches or cell edits do not change it.  The
/// order of the contacts is not guaranteed and must not be relied upon.
/// </param>
public readonly record struct GridSeam(
	PlacementId A,
	PlacementId B,
	IReadOnlyList<GridContact> Contacts
);
