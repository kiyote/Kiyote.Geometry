namespace Kiyote.Geometry.DelaunayVoronoi;

public sealed record Delaunator(
	IReadOnlyList<double> Coords,
	IReadOnlyList<int> Hull,
	IReadOnlyList<int> Triangles,
	IReadOnlyList<int> HalfEdges
) {
	// Provides a loop around the edges.  If this is the third edge, it returns
	// the first edge, otherwise returns the next edge.
	public static int NextHalfedge( int e ) =>  e % 3 == 2  ? e - 2 : e + 1;

	public static void Circumcenter(
		double ax,
		double ay,
		double bx,
		double by,
		double cx,
		double cy,
		out double x,
		out double y
	) {
		double dx = bx - ax;
		double dy = by - ay;
		double ex = cx - ax;
		double ey = cy - ay;

		double bl =  dx * dx  +  dy * dy ;
		double cl =  ex * ex  +  ey * ey ;
		double d = 0.5D / (  dx * ey  -  dy * ex  );

		x = ax +  (  ey * bl  -  dy * cl  ) * d ;
		y = ay +  (  dx * cl  -  ex * bl  ) * d ;
	}
}
