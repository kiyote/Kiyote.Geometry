using Kiyote.Geometry.Visualizer.Randomization;
using Kiyote.Geometry.Visualizer.DelaunayVoronoi;

namespace Kiyote.Geometry.Visualizer;

public static class Program {

	public static void Main() {
		string outputFolder = Path.Combine( Path.GetTempPath(), "Kiyote.Geometry.Visualizer" );
		if (!Directory.Exists( outputFolder )) {
			Directory.CreateDirectory( outputFolder );
		}
		Bounds bounds = new Bounds( 1000, 1000 );

		// Edge
		var pointVisualizer = new PolygonVisualizer(
			outputFolder,
			bounds
		);
		pointVisualizer.Visualize();

		// Edge
		var edgeVisualizer = new EdgeVisualizer(
			outputFolder,
			bounds
		);
		edgeVisualizer.Visualize();

		// FastPoissonDiscPointFactory
		var fastPoissonDiscPointFactory = new FastPoissonDiscPointFactoryVisualizer(
			outputFolder,
			bounds
		);
		fastPoissonDiscPointFactory.Visualize();

		// PoissonDiscPointFactory
		var poissonDiscPointFactory = new PoissonDiscPointFactoryVisualizer(
			outputFolder,
			bounds
		);
		poissonDiscPointFactory.Visualize();

		// FastRandom
		var fastRandom = new FastRandomVisualizer(
			outputFolder,
			bounds
		);
		fastRandom.Visualize();

		// DelaunatorFactory
		var delaunatorFactory = new DelaunatorFactoryVisualizer(
			outputFolder,
			bounds
		);
		delaunatorFactory.Visualize();

		// DelaunayFactory
		var delaunayFactory = new DelaunayFactoryVisualizer(
			outputFolder,
			bounds
		);
		delaunayFactory.Visualize();

		// VoronoiFactory
		var voronoiFactory = new VoronoiFactoryVisualizer(
			outputFolder,
			bounds
		);
		voronoiFactory.Visualize();
	}
}
