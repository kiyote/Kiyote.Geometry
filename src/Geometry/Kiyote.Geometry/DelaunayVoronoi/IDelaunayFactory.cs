namespace Kiyote.Geometry.DelaunayVoronoi;

public interface IDelaunayFactory {

	/// <summary>
	/// Converts the supplied <see cref="Delaunator"/> in to a <see cref="Delaunay"/>.
	/// </summary>
	/// <param name="delaunator">The <see cref="Delaunator"/> generated from a <see cref="IDelaunatorFactory"/>.</param>
	/// <returns>
	/// Returns a <see cref="Delaunay"/> representing a simpler consumption
	/// of the <see cref="Delaunator"/> data.
	/// </returns>
	public Delaunay Create( Delaunator delaunator );

}
