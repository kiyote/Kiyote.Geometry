using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Kiyote.Geometry;
using Kiyote.Geometry.Benchmarks;
using Kiyote.Geometry.Benchmarks.DelaunayVoronoi;
using Kiyote.Geometry.Benchmarks.Randomization;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;


#pragma warning disable CA1852 // Until https://github.com/dotnet/roslyn-analyzers/issues/6141 is fixed


ManualConfig config = DefaultConfig.Instance
	.AddJob( Job
		 .MediumRun
		 .WithLaunchCount( 1 )
		 .WithToolchain( InProcessNoEmitToolchain.Instance ) );

BenchmarkRunner.Run<D3VoronoiFactoryBenchmarks>( config );


/*
var b = new D3VoronoiFactoryBenchmarks();
b.Create_1000x1000();
*/

#pragma warning restore CA1852

