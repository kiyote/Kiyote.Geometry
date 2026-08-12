```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.8893/24H2/2024Update/HudsonValley)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400-preview.0.26322.102
  [Host] : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=1  WarmupCount=10  

```
# D3DelaunayFactoryBenchmarks
| Method           | Mean         | Error        | StdDev     | Allocated   |
|----------------- |-------------:|-------------:|-----------:|------------:|
| Create_100x100   |     75.83 μs |     1.876 μs |   1.755 μs |   102.25 KB |
| Create_500x500   |  3,769.69 μs |   238.757 μs | 223.333 μs |  2562.21 KB |
| Create_1000x1000 | 17,011.50 μs | 1,039.295 μs | 972.157 μs | 10278.88 KB |

# D3VoronoiFactory Benchmarks
| Method           | Mean        | Error       | StdDev      | Allocated   |
|----------------- |------------:|------------:|------------:|------------:|
| Create_100x100   |    293.5 μs |     6.66 μs |     6.23 μs |   471.42 KB |
| Create_500x500   | 12,983.8 μs |   719.87 μs |   638.15 μs | 13666.71 KB |
| Create_1000x1000 | 69,793.4 μs | 1,691.23 μs | 1,499.23 μs | 56312.05 KB |

# MapboxDelaunatorFactory
| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     55.12 μs |   1.339 μs |   1.253 μs |   47.63 KB |
| Create_500x500   |  3,002.53 μs | 178.270 μs | 166.753 μs | 1168.46 KB |
| Create_1000x1000 | 13,134.60 μs | 118.755 μs | 105.273 μs | 4681.18 KB |

# Edge
| Method              | Mean       | Error     | StdDev    | Median     | Allocated |
|-------------------- |-----------:|----------:|----------:|-----------:|----------:|
| HasIntersection     |  0.0041 ns | 0.0093 ns | 0.0087 ns |  0.0000 ns |         - |
| TryFindIntersection | 11.0458 ns | 0.3005 ns | 0.2664 ns | 10.9902 ns |         - |
| GetBoundingBox      |  0.4301 ns | 0.0929 ns | 0.0869 ns |  0.4048 ns |         - |
| GetMidpoint         |  0.0000 ns | 0.0000 ns | 0.0000 ns |  0.0000 ns |         - |
| Ctor_PointPoint     |  0.0084 ns | 0.0127 ns | 0.0113 ns |  0.0018 ns |         - |
| Ctor_IntIntIntInt   |  0.0007 ns | 0.0024 ns | 0.0023 ns |  0.0000 ns |         - |

# FastPoissonDiscPointFactory
| Method         | Mean        | Error     | StdDev    | Allocated  |
|--------------- |------------:|----------:|----------:|-----------:|
| Fill_100x100   |    151.7 μs |   3.19 μs |   2.67 μs |   19.92 KB |
| Fill_500x500   |  3,946.3 μs |  66.98 μs |  55.93 μs |  358.58 KB |
| Fill_1000x1000 | 15,481.4 μs | 187.44 μs | 166.16 μs | 1412.79 KB |

# FastRandom
| Method                                    | Mean      | Error     | StdDev    | Allocated |
|------------------------------------------ |----------:|----------:|----------:|----------:|
| FastRandom_NextInt                        |  1.428 ns | 0.0521 ns | 0.0435 ns |         - |
| FastRandom_NextInt_UpperBound             |  3.493 ns | 0.0408 ns | 0.0382 ns |         - |
| FastRandom_NextInt_LowerBoundUpperBound   |  4.136 ns | 0.1601 ns | 0.1420 ns |         - |
| FastRandom_NextBool                       |  2.349 ns | 0.1085 ns | 0.1015 ns |         - |
| FastRandom_NextUInt                       |  1.506 ns | 0.0262 ns | 0.0232 ns |         - |
| FastRandom_NextDouble                     |  1.563 ns | 0.1375 ns | 0.1286 ns |         - |
| FastRandom_NextFloat                      |  1.776 ns | 0.1372 ns | 0.1146 ns |         - |
| FastRandom_NextFloat_LowerBoundUpperBound |  3.677 ns | 0.0811 ns | 0.0759 ns |         - |
| FastRandom_NextBytes                      | 30.432 ns | 0.2349 ns | 0.2197 ns |         - |

