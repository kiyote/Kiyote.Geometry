
namespace Kiyote.Geometry.DelaunayVoronoi.Profiler;

public sealed class D3VoronoiFactoryProfiler {

	public const int Iterations = 1000;
	public const int Separation = 5;
	private readonly IReadOnlyList<Point> _points;
	private readonly IVoronoiFactory _voronoiFactory;
	private readonly Rect _bounds;

	public D3VoronoiFactoryProfiler() {
		ISize size = new Point( 1000, 1000 );
		_bounds = new Rect( 0, 0, size );

		int cellWidth = size.Width / 20;
		int cellHeight = size.Height / 20;

		List<Point> points = [];
		for( int c = cellWidth / 2; c < size.Width; c += cellWidth ) {
			for( int r = cellHeight / 2; r < size.Height; r += cellHeight ) {
				points.Add( new Point( c, r ) );
			}
		}
		_points = points;

		_voronoiFactory = new D3VoronoiFactory();
	}

	public void Profile() {
		for( int i = 0; i < Iterations; i++ ) {
			 _voronoiFactory.Create( _bounds, _points );
		}
	}
}
