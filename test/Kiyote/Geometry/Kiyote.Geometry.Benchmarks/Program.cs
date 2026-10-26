using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Kiyote.Geometry.Benchmarks;
using Kiyote.Geometry.Benchmarks.DelaunayVoronoi.D3;
using Kiyote.Geometry.Benchmarks.Rasterizers;
using Kiyote.Geometry.Benchmarks.Topology;
using Kiyote.Geometry.DelaunayVoronoi.Benchmarks;
using Kiyote.Geometry.Trees.Benchmarks;

ManualConfig config = DefaultConfig.Instance
	.AddExporter( MarkdownExporter.Default );

Summary[] summaries = BenchmarkSwitcher
	.FromTypes( [
		typeof( D3DelaunayFactoryBenchmarks ),
		typeof( D3VoronoiFactoryBenchmarks ),
		typeof( MapboxDelaunatorFactoryBenchmarks ),
		typeof( EdgeBenchmarks ),
		typeof( IntersectBenchmarks ),
		typeof( GeometryWorkloadBenchmarks ),
		typeof( VoronoiWorkloadBenchmarks ),
		typeof( PolygonBenchmarks ),
		typeof( RectangleBenchmarks ),
		typeof( SimpleQuadTreeNodeBenchmarks ),
		typeof( IntegerRasterizerBenchmarks ),
		typeof( TriangleRepresentationBenchmarks ),
		typeof( SearchableVoronoiBenchmarks ),
		typeof( PointBenchmarks ),
		typeof( DenseGridSourceBenchmarks ),
		typeof( GridAssemblyBenchmarks ),
		typeof( GridCompilerBenchmarks ),
		typeof( CompiledGridAssemblyBenchmarks ),
		typeof( CompiledGridAssemblyMutationBenchmarks ),
		typeof( TopologyLocalityBenchmarks ),
		typeof( ConnectivityBuilderBenchmarks ),
		typeof( DirectionExtensionsBenchmarks )
	] )
	.Run( args, config )
	.ToArray();

return summaries.Any( summary => summary.HasCriticalValidationErrors
	|| summary.Reports.Any( report => !report.Success ) ) ? 1 : 0;
