namespace Kiyote.Geometry {

	public interface IEdge: IEquatable<IEdge> {
		Point A { get; }

		Point B { get; }

		bool Equals( Point a, Point b );

		bool TryFindIntersection( IEdge other, out Point intersection );

		bool TryFindIntersection( Point a, Point b, out Point intersection );
	}
}