# FastRandom Benchmarks vs System.Random
| Method                  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| FastRandom_NextBytes    | 30.530 ns | 0.3745 ns | 0.3503 ns |  1.66 |    0.02 |         - |          NA |
| SystemRandom_NextBytes  | 18.375 ns | 0.1930 ns | 0.1711 ns |  1.00 |    0.01 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextDouble   |  1.173 ns | 0.0260 ns | 0.0217 ns |  0.36 |    0.01 |         - |          NA |
| SystemRandom_NextDouble |  3.232 ns | 0.0909 ns | 0.0851 ns |  1.00 |    0.04 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextFloat    |  1.495 ns | 0.0577 ns | 0.0512 ns |  0.46 |    0.02 |         - |          NA |
| SystemRandom_NextFloat  |  3.217 ns | 0.0626 ns | 0.0488 ns |  1.00 |    0.02 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextInt      |  1.311 ns | 0.0584 ns | 0.0547 ns |  0.48 |    0.03 |         - |          NA |
| SystemRandom_NextInt    |  2.734 ns | 0.1151 ns | 0.1077 ns |  1.00 |    0.05 |         - |          NA |

# Polygon
| Method                                            | Mean        | Error     | StdDev    | Allocated |
|-------------------------------------------------- |------------:|----------:|----------:|----------:|
| Ctor                                              |    31.61 ns |  1.295 ns |  1.211 ns |     240 B |
| Contains_Point                                    |    30.58 ns |  0.294 ns |  0.275 ns |         - |
| Contains_Polygon                                  |    33.12 ns |  0.716 ns |  0.634 ns |         - |
| HasOverlap                                        |    32.87 ns |  0.558 ns |  0.495 ns |         - |
| IsEquivalentTo                                    |    44.72 ns |  2.099 ns |  1.963 ns |         - |
| TryIntersect_WithIntersection                     | 1,072.20 ns | 12.728 ns | 11.283 ns |    1344 B |
| TryIntersect_WithoutIntersection                  |    83.91 ns |  3.182 ns |  2.977 ns |      24 B |
| TryFindIntersections_Edge_WithIntersections       |    67.18 ns |  1.467 ns |  1.300 ns |     104 B |
| TryFindIntersections_Edge_WithoutIntersections    |    17.23 ns |  0.524 ns |  0.490 ns |         - |
| TryFindIntersections_Polygon_WithIntersections    |   133.31 ns |  2.801 ns |  2.620 ns |     104 B |
| TryFindIntersections_Polygon_WithoutIntersections |    73.76 ns |  2.431 ns |  2.274 ns |         - |
| HasIntersection_Edge                              |    11.37 ns |  0.278 ns |  0.260 ns |         - |
| HasIntersection_Polygon                           |    57.59 ns |  1.101 ns |  0.976 ns |         - |
| HasIntersection_EdgeArray                         |    57.35 ns |  0.350 ns |  0.292 ns |         - |

# Rectangle
| Method                      | Mean      | Error     | StdDev    | Median    | Allocated |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|
| Contains_Rect               | 0.0431 ns | 0.0691 ns | 0.0612 ns | 0.0176 ns |         - |
| Contains_IRect              | 2.2724 ns | 0.0687 ns | 0.0643 ns | 2.2670 ns |         - |
| Contains_Point              | 0.0016 ns | 0.0068 ns | 0.0064 ns | 0.0000 ns |         - |
| Contains_IntInt             | 0.0166 ns | 0.0198 ns | 0.0176 ns | 0.0107 ns |         - |
| IsEquivalentTo_Rect         | 0.0157 ns | 0.0200 ns | 0.0178 ns | 0.0048 ns |         - |
| IsEquivalentTo_IRect        | 0.2314 ns | 0.0366 ns | 0.0342 ns | 0.2220 ns |         - |
| IsEquivalentTo_PointPoint   | 0.0122 ns | 0.0185 ns | 0.0173 ns | 0.0016 ns |         - |
| IsEquivalentTo_IntIntIntInt | 0.0201 ns | 0.0178 ns | 0.0167 ns | 0.0192 ns |         - |

# SimpleQuadTreeNode
| Method             | Mean          | Error         | StdDev        | Allocated |
|------------------- |--------------:|--------------:|--------------:|----------:|
| Count              |  80,349.36 ns |  2,626.041 ns |  2,456.400 ns |       1 B |
| GetSubTreeContents | 615,621.04 ns | 23,288.308 ns | 21,783.897 ns |  788016 B |
| Insert             |      32.61 ns |      1.035 ns |      0.864 ns |     200 B |
| Query              |   4,172.36 ns |    235.450 ns |    208.720 ns |   23973 B |

# MidpointDisplacementNoisyEdgeFactory
| Method                     | Mean       | Error    | StdDev   | Allocated |
|--------------------------- |-----------:|---------:|---------:|----------:|
| Create_Amplitude05_Levels3 |   502.8 ns | 19.07 ns | 15.92 ns |     912 B |
| Create_Amplitude05_Levels4 | 1,044.4 ns | 10.64 ns |  8.30 ns |    1680 B |
