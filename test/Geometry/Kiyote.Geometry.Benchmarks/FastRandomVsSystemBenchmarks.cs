using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Benchmarks;


[MemoryDiagnoser( false )]
[GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory )]
public class FastRandomVsSystemBenchmarks {

	private readonly IRandom _fastRandom;
	private readonly Random _systemRandom;
	private readonly byte[] _buffer;

	public FastRandomVsSystemBenchmarks() {
		_fastRandom = new FastRandom();
		_systemRandom = new Random();
		_buffer = new byte[100];
	}

	[BenchmarkCategory( "NextInt" ), Benchmark]
	public void FastRandom_NextInt() {
		_fastRandom.NextInt();
	}

	[BenchmarkCategory( "NextInt" ), Benchmark( Baseline = true )]
	public void SystemRandom_NextInt() {
		_systemRandom.Next();
	}

	[BenchmarkCategory( "NextFloat" ), Benchmark]
	public void FastRandom_NextFloat() {
		_fastRandom.NextFloat();
	}

	[BenchmarkCategory( "NextFloat" ), Benchmark( Baseline = true )]
	public void SystemRandom_NextFloat() {
		_systemRandom.NextSingle();
	}

	[BenchmarkCategory( "NextDouble" ), Benchmark]
	public void FastRandom_NextDouble() {
		_fastRandom.NextDouble();
	}

	[BenchmarkCategory( "NextDouble" ), Benchmark( Baseline = true )]
	public void SystemRandom_NextDouble() {
		_systemRandom.NextDouble();
	}

	[BenchmarkCategory( "NextBytes" ), Benchmark]
	public void FastRandom_NextBytes() {
		_fastRandom.NextBytes( _buffer );
	}

	[BenchmarkCategory( "NextBytes" ), Benchmark( Baseline = true )]
	public void SystemRandom_NextBytes() {
		_systemRandom.NextBytes( _buffer );
	}
}
