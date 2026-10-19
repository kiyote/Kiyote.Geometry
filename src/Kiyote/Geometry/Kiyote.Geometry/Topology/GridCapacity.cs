namespace Kiyote.Geometry.Topology;

internal static class GridCapacity {
	internal static int GetLength( long length, string parameterName ) {
		if( length < 0 || length > Array.MaxLength ) {
			throw new ArgumentOutOfRangeException( parameterName, "Grid storage exceeds the supported array capacity." );
		}
		return (int)length;
	}
}
