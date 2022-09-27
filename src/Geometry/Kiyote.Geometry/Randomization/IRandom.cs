namespace Kiyote.Geometry.Randomization;

public interface IRandom {
	void Reinitialise( int seed );

	int NextInt();

	byte NextByte();

	bool NextBool();

	uint NextUInt();

	double NextDouble();

	float NextFloat();

	void NextBytes( byte[] buffer );

	int NextInt( int upperBound );

	int NextInt( int lowerBound, int upperBound );

	float NextFloat( float lowerBound, float upperBound );
}
