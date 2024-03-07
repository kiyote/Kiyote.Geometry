namespace Kiyote.Geometry.DelaunayVoronoi;

internal static class ExtensionMethods {
	public static double[] ToCoords(
		this IReadOnlyList<Point> points
	) {
		double[] coords = new double[points.Count * 2];
		for( int i = 0; i < points.Count; i++ ) {
			coords[( i * 2 ) + 0] = points[i].X;
			coords[( i * 2 ) + 1] = points[i].Y;
		}
		return coords;
	}
}
