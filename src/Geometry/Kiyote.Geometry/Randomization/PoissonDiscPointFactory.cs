using System.Numerics;

namespace Kiyote.Geometry.Randomization;

/*
 * Code derived from:
 * https://github.com/SebLague/Poisson-Disc-Sampling
 */

// This produces a fairly "true" random distribution within the bounds
// supplied.  Whereas FastPoissonDiscPointFactory is slightly less uniform
// in its distribution.

internal sealed class PoissonDiscPointFactory : IPointFactory {

	public const int NumSamplesBeforeRejection = 30;
	public const float Sqrt2 = 1.414213562f;
	private readonly IRandom _random;

	public PoissonDiscPointFactory(
		IRandom random
	) {
		_random = random;
	}

	IReadOnlyList<IPoint> IPointFactory.Fill(
		IBounds bounds,
		int distanceApart
	) {
		float fDistanceApart = distanceApart;
		Vector2 sampleRegionSize = new Vector2( bounds.Width, bounds.Height );
		float cellSize =  fDistanceApart / Sqrt2 ;

		int[,] grid = new int[(int)Math.Ceiling( sampleRegionSize.X / cellSize ), (int)Math.Ceiling( sampleRegionSize.Y / cellSize )];
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2> {
			sampleRegionSize / 2
		};
		while( spawnPoints.Count > 0 ) {
			int spawnIndex = _random.NextInt( 0, spawnPoints.Count );
			Vector2 spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for( int i = 0; i < NumSamplesBeforeRejection; i++ ) {
				double angle = _random.NextDouble() * Math.PI * 2;
				Vector2 dir = new Vector2( (float)Math.Sin( angle ), (float)Math.Cos( angle ) );
				Vector2 candidate = spawnCentre +  dir * _random.NextFloat( fDistanceApart, 2 * fDistanceApart ) ;
				if( IsValid( candidate, sampleRegionSize, cellSize, fDistanceApart, points, grid ) ) {
					points.Add( candidate );
					spawnPoints.Add( candidate );
					grid[(int)( candidate.X / cellSize ), (int)( candidate.Y / cellSize )] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if( !candidateAccepted ) {
				spawnPoints.RemoveAt( spawnIndex );
			}

		}


		return points.Select( p => new Point( (int)p.X, (int)p.Y ) ).ToList();
	}

	private static bool IsValid(
		Vector2 candidate,
		Vector2 sampleRegionSize,
		float cellSize, float radius,
		List<Vector2> points,
		int[,] grid
	) {
		if( candidate.X >= 0
			&& candidate.X < sampleRegionSize.X
			&& candidate.Y >= 0
			&& candidate.Y < sampleRegionSize.Y
		) {
			int cellX = (int)( candidate.X / cellSize );
			int cellY = (int)( candidate.Y / cellSize );
			int searchStartX = Math.Max( 0, cellX - 2 );
			int searchEndX = Math.Min( cellX + 2, grid.GetLength( 0 ) - 1 );
			int searchStartY = Math.Max( 0, cellY - 2 );
			int searchEndY = Math.Min( cellY + 2, grid.GetLength( 1 ) - 1 );

			for( int x = searchStartX; x <= searchEndX; x++ ) {
				for( int y = searchStartY; y <= searchEndY; y++ ) {
					int pointIndex = grid[x, y] - 1;
					if( pointIndex != -1 ) {
						float sqrDst = ( candidate - points[pointIndex] ).LengthSquared();
						if( sqrDst < radius * radius ) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
}
