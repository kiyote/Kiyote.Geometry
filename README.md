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
| Method              | Mean      | Error     | StdDev    | Allocated |
|-------------------- |----------:|----------:|----------:|----------:|
| HasIntersection     |  6.946 ns | 0.2079 ns | 0.1736 ns |         - |
| TryFindIntersection | 13.635 ns | 0.4927 ns | 0.4608 ns |      24 B |

# Polygon
| Method               | Mean           | Error         | StdDev        | Allocated |
|--------------------- |---------------:|--------------:|--------------:|----------:|
| TryFindOverlap       |   1,242.683 ns |    12.8698 ns |    11.4087 ns |    2232 B |
| Contains             |      26.580 ns |     0.3306 ns |     0.3092 ns |         - |
| Contains_1000x1000   | 865,219.182 ns | 3,705.4233 ns | 3,284.7613 ns |       1 B |
| Intersections        |     173.365 ns |     0.9958 ns |     0.8316 ns |     216 B |
| HasIntersection      |       9.443 ns |     0.2039 ns |     0.1808 ns |         - |
| TryFindIntersections |      82.983 ns |     0.9793 ns |     0.9161 ns |     216 B |

# FastRandom Benchmarks
| Method                                    | Mean      | Error     | StdDev    | Allocated |
|------------------------------------------ |----------:|----------:|----------:|----------:|
| FastRandom_NextInt                        |  1.038 ns | 0.0147 ns | 0.0130 ns |         - |
| FastRandom_NextInt_UpperBound             |  2.428 ns | 0.0479 ns | 0.0424 ns |         - |
| FastRandom_NextInt_LowerBoundUpperBound   |  3.184 ns | 0.0575 ns | 0.0538 ns |         - |
| FastRandom_NextBool                       |  1.876 ns | 0.0778 ns | 0.0690 ns |         - |
| FastRandom_NextUInt                       |  1.350 ns | 0.0304 ns | 0.0269 ns |         - |
| FastRandom_NextDouble                     |  1.837 ns | 0.0312 ns | 0.0261 ns |         - |
| FastRandom_NextFloat                      |  1.791 ns | 0.0564 ns | 0.0527 ns |         - |
| FastRandom_NextFloat_LowerBoundUpperBound |  2.778 ns | 0.0340 ns | 0.0301 ns |         - |
| FastRandom_NextBytes                      | 59.486 ns | 1.1592 ns | 0.9680 ns |         - |

# FastRandom Benchmarks vs System.Random
| Method                  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| FastRandom_NextBytes    | 57.842 ns | 0.3804 ns | 0.3558 ns |  3.27 |    0.04 |         - |          NA |
| SystemRandom_NextBytes  | 17.692 ns | 0.1999 ns | 0.1772 ns |  1.00 |    0.01 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextDouble   |  1.510 ns | 0.0233 ns | 0.0195 ns |  0.84 |    0.01 |         - |          NA |
| SystemRandom_NextDouble |  1.801 ns | 0.0186 ns | 0.0145 ns |  1.00 |    0.01 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextFloat    |  1.768 ns | 0.0268 ns | 0.0237 ns |  0.68 |    0.03 |         - |          NA |
| SystemRandom_NextFloat  |  2.614 ns | 0.1443 ns | 0.1280 ns |  1.00 |    0.07 |         - |          NA |
|                         |           |           |           |       |         |           |             |
| FastRandom_NextInt      |  1.155 ns | 0.0194 ns | 0.0162 ns |  0.59 |    0.02 |         - |          NA |
| SystemRandom_NextInt    |  1.947 ns | 0.0502 ns | 0.0445 ns |  1.00 |    0.03 |         - |          NA |

# FastPoissonDiscPointFactory Benchmarks
<sub>
The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randomizing points and constructing a list of them.
The surface is filled with a separation of 5.
</sub>

| Method              | Mean          | Error       | StdDev      | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------------- |--------------:|------------:|------------:|------:|--------:|-----------:|------------:|
| Fill_100x100        |    146.046 μs |   1.8147 μs |   1.5153 μs | 44.19 |    0.87 |   22.38 KB |        1.54 |
| Fill_100x100_Base   |      3.306 μs |   0.0703 μs |   0.0587 μs |  1.00 |    0.02 |   14.54 KB |        1.00 |
|                     |               |             |             |       |         |            |             |
| Fill_500x500        |  3,724.046 μs |  26.4510 μs |  22.0878 μs | 51.12 |    1.61 |  449.73 KB |        1.62 |
| Fill_500x500_Base   |     72.917 μs |   2.5803 μs |   2.4136 μs |  1.00 |    0.04 |  277.13 KB |        1.00 |
|                     |               |             |             |       |         |            |             |
| Fill_1000x1000      | 15,920.582 μs | 535.7817 μs | 474.9566 μs | 30.05 |    0.97 | 1777.32 KB |        1.61 |
| Fill_1000x1000_Base |    529.963 μs |   9.0660 μs |   8.0368 μs |  1.00 |    0.02 | 1105.44 KB |        1.00 |

All of the following benchmarks fill a surface of the specified size with a poisson disc set of random values with a separation of 5.

# MapboxDelaunatorFactory Benchmarks
| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     60.32 us |   0.193 us |   0.181 us |   47.63 KB |
| Create_500x500   |  2,893.19 us |  13.911 us |  10.861 us | 1168.35 KB |
| Create_1000x1000 | 14,129.65 us | 107.994 us | 101.018 us | 4681.92 KB |

# D3DelaunayFactory Benchmarks
| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     79.93 μs |   3.766 μs |   3.339 μs |   89.98 KB |
| Create_500x500   |  3,566.07 μs |  38.761 μs |  34.360 μs |  2252.2 KB |
| Create_1000x1000 | 18,431.21 μs | 510.076 μs | 477.125 μs | 9035.57 KB |


# D3VoronoiFactory Benchmarks
| Method           | Mean         | Error       | StdDev      | Allocated   |
|----------------- |-------------:|------------:|------------:|------------:|
| Create_100x100   |     392.6 μs |     5.46 μs |     4.84 μs |   436.54 KB |
| Create_500x500   |  21,884.3 μs | 1,642.02 μs | 1,535.95 μs | 11933.25 KB |
| Create_1000x1000 | 109,610.7 μs | 3,384.38 μs | 3,000.16 μs | 48706.67 KB |

# SimpleQuadTreeNode Benchmarks
| Method | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------- |---------:|----------:|----------:|-------:|----------:|
| Query  | 5.531 μs | 0.0734 μs | 0.0613 μs | 0.2136 |   1.34 KB |

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

