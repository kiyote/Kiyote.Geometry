using Kiyote.Geometry.DelaunayVoronoi;

namespace Kiyote.Geometry.Testing;

internal sealed class D3WorkloadFixture {
	private D3WorkloadFixture( Rect bounds, Point[] input, Point[] sites, bool sanitize ) {
		Bounds = bounds;
		Input = input;
		Sites = sites;
		Sanitize = sanitize;
	}

	internal Rect Bounds { get; }
	internal Point[] Input { get; }
	internal Point[] Sites { get; }
	internal bool Sanitize { get; }

	internal static D3WorkloadFixture Create( string workload ) {
		if( workload == "MostlyHull" ) {
			Point[] ring = new Point[128];
			for( int i = 0; i < ring.Length; i++ ) {
				double angle = i * 2 * Math.PI / ring.Length;
				ring[i] = new Point( (int)Math.Round( 15000 * Math.Cos( angle ) ), (int)Math.Round( 15000 * Math.Sin( angle ) ) );
			}
			return new D3WorkloadFixture( new Rect( -20000, -20000, 40001, 40001 ), ring, ring, false );
		}
		if( workload == "Sanitized" ) {
			Point[] sites = new Point[400];
			for( int i = 0; i < sites.Length; i++ ) {
				sites[i] = new Point( -9500 + (i % 20 * 1000), -9500 + (i / 20 * 1000) );
			}
			Point[] input = new Point[40000];
			for( int i = 0; i < input.Length; i++ ) {
				input[i] = i % 10 == 0 ? sites[i / 10 % sites.Length] : new Point( 30000, 30000 );
			}
			return new D3WorkloadFixture( new Rect( -10000, -10000, 20001, 20001 ), input, sites, true );
		}
		if( workload is not ("Irregular" or "Thin") ) {
			throw new ArgumentException( "Expected Irregular, Thin, MostlyHull or Sanitized.", nameof( workload ) );
		}
		bool thin = workload == "Thin";
		Point[] points = new Point[thin ? 512 : 4096];
		Random random = new Random( 7291 );
		for( int i = 0; i < points.Length; i++ ) {
			int x = thin ? -32000 + (i % 64 * 1000) + random.Next( -100, 101 )
				: -18900 + (i % 64 * 600) + random.Next( -100, 101 );
			int y = thin ? 15100 + (i / 64 * 100) + random.Next( -20, 21 )
				: -18900 + (i / 64 * 600) + random.Next( -100, 101 );
			points[i] = new Point( x, y );
		}
		Rect bounds = thin ? new Rect( -32768, 15000, 65536, 1001 ) : new Rect( -20000, -20000, 40001, 40001 );
		return new D3WorkloadFixture( bounds, points, points, false );
	}

	internal void Validate( IVoronoi result ) {
		if( result.Cells.Length != Sites.Length || result.Points.Count != Sites.Length || result.Edges.Count == 0 ) {
			throw new InvalidOperationException( "D3 workload did not produce complete output." );
		}
		for( int i = 0; i < Sites.Length; i++ ) {
			if( result.Points[i] != Sites[i] || !result.Contains( i, Sites[i] ) ) {
				throw new InvalidOperationException( $"D3 workload cell {i} does not preserve/contain its site." );
			}
		}
	}
}
