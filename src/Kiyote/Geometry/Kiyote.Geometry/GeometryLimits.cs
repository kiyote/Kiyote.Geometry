using System.Runtime.CompilerServices;

namespace Kiyote.Geometry;

/// <summary>
/// Coordinate limits for bounded geometry algorithms, not shared coordinate
/// storage, translations, rectangle bounds, or topology addressing.
/// </summary>
public static class GeometryLimits {

	public const int MinimumCoordinate = -32768;
	public const int MaximumCoordinate = 32767;

	internal static void Validate(
		Point point,
		string parameterName
	) {
		if( point.X < MinimumCoordinate || point.X > MaximumCoordinate
			|| point.Y < MinimumCoordinate || point.Y > MaximumCoordinate
		) {
			throw new ArgumentOutOfRangeException( parameterName, point, "Geometry coordinates must be in [-32768, 32767]." );
		}
	}

	internal static void ValidateCoordinate(
		int coordinate,
		[CallerArgumentExpression( nameof( coordinate ) )] string? parameterName = null
	) {
		if( coordinate < MinimumCoordinate || coordinate > MaximumCoordinate ) {
			throw new ArgumentOutOfRangeException( parameterName, coordinate, "Geometry coordinates must be in [-32768, 32767]." );
		}
	}
}
