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

# DenseGridSource
| Method                              | Size | Mean         | Error       | StdDev      | Allocated |
|------------------------------------ |----- |-------------:|------------:|------------:|----------:|
| GetOccupiedRuns_Cached              | 64   |     120.8 ns |     4.46 ns |     4.17 ns |         - |
| TryClearCell_TrySetCell_RebuildRuns | 64   |     126.0 ns |     2.92 ns |     2.44 ns |         - |
| IsOccupied_Scan                     | 64   |   6,502.5 ns |    89.21 ns |    79.08 ns |         - |
| GetCell_Scan                        | 64   |   3,872.5 ns |   190.15 ns |   177.86 ns |         - |
| TryGetRow_Scan                      | 64   |   3,640.5 ns |    71.95 ns |    67.30 ns |         - |
| GetOccupiedRuns_Cached              | 256  |     467.9 ns |    17.60 ns |    15.60 ns |         - |
| TryClearCell_TrySetCell_RebuildRuns | 256  |     389.5 ns |     6.04 ns |     5.65 ns |         - |
| IsOccupied_Scan                     | 256  | 105,981.7 ns | 4,297.04 ns | 4,019.46 ns |       1 B |
| GetCell_Scan                        | 256  |  61,541.0 ns |   728.09 ns |   681.06 ns |       1 B |
| TryGetRow_Scan                      | 256  |  58,758.7 ns |   728.46 ns |   681.40 ns |         - |

# GridAssembly
| Method                            | Size | Mean           | Error         | StdDev        | Allocated |
|---------------------------------- |----- |---------------:|--------------:|--------------:|----------:|
| TryAttach_TryDetach               | 64   |   289,244.4 ns |   9,781.46 ns |   9,149.58 ns |   32176 B |
| TryGetPlacementAt_Scan            | 64   |   114,323.8 ns |     662.53 ns |     619.73 ns |         - |
| TryRemoveCell_TryAddCell_Interior | 64   |       500.2 ns |      18.41 ns |      17.22 ns |         - |
| TryRemoveCell_TryAddCell_Seam     | 64   |       663.2 ns |      18.02 ns |      16.85 ns |         - |
| TryAttach_TryDetach               | 256  | 4,503,297.9 ns | 134,619.33 ns | 125,923.00 ns |  330458 B |
| TryGetPlacementAt_Scan            | 256  | 1,824,951.7 ns |  27,028.67 ns |  25,282.64 ns |       8 B |
| TryRemoveCell_TryAddCell_Interior | 256  |     1,867.5 ns |      42.44 ns |      37.62 ns |         - |
| TryRemoveCell_TryAddCell_Seam     | 256  |     2,301.6 ns |      53.52 ns |      50.06 ns |         - |

# GridCompiler
| Method  | Size | ChunkSize | Mean      | Error    | StdDev   | Allocated |
|-------- |----- |---------- |----------:|---------:|---------:|----------:|
| Compile | 64   | 32        |  40.20 us | 1.035 us | 0.968 us |  73.14 KB |
| Compile | 64   | 64        |  29.09 us | 0.727 us | 0.680 us |  56.35 KB |
| Compile | 256  | 32        | 454.78 us | 8.190 us | 7.661 us | 676.56 KB |
| Compile | 256  | 64        | 240.13 us | 5.817 us | 4.857 us | 401.46 KB |

# CompiledGridAssembly
| Method                              | Size | Mean       | Error     | StdDev    | Allocated |
|------------------------------------ |----- |-----------:|----------:|----------:|----------:|
| Commit_AllChunksDirty               | 64   |  12.523 us | 0.2478 us | 0.2318 us |         - |
| Commit_OneChunkDirty                | 64   |   2.418 us | 0.0675 us | 0.0598 us |         - |
| Commit_NothingDirty                 | 64   |   1.166 us | 0.0348 us | 0.0326 us |         - |
| TryRemoveCell_TryAddCell_Propagated | 64   |   2.361 us | 0.0913 us | 0.0854 us |         - |
| Commit_AllChunksDirty               | 256  | 208.686 us | 8.2232 us | 7.6920 us |       1 B |
| Commit_OneChunkDirty                | 256  |  25.348 us | 1.1110 us | 0.9848 us |         - |
| Commit_NothingDirty                 | 256  |  18.799 us | 0.5894 us | 0.5514 us |         - |
| TryRemoveCell_TryAddCell_Propagated | 256  |  19.272 us | 0.5282 us | 0.4941 us |         - |

# CompiledGridAssemblyMutation
| Method                    | Size | Mean         | Error      | StdDev     | Allocated  |
|-------------------------- |----- |-------------:|-----------:|-----------:|-----------:|
| Bind                      | 64   |     26.33 us |   8.885 us |   7.419 us |   70.26 KB |
| ConnectivityBuilder_Build | 64   |  1,046.50 us | 293.923 us | 229.475 us |   22.91 KB |
| CreateVacuumLayer         | 64   |    207.86 us |  24.886 us |  22.061 us |   22.91 KB |
| Bind                      | 256  |    411.26 us | 131.927 us | 123.404 us | 1039.35 KB |
| ConnectivityBuilder_Build | 256  | 14,813.24 us | 555.487 us | 463.857 us |  271.34 KB |
| CreateVacuumLayer         | 256  |  1,811.41 us | 196.313 us | 174.026 us |  271.34 KB |

# ConnectivityBuilder
| Method            | Size | Mean        | Error       | StdDev      | Allocated |
|------------------ |----- |------------:|------------:|------------:|----------:|
| Update_SingleCell | 64   |    901.7 ns |    14.85 ns |    12.40 ns |         - |
| Update_SeamCell   | 64   |  1,050.3 ns |    29.58 ns |    27.67 ns |         - |
| Update_32x32      | 64   | 80,313.1 ns | 2,070.46 ns | 1,936.71 ns |       1 B |
| Update_SingleCell | 256  |  1,317.7 ns |    34.56 ns |    32.32 ns |         - |
| Update_SeamCell   | 256  |  1,401.8 ns |    36.85 ns |    34.47 ns |         - |
| Update_32x32      | 256  | 86,901.3 ns | 2,573.30 ns | 2,407.06 ns |       1 B |

# DirectionExtensions
| Method            | Mean     | Error     | StdDev    | Allocated |
|------------------ |---------:|----------:|----------:|----------:|
| ToOffset          | 2.661 ns | 0.0543 ns | 0.0453 ns |         - |
| Opposite          | 2.514 ns | 0.1178 ns | 0.1044 ns |         - |
| TryGetOrthogonals | 1.379 ns | 0.0140 ns | 0.0117 ns |         - |
