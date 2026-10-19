namespace Kiyote.Geometry.Trees;

/// <summary>
/// Receives query matches without materializing an intermediate collection.
/// Do not modify the index or indexed bounds during a visit.
/// </summary>
public interface IQueryVisitor<T> {
	void Visit( scoped in T item );
}
