namespace Kiyote.Geometry.Randomization;

public interface IRandom {
	void Reinitialise( int seed );

	/// <summary>
	/// Generates a random value in the range [0, int.MaxValue]
	/// </summary>
	/// <returns>
	/// Returns a value from [0, int.MaxValue]
	/// </returns>
	int NextInt();

	/// <summary>
	/// Generates a random value in the range [0, upperBound]
	/// </summary>
	/// <param name="upperBound">The maximum value to be returned.</param>
	/// <returns>
	/// Returns a value from [0, upperBound]
	/// </returns>
	int NextInt( int upperBound );

	/// <summary>
	/// Generates a random value in the range [lowerBound, upperBound]
	/// </summary>
	/// <param name="lowerBound">The minimum value to be returned.</param>
	/// <param name="upperBound">The maximum value to be returned.</param>
	/// <returns>
	/// Returns a value from [lowerBound, upperBound]
	/// </returns>
	int NextInt( int lowerBound, int upperBound );

	bool NextBool();

	/// <summary>
	/// Generates a random value in the range [uint.MinValue, uint.MaxValue]
	/// </summary>
	/// <returns>
	/// Returns a value from [uint.MinValue, uint.MaxValue]
	/// </returns>
	uint NextUInt();

	/// <summary>
	/// Returns a value in (0, 1).
	/// </summary>
	/// <remarks>
	/// That is, the returned value is exclusive of 0 and 1.
	/// </remarks>
	/// <returns>
	/// A random value: 1 > value > 0.
	/// </returns>
	double NextDouble();

	/// <summary>
	/// Returns a value in (0, 1).
	/// </summary>
	/// <remarks>
	/// That is, the returned value is exclusive of 0 and 1.
	/// </remarks>
	/// <returns>
	/// A random value: 1 > value > 0.
	/// </returns>
	float NextFloat();

	/// <summary>
	/// Returns a value in (0, 1) * (upperBound - lowerBound).
	/// </summary>
	/// <remarks>
	/// That is, the returned value is exclusive of lowerBound and upperBound.
	/// </remarks>
	/// <returns>
	/// A random value: upperBound > value > lowerBound.
	/// </returns>
	float NextFloat( float lowerBound, float upperBound );

	void NextBytes( Span<byte> buffer );
}
