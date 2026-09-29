```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.8893/24H2/2024Update/HudsonValley)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400-preview.0.26322.102
  [Host] : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=1  WarmupCount=10  

```
# D3DelaunayFactoryBenchmarks
| Method           | Mean         | Error      | StdDev     | Allocated   |
|----------------- |-------------:|-----------:|-----------:|------------:|
| Create_100x100   |     73.38 μs |   1.924 μs |   1.705 μs |   102.25 KB |
| Create_500x500   |  3,396.07 μs |  60.224 μs |  53.387 μs |  2562.19 KB |
| Create_1000x1000 | 16,345.21 μs | 625.686 μs | 585.267 μs | 10278.88 KB |

# D3VoronoiFactory Benchmarks
| Method           | Mean        | Error       | StdDev      | Allocated   |
|----------------- |------------:|------------:|------------:|------------:|
| Create_100x100   |    327.3 μs |     8.79 μs |     8.23 μs |   471.42 KB |
| Create_500x500   | 12,976.2 μs |   494.36 μs |   462.43 μs | 13666.71 KB |
| Create_1000x1000 | 68,843.7 μs | 3,229.21 μs | 3,020.60 μs | 56312.43 KB |

# MapboxDelaunatorFactory
| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     55.47 μs |   1.392 μs |   1.163 μs |   47.63 KB |
| Create_500x500   |  2,924.15 μs |  54.980 μs |  51.428 μs | 1168.46 KB |
| Create_1000x1000 | 13,839.84 μs | 435.720 μs | 386.255 μs | 4681.18 KB |

# Edge
| Method              | Mean       | Error     | StdDev    | Median     | Allocated |
|-------------------- |-----------:|----------:|----------:|-----------:|----------:|
| HasIntersection     |  0.0000 ns | 0.0000 ns | 0.0000 ns |  0.0000 ns |         - |
| TryFindIntersection | 11.5721 ns | 0.4656 ns | 0.4355 ns | 11.4618 ns |         - |
| GetBoundingBox      |  0.1542 ns | 0.0431 ns | 0.0360 ns |  0.1355 ns |         - |
| GetMidpoint         |  0.0170 ns | 0.0379 ns | 0.0336 ns |  0.0000 ns |         - |
| Ctor_PointPoint     |  0.0419 ns | 0.0482 ns | 0.0451 ns |  0.0349 ns |         - |
| Ctor_IntIntIntInt   |  0.0284 ns | 0.0319 ns | 0.0299 ns |  0.0222 ns |         - |

# FastPoissonDiscPointFactory
| Method         | Mean        | Error     | StdDev    | Allocated  |
|--------------- |------------:|----------:|----------:|-----------:|
| Fill_100x100   |    164.7 μs |   6.19 μs |   5.79 μs |   19.92 KB |
| Fill_500x500   |  4,469.6 μs | 115.99 μs | 108.50 μs |  358.57 KB |
| Fill_1000x1000 | 16,583.9 μs | 730.61 μs | 683.41 μs | 1412.79 KB |

# FastRandom
| Method                                    | Mean      | Error     | StdDev    | Allocated |
|------------------------------------------ |----------:|----------:|----------:|----------:|
| FastRandom_NextInt                        |  1.526 ns | 0.1267 ns | 0.1185 ns |         - |
| FastRandom_NextInt_UpperBound             |  4.102 ns | 0.2222 ns | 0.1970 ns |         - |
| FastRandom_NextInt_LowerBoundUpperBound   |  4.248 ns | 0.0220 ns | 0.0206 ns |         - |
| FastRandom_NextBool                       |  2.447 ns | 0.0955 ns | 0.0847 ns |         - |
| FastRandom_NextUInt                       |  1.327 ns | 0.0590 ns | 0.0523 ns |         - |
| FastRandom_NextDouble                     |  1.384 ns | 0.0511 ns | 0.0478 ns |         - |
| FastRandom_NextFloat                      |  1.476 ns | 0.0897 ns | 0.0700 ns |         - |
| FastRandom_NextFloat_LowerBoundUpperBound |  3.370 ns | 0.1795 ns | 0.1679 ns |         - |
| FastRandom_NextBytes                      | 31.426 ns | 0.3619 ns | 0.2826 ns |         - |

