namespace Kiyote.Geometry.Grids;

public interface IGridAnalyzer {

	bool IsSealed<TCell, TPassability>(
		IGrid<TCell> grid,
		int startColumn,
		int startRow,
		TPassability isPassable
	)
		where TPassability : ICellStrategy<TCell, bool>;

}
