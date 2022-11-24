namespace Kiyote.Geometry.Visualizer;

public static class Program {

	public static void Main() {
		string outputFolder = Path.Combine( Path.GetTempPath(), "Kiyote.Geometry.Visualizer" );
		if (!Directory.Exists( outputFolder )) {
			Directory.CreateDirectory( outputFolder );
		}
		Bounds bounds = new Bounds( 1000, 1000 );

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

	}
}
