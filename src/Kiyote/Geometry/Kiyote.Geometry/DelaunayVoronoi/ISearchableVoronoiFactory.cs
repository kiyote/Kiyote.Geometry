namespace Kiyote.Geometry.DelaunayVoronoi;

public interface ISearchableVoronoiFactory {

	/// <summary>Indexes a diagram within the nonempty origin-anchored rectangle defined by size.</summary>
	/// <exception cref="ArgumentException">A cell's bounding box is empty or extends outside the index bounds.</exception>
	ISearchableVoronoi Create(
		IVoronoi voronoi,
		ISize size
	);
}
