![CI](https://github.com/kiyote/Kiyote.Geometry/actions/workflows/ci.yml/badge.svg?branch=main)
![coverage](https://github.com/kiyote/Kiyote.Geometry/blob/badges/.badges/main/coverage.svg?raw=true)

```

BenchmarkDotNet v0.15.2, Windows 10 (10.0.19045.6093/22H2/2022Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.302
  [Host] : .NET 8.0.18 (8.0.1825.31117), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessNoEmitToolchain  IterationCount=15  
LaunchCount=1  WarmupCount=10  

```
# Edge
| Method              | Mean       | Error     | StdDev    | Median     | Allocated |
|-------------------- |-----------:|----------:|----------:|-----------:|----------:|
| HasIntersection     |  7.1543 ns | 0.3235 ns | 0.3026 ns |  7.0201 ns |         - |
| TryFindIntersection | 10.5231 ns | 0.2849 ns | 0.2665 ns | 10.4053 ns |         - |
| GetBoundingBox      |  1.9560 ns | 0.1252 ns | 0.1171 ns |  1.9322 ns |         - |
| GetMidpoint         |  7.5447 ns | 0.1799 ns | 0.1683 ns |  7.5094 ns |         - |
| Ctor_PointPoint     |  0.0036 ns | 0.0110 ns | 0.0103 ns |  0.0000 ns |         - |
| Ctor_IntIntIntInt   |  3.7167 ns | 0.1899 ns | 0.1776 ns |  3.6965 ns |         - |


# Polygon
| Method                                            | Mean        | Error     | StdDev    | Allocated |
|-------------------------------------------------- |------------:|----------:|----------:|----------:|
| Ctor                                              |    70.55 ns |  3.506 ns |  3.108 ns |     272 B |
| Contains_Point                                    |    55.12 ns |  1.625 ns |  1.520 ns |         - |
| Contains_Polygon                                  |    58.23 ns |  1.894 ns |  1.772 ns |         - |
| HasOverlap                                        |    56.95 ns |  0.899 ns |  0.840 ns |         - |
| IsEquivalentTo                                    |    53.43 ns |  1.948 ns |  1.822 ns |         - |
| TryIntersect_WithIntersection                     | 1,075.64 ns | 30.766 ns | 27.274 ns |    1304 B |
| TryIntersect_WithoutIntersection                  |    94.47 ns |  5.909 ns |  5.527 ns |      24 B |
| TryFindIntersections_Edge_WithIntersections       |    72.85 ns |  3.318 ns |  3.103 ns |     104 B |
| TryFindIntersections_Edge_WithoutIntersections    |    17.93 ns |  0.706 ns |  0.660 ns |         - |
| TryFindIntersections_Polygon_WithIntersections    |   137.99 ns |  7.343 ns |  6.869 ns |     104 B |
| TryFindIntersections_Polygon_WithoutIntersections |    81.33 ns |  2.354 ns |  2.202 ns |         - |
| HasIntersection_Edge                              |    12.73 ns |  0.273 ns |  0.255 ns |         - |
| HasIntersection_Polygon                           |    70.50 ns |  1.579 ns |  1.477 ns |         - |
| HasIntersection_EdgeArray                         |    72.31 ns |  2.843 ns |  2.659 ns |         - |

# Rect
| Method                      | Mean      | Error     | StdDev    | Median    | Allocated |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|
| Contains_Rect               | 1.6484 ns | 0.0895 ns | 0.0793 ns | 1.6516 ns |         - |
| Contains_IRect              | 2.8592 ns | 0.1698 ns | 0.1588 ns | 2.8406 ns |         - |
| Contains_Point              | 1.2285 ns | 0.0617 ns | 0.0547 ns | 1.2276 ns |         - |
| Contains_IntInt             | 0.0006 ns | 0.0025 ns | 0.0023 ns | 0.0000 ns |         - |
| IsEquivalentTo_Rect         | 3.5132 ns | 0.1529 ns | 0.1430 ns | 3.5039 ns |         - |
| IsEquivalentTo_IRect        | 4.2392 ns | 0.1280 ns | 0.1135 ns | 4.2351 ns |         - |
| IsEquivalentTo_PointPoint   | 3.6127 ns | 0.0731 ns | 0.0610 ns | 3.6149 ns |         - |
| IsEquivalentTo_IntIntIntInt | 3.5041 ns | 0.1253 ns | 0.1172 ns | 3.4988 ns |         - |

# FastRandom Benchmarks
| Method                                    | Mean      | Error     | StdDev    | Allocated |
|------------------------------------------ |----------:|----------:|----------:|----------:|
| FastRandom_NextInt                        |  1.864 ns | 0.1373 ns | 0.1284 ns |         - |
| FastRandom_NextInt_UpperBound             |  3.160 ns | 0.1749 ns | 0.1636 ns |         - |
| FastRandom_NextInt_LowerBoundUpperBound   |  3.259 ns | 0.1680 ns | 0.1489 ns |         - |
| FastRandom_NextBool                       |  2.587 ns | 0.1357 ns | 0.1269 ns |         - |
| FastRandom_NextUInt                       |  1.877 ns | 0.1534 ns | 0.1435 ns |         - |
| FastRandom_NextDouble                     |  2.289 ns | 0.1676 ns | 0.1568 ns |         - |
| FastRandom_NextFloat                      |  2.362 ns | 0.1545 ns | 0.1445 ns |         - |
| FastRandom_NextFloat_LowerBoundUpperBound |  3.122 ns | 0.1856 ns | 0.1736 ns |         - |
| FastRandom_NextBytes                      | 60.006 ns | 2.1685 ns | 2.0284 ns |         - |

# FastRandom Benchmarks vs System.Random
| Method                  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| FastRandom_NextBytes    | 59.430 ns | 2.5622 ns | 2.3967 ns |  3.30 |    0.16 |         - |          NA |
| SystemRandom_NextBytes  | 18.018 ns | 0.5967 ns | 0.5290 ns |  1.00 |    0.04 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextDouble   |  2.078 ns | 0.1822 ns | 0.1704 ns |  0.71 |    0.07 |         - |          NA |
| SystemRandom_NextDouble |  2.938 ns | 0.1731 ns | 0.1619 ns |  1.00 |    0.08 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextFloat    |  2.174 ns | 0.1610 ns | 0.1506 ns |  0.97 |    0.09 |         - |          NA |
| SystemRandom_NextFloat  |  2.247 ns | 0.1735 ns | 0.1623 ns |  1.00 |    0.10 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextInt      |  1.360 ns | 0.1371 ns | 0.1282 ns |  0.71 |    0.08 |         - |          NA |
| SystemRandom_NextInt    |  1.933 ns | 0.1500 ns | 0.1404 ns |  1.00 |    0.10 |         - |          NA |

# FastPoissonDiscPointFactory Benchmarks
| Method         | Mean        | Error     | StdDev    | Allocated  |
|--------------- |------------:|----------:|----------:|-----------:|
| Fill_100x100   |    139.9 us |   1.30 us |   1.15 us |   19.92 KB |
| Fill_500x500   |  3,742.5 us | 164.07 us | 153.47 us |   358.1 KB |
| Fill_1000x1000 | 15,474.4 us | 740.32 us | 692.50 us | 1410.53 KB |

# Intersect
| Method              | Mean     | Error     | StdDev    | Allocated |
|-------------------- |---------:|----------:|----------:|----------:|
| HasIntersection     | 4.856 ns | 0.1851 ns | 0.1732 ns |         - |
| TryFindIntersection | 7.325 ns | 0.1383 ns | 0.1294 ns |         - |

# MapboxDelaunatorFactory Benchmarks
| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     63.58 us |   1.393 us |   1.163 us |   47.63 KB |
| Create_500x500   |  2,984.96 us | 111.314 us | 104.123 us | 1168.34 KB |
| Create_1000x1000 | 14,114.88 us | 459.767 us | 430.066 us |  4681.6 KB |

# D3DelaunayFactory Benchmarks
| Method           | Mean         | Error        | StdDev       | Allocated  |
|----------------- |-------------:|-------------:|-------------:|-----------:|
| Create_100x100   |     85.49 us |     4.978 us |     4.656 us |  102.25 KB |
| Create_500x500   |  3,801.30 us |   119.785 us |   112.047 us | 2562.28 KB |
| Create_1000x1000 | 17,327.80 us | 1,101.025 us | 1,029.899 us | 10281.1 KB |

# D3VoronoiFactory Benchmarks
| Method           | Mean         | Error       | StdDev      | Allocated   |
|----------------- |-------------:|------------:|------------:|------------:|
| Create_100x100   |     352.9 us |    17.23 us |    16.12 us |   471.41 KB |
| Create_500x500   |  21,065.9 us | 1,583.75 us | 1,481.44 us | 13657.32 KB |
| Create_1000x1000 | 102,617.1 us | 8,399.49 us | 7,445.93 us | 56306.48 KB |

# SimpleQuadTreeNode Benchmarks
| Method             | Mean          | Error         | StdDev        | Allocated |
|------------------- |--------------:|--------------:|--------------:|----------:|
| Count              |  81,048.03 ns |  5,601.581 ns |  5,239.722 ns |         - |
| GetSubTreeContents | 803,318.79 ns | 79,809.932 ns | 74,654.260 ns |  786805 B |
| Insert             |      44.91 ns |      2.815 ns |      2.496 ns |     200 B |
| Query              |   7,123.67 ns |    555.542 ns |    519.655 ns |   24408 B |

# Notes

Github action failing with permission denied?
```
git update-index --chmod=+x ./create-orphan-branch.sh
```

Still failing?  Try running the `./create-orphan-branch.sh` locally once.


Coverage badge failing to be pushed?

Give the workflow the `contents: write` permission.


Coverage showing as 0%

Add the `coverlet.msbuild` package to your test project.

