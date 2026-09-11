
namespace Kiyote.Geometry.DelaunayVoronoi.Profiler;

public sealed class MapboxDelaunatorFactoryProfiler {

	public const int Iterations = 100;
	public const int Separation = 5;
	private readonly double[] _coords;

	public MapboxDelaunatorFactoryProfiler() {
		ISize size = new Point( 1000, 1000 );
		int cellWidth = size.Width / 20;
		int cellHeight = size.Height / 20;

		List<Point> points = [];
		for( int c = cellWidth / 2; c < size.Width; c += cellWidth ) {
			for( int r = cellHeight / 2; r < size.Height; r += cellHeight ) {
				points.Add( new Point( c, r ) );
			}
		}
		_coords = points.ToCoords();
	}

	public void Profile() {
		for (int i = 0; i < Iterations; i++) {
			 MapboxDelaunatorFactory.Create( _coords );
		}
	}
}
