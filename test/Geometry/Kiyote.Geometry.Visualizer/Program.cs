using Kiyote.Geometry.Visualizer.Randomization;
using Kiyote.Geometry.Visualizer.DelaunayVoronoi;

namespace Kiyote.Geometry.Visualizer;

public static class Program {

	public static void Main() {
		string outputFolder = Path.Combine( Path.GetTempPath(), "Kiyote.Geometry.Visualizer" );
		if (!Directory.Exists( outputFolder )) {
			_ = Directory.CreateDirectory( outputFolder );
		}
		ISize bounds = new Point( 1000, 1000 );
		
		// Edge
		var pointVisualizer = new PolygonVisualizer(
			outputFolder,
			bounds
		);
		pointVisualizer.Visualize();

		// FastPoissonDiscPointFactory
		var fastPoissonDiscPointFactory = new FastPoissonDiscPointFactoryVisualizer(
			outputFolder,
			bounds
		);
		fastPoissonDiscPointFactory.Visualize();

		// FastRandom
		var fastRandom = new FastRandomVisualizer(
			outputFolder,
			bounds
		);
		fastRandom.Visualize();

		// D3DelaunayFactory
		var d3DelaunayFactory = new D3DelaunayFactoryVisualizer(
			outputFolder,
			bounds
		);
		d3DelaunayFactory.Visualize();
		

		// D3VoronoiFactory
		var d3VoronoiFactory = new D3VoronoiFactoryVisualizer(
			outputFolder,
			bounds
		);
		d3VoronoiFactory.Visualize();
	}
}
