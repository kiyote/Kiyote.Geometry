namespace Kiyote.Geometry {

	public interface IEdge: IEquatable<IEdge> {
		IPoint A { get; }

		IPoint B { get; }

		bool Equals( IPoint a, IPoint b );

		bool TryFindIntersection( IEdge other, out IPoint intersection );

		bool TryFindIntersection( IPoint a, IPoint b, out IPoint intersection );
	}
}