# FastRandom Benchmarks vs System.Random
| Method                  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| FastRandom_NextBytes    | 32.490 ns | 0.9221 ns | 0.8626 ns |  1.70 |    0.06 |         - |          NA |
| SystemRandom_NextBytes  | 19.094 ns | 0.4838 ns | 0.4289 ns |  1.00 |    0.03 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextDouble   |  1.508 ns | 0.1713 ns | 0.1518 ns |  0.46 |    0.05 |         - |          NA |
| SystemRandom_NextDouble |  3.312 ns | 0.1694 ns | 0.1585 ns |  1.00 |    0.07 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextFloat    |  1.350 ns | 0.1115 ns | 0.1043 ns |  0.39 |    0.03 |         - |          NA |
| SystemRandom_NextFloat  |  3.472 ns | 0.1823 ns | 0.1705 ns |  1.00 |    0.07 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextInt      |  1.146 ns | 0.0732 ns | 0.0612 ns |  0.46 |    0.03 |         - |          NA |
| SystemRandom_NextInt    |  2.492 ns | 0.1004 ns | 0.0890 ns |  1.00 |    0.05 |         - |          NA |

# Intersect
| Method              | Mean   | Error  | Allocated |
|-------------------- |-------:|-------:|----------:|
| HasIntersection     | 0.0 ns | 0.0 ns |         - |
| TryFindIntersection | 0.0 ns | 0.0 ns |         - |

# Polygon
| Method                                            | Mean      | Error     | StdDev    | Allocated |
|-------------------------------------------------- |----------:|----------:|----------:|----------:|
| Ctor                                              |  30.20 ns |  1.529 ns |  1.430 ns |     240 B |
| Contains_Point                                    |  28.60 ns |  0.293 ns |  0.260 ns |         - |
| Contains_Polygon                                  |  29.55 ns |  0.368 ns |  0.308 ns |         - |
| HasOverlap                                        |  31.81 ns |  0.798 ns |  0.707 ns |         - |
| IsEquivalentTo                                    |  34.58 ns |  0.946 ns |  0.839 ns |         - |
| TryIntersect_WithIntersection                     | 984.13 ns | 51.072 ns | 47.773 ns |    1344 B |
| TryIntersect_WithoutIntersection                  |  77.04 ns |  2.060 ns |  1.826 ns |      24 B |
| TryFindIntersections_Edge_WithIntersections       |  72.69 ns |  2.076 ns |  1.942 ns |     104 B |
| TryFindIntersections_Edge_WithoutIntersections    |  16.80 ns |  0.716 ns |  0.670 ns |         - |
| TryFindIntersections_Polygon_WithIntersections    | 123.70 ns |  3.052 ns |  2.705 ns |     104 B |
| TryFindIntersections_Polygon_WithoutIntersections |  73.28 ns |  2.788 ns |  2.471 ns |         - |
| HasIntersection_Edge                              |  11.75 ns |  0.417 ns |  0.390 ns |         - |
| HasIntersection_Polygon                           |  58.46 ns |  0.833 ns |  0.779 ns |         - |
| HasIntersection_EdgeArray                         |  60.12 ns |  1.856 ns |  1.737 ns |         - |

# Rect
| Method                      | Mean      | Error     | StdDev    | Median    | Allocated |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|
| Contains_Rect               | 0.0015 ns | 0.0061 ns | 0.0048 ns | 0.0000 ns |         - |
| Contains_IRect              | 2.0101 ns | 0.0522 ns | 0.0436 ns | 2.0102 ns |         - |
| Contains_Point              | 0.0025 ns | 0.0098 ns | 0.0091 ns | 0.0000 ns |         - |
| Contains_IntInt             | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns |         - |
| IsEquivalentTo_Rect         | 0.0021 ns | 0.0057 ns | 0.0044 ns | 0.0000 ns |         - |
| IsEquivalentTo_IRect        | 0.0204 ns | 0.0240 ns | 0.0225 ns | 0.0151 ns |         - |
| IsEquivalentTo_PointPoint   | 0.0936 ns | 0.0793 ns | 0.0742 ns | 0.1252 ns |         - |
| IsEquivalentTo_IntIntIntInt | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns |         - |

# SimpleQuadTreeNode
| Method             | Mean          | Error         | StdDev        | Allocated |
|------------------- |--------------:|--------------:|--------------:|----------:|
| Count              |  81,620.42 ns |  3,889.799 ns |  3,638.520 ns |       1 B |
| GetSubTreeContents | 641,767.72 ns | 24,038.926 ns | 21,309.882 ns |  788083 B |
| Insert             |      35.26 ns |      2.507 ns |      2.093 ns |     200 B |
| Query              |   4,064.63 ns |    437.792 ns |    409.511 ns |   23960 B |

