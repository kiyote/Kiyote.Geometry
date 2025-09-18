using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Kiyote.Geometry.Benchmarks;
using Kiyote.Geometry.Benchmarks.DelaunayVoronoi;
using Kiyote.Geometry.Benchmarks.Randomization;
using Kiyote.Geometry.Trees.Benchmarks;

ManualConfig config = DefaultConfig.Instance
	.AddExporter( MarkdownExporter.Default )
	.AddJob( Job
		 .MediumRun
		 .WithLaunchCount( 1 )
		 .WithToolchain( InProcessNoEmitToolchain.Instance ) );

BenchmarkRunner.Run<D3DelaunayFactoryBenchmarks>( config );
BenchmarkRunner.Run<D3VoronoiFactoryBenchmarks>( config );
BenchmarkRunner.Run<EdgeBenchmarks>( config );
BenchmarkRunner.Run<FastPoissonDiscPointFactoryBenchmarks>( config );
BenchmarkRunner.Run<FastRandomBenchmarks>( config );
BenchmarkRunner.Run<FastRandomVsSystemBenchmarks>( config );
BenchmarkRunner.Run<MapboxDelaunatorFactoryBenchmarks>( config );
BenchmarkRunner.Run<PolygonBenchmarks>( config );
BenchmarkRunner.Run<RectangleBenchmarks>( config );
BenchmarkRunner.Run<SimpleQuadTreeNodeBenchmarks>( config );

