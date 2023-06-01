using BenchmarkDotNet.Running;
using Kiyote.Geometry;
using Kiyote.Geometry.Benchmarks;
using Kiyote.Geometry.Benchmarks.DelaunayVoronoi;
using Kiyote.Geometry.Benchmarks.Randomization;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;


#pragma warning disable CA1852 // Until https://github.com/dotnet/roslyn-analyzers/issues/6141 is fixed
BenchmarkRunner.Run<D3VoronoiFactoryBenchmarks>();
#pragma warning restore CA1852

