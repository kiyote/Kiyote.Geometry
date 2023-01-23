namespace Kiyote.Geometry {

	public interface IEdge: IEquatable<IEdge> {
		IPoint A { get; }
		IPoint B { get; }

		bool Equals( IPoint a, IPoint b );

		IPoint? Intersection( IEdge other );

		IPoint? Intersection( IPoint a, IPoint b );
	}
}
