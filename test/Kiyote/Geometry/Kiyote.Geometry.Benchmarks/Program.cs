using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Kiyote.Geometry.Benchmarks;
using Kiyote.Geometry.Benchmarks.Rasterizers;
using Kiyote.Geometry.DelaunayVoronoi.Benchmarks;
using Kiyote.Geometry.Trees.Benchmarks;

ManualConfig config = DefaultConfig.Instance
	.AddExporter( MarkdownExporter.Default )
	.AddJob( Job
		 .MediumRun
		 .WithLaunchCount( 1 )
		 .WithToolchain( InProcessNoEmitToolchain.Instance ) );

BenchmarkSwitcher
	.FromTypes( [
		typeof( D3DelaunayFactoryBenchmarks ),
		typeof( D3VoronoiFactoryBenchmarks ),
		typeof( MapboxDelaunatorFactoryBenchmarks ),
		typeof( EdgeBenchmarks ),
		typeof( IntersectBenchmarks ),
		typeof( PolygonBenchmarks ),
		typeof( RectangleBenchmarks ),
		typeof( SimpleQuadTreeNodeBenchmarks ),
		typeof( IntegerRasterizerBenchmarks ),
	] )
	.RunAll( config, args );


