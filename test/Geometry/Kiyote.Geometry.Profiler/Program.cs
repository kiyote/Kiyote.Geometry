﻿using Kiyote.Geometry.DelaunayVoronoi.Profiler;

#pragma warning disable CA1852 // Until https://github.com/dotnet/roslyn-analyzers/issues/6141 is fixed
DelaunatorFactoryProfiler profiler = new DelaunatorFactoryProfiler();
profiler.Profile();
#pragma warning restore CA1852