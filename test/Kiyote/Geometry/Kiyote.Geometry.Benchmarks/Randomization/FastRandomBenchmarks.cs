using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks.Randomization;


[MemoryDiagnoser( false )]
public class FastRandomBenchmarks {

	private readonly IRandom _fastRandom;
	private readonly byte[] _buffer;

	public FastRandomBenchmarks() {
		_fastRandom = new FastRandom();
		_buffer = new byte[100];
	}

	[Benchmark]
	public void FastRandom_NextInt() {
		_fastRandom.NextInt();
	}

	[Benchmark]
	public void FastRandom_NextInt_UpperBound() {
		_fastRandom.NextInt( 1000 );
	}

	[Benchmark]
	public void FastRandom_NextInt_LowerBoundUpperBound() {
		_fastRandom.NextInt( 100, 1000 );
	}

	[Benchmark]
	public void FastRandom_NextBool() {
		_fastRandom.NextBool();
	}

	[Benchmark]
	public void FastRandom_NextUInt() {
		_fastRandom.NextUInt();
	}

	[Benchmark]
	public void FastRandom_NextDouble() {
		_fastRandom.NextDouble();
	}

	[Benchmark]
	public void FastRandom_NextFloat() {
		_fastRandom.NextFloat();
	}

	[Benchmark]
	public void FastRandom_NextFloat_LowerBoundUpperBound() {
		_fastRandom.NextFloat( 100.0f, 1000.0f );
	}

	[Benchmark]
	public void FastRandom_NextBytes() {
		_fastRandom.NextBytes( _buffer );
	}
}
