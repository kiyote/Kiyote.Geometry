using BenchmarkDotNet.Running;
using Kiyote.Geometry;
using Kiyote.Geometry.Benchmarks;
using Kiyote.Geometry.Benchmarks.DelaunayVoronoi;
using Kiyote.Geometry.Benchmarks.Randomization;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;


BenchmarkRunner.Run<VoronoiFactoryBenchmarks>();


