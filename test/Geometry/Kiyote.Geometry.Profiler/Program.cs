using Kiyote.Geometry.DelaunayVoronoi.Profiler;
using Kiyote.Geometry.Randomization.Profiler;

#pragma warning disable CA1852 // Until https://github.com/dotnet/roslyn-analyzers/issues/6141 is fixed
var profiler = new FastPoissonDiscPointFactoryProfiler();
profiler.Profile();
#pragma warning restore CA1852
