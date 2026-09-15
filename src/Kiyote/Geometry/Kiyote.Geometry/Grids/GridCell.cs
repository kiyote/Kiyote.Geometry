namespace Kiyote.Geometry.Grids;

public readonly record struct GridCell<TCell>(
	int Column,
	int Row,
	TCell? Cell
);
