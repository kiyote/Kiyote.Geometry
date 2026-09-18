namespace Kiyote.Geometry.Grids;

public interface IGridSampler<TCoordinate, TValue> {

	TValue Sample(
		IGrid<TValue> grid,
		TCoordinate column,
		TCoordinate row
	);

	// Generic overload: lets struct-based IGrid<TValue> implementations be sampled
	// without boxing. Default body falls back to the interface-typed overload (and
	// will box) so this is source/binary compatible for any existing implementers
	// that don't override it; performance-sensitive samplers should override this
	// directly instead.
	TValue Sample<TGrid>(
		TGrid grid,
		TCoordinate column,
		TCoordinate row
	) where TGrid : IGrid<TValue> {
		return Sample( (IGrid<TValue>)grid, column, row );
	}

}
