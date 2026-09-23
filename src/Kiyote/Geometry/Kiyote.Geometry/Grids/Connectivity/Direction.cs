namespace Kiyote.Geometry.Grids.Connectivity;

[Flags]
[System.Diagnostics.CodeAnalysis.SuppressMessage( "Design", "CA1028:Enum Storage should be Int32", Justification = "Backed by byte intentionally so values can be stored directly in an IGrid<byte> without conversion, one byte per cell." )]
public enum Direction : byte {
	None = 0,
	North = 1 << 0,
	NorthEast = 1 << 1,
	East = 1 << 2,
	SouthEast = 1 << 3,
	South = 1 << 4,
	SouthWest = 1 << 5,
	West = 1 << 6,
	NorthWest = 1 << 7
}
