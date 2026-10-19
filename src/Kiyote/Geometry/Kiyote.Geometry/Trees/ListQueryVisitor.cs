namespace Kiyote.Geometry.Trees;

internal struct ListQueryVisitor<T> : IQueryVisitor<T> {
	private List<T>? _items;

	internal ListQueryVisitor( int capacity ) {
		_items = new List<T>( capacity );
	}

	internal readonly IReadOnlyList<T> Results => _items is null ? Array.Empty<T>() : _items;

	public void Visit( scoped in T item ) {
		( _items ??= [] ).Add( item );
	}
}