# MidpointDisplacementNoisyEdgeFactory
| Method                     | Mean       | Error    | StdDev   | Allocated |
|--------------------------- |-----------:|---------:|---------:|----------:|
| Create_Amplitude05_Levels3 |   537.0 ns | 47.82 ns | 42.39 ns |     912 B |
| Create_Amplitude05_Levels4 | 1,145.9 ns | 51.28 ns | 47.96 ns |    1680 B |

# ConnectivityGrid
| Method             | Mean     | Error    | StdDev   | Allocated |
|------------------- |---------:|---------:|---------:|----------:|
| UpdateConnectivity | 21.00 us | 0.196 us | 0.174 us |         - |

# Point
| Method                  | Mean       | Error     | StdDev    | Median     | Allocated |
|------------------------ |-----------:|----------:|----------:|-----------:|----------:|
| Deconstruct             |  0.1096 ns | 0.0869 ns | 0.0813 ns |  0.0978 ns |         - |
| ToStringBenchmark       | 46.3868 ns | 2.2878 ns | 2.1401 ns | 46.9177 ns |      48 B |
| IsAdjacentTo_Orthogonal |  0.5488 ns | 0.0903 ns | 0.0845 ns |  0.5524 ns |         - |
| IsAdjacentTo_Diagonal   |  0.9635 ns | 0.0638 ns | 0.0566 ns |  0.9752 ns |         - |
| Add_Point               |  1.4746 ns | 0.0504 ns | 0.0447 ns |  1.4534 ns |         - |
| Add_IntInt              |  1.2883 ns | 0.0409 ns | 0.0383 ns |  1.2832 ns |         - |
| Subtract_Point          |  1.3646 ns | 0.0307 ns | 0.0256 ns |  1.3612 ns |         - |
| Subtract_IntInt         |  1.3573 ns | 0.0970 ns | 0.0908 ns |  1.3422 ns |         - |
| Negate                  |  1.3236 ns | 0.0269 ns | 0.0238 ns |  1.3276 ns |         - |
| Operator_Add            |  1.3531 ns | 0.1195 ns | 0.1118 ns |  1.3847 ns |         - |
| Operator_Subtract       |  1.2617 ns | 0.0500 ns | 0.0418 ns |  1.2652 ns |         - |
| Operator_Negate         |  1.2279 ns | 0.1194 ns | 0.1116 ns |  1.1988 ns |         - |
| ShiftRight              |  1.3625 ns | 0.0401 ns | 0.0355 ns |  1.3483 ns |         - |
| And                     |  1.3847 ns | 0.0422 ns | 0.0395 ns |  1.3813 ns |         - |
| Ctor                    |  0.0006 ns | 0.0024 ns | 0.0023 ns |  0.0000 ns |         - |
| ManhattanDistance       |  0.4899 ns | 0.0150 ns | 0.0133 ns |  0.4868 ns |         - |
| ChebyshevDistance       |  0.4988 ns | 0.0258 ns | 0.0228 ns |  0.4976 ns |         - |
| DistanceSquared         |  0.3741 ns | 0.2264 ns | 0.2007 ns |  0.2902 ns |         - |
| Operator_Equals         |  0.0000 ns | 0.0000 ns | 0.0000 ns |  0.0000 ns |         - |
| Operator_NotEquals      |  0.1936 ns | 0.0199 ns | 0.0177 ns |  0.1889 ns |         - |
| Equals_Object           |  0.0000 ns | 0.0000 ns | 0.0000 ns |  0.0000 ns |         - |
| Equals_IEquatable       |  0.0504 ns | 0.0256 ns | 0.0227 ns |  0.0437 ns |         - |
| GetHashCode_Single      |  0.0134 ns | 0.0280 ns | 0.0248 ns |  0.0000 ns |         - |
| HashSet_Contains        |  5.4885 ns | 0.3488 ns | 0.3263 ns |  5.3107 ns |         - |
| Dictionary_TryGetValue  |  5.8597 ns | 0.1136 ns | 0.0949 ns |  5.8475 ns |         - |
| HashSet_Build           |  7.7450 ns | 0.1338 ns | 0.1252 ns |  7.7221 ns |      26 B |
| Min                     |  1.9207 ns | 0.0632 ns | 0.0591 ns |  1.9105 ns |         - |
| Max                     |  1.5877 ns | 0.0475 ns | 0.0421 ns |  1.5728 ns |         - |
