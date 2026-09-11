namespace Kiyote.Geometry.Grids;

public interface IMutableGrid<T> : IImmutableGrid<T> {

	/// <summary>
	/// Redeclares the indexer so that it exposes a setter.  C# has no way to add an
	/// accessor to an inherited member, so the member is hidden with a new one that
	/// declares both accessors.
	/// </summary>
	new T? this[int column, int row] { get; set; }
}
