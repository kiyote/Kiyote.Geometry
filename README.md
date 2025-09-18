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
| HasIntersection     |  6.643 ns | 0.3820 ns | 0.3573 ns |         - |
| TryFindIntersection | 10.836 ns | 0.4186 ns | 0.3916 ns |         - |

# Polygon
| Method                   | Mean        | Error     | StdDev    | Allocated |
|------------------------- |------------:|----------:|----------:|----------:|
| TryIntersect             | 1,447.16 ns | 31.768 ns | 29.716 ns |    2368 B |
| Contains                 |    33.43 ns |  0.509 ns |  0.477 ns |         - |
| Intersections            |   204.16 ns |  2.812 ns |  2.492 ns |     104 B |
| IsEquivalentTo           |   524.55 ns | 20.538 ns | 19.211 ns |    1104 B |
| HasOverlap               |   282.13 ns |  4.899 ns |  4.343 ns |     232 B |
| TryFindIntersections     |   106.76 ns |  2.308 ns |  2.159 ns |     104 B |
| HasIntersection_Edge     |    11.60 ns |  0.219 ns |  0.205 ns |         - |
| HasIntersection_Polygon  |    90.56 ns |  1.908 ns |  1.784 ns |      40 B |
| HasIntersection_EdgeList |    81.57 ns |  1.662 ns |  1.473 ns |      40 B |

# Rect
| Method                      | Mean      | Error     | StdDev    | Allocated |
|---------------------------- |----------:|----------:|----------:|----------:|
| Contains_Rect               | 1.6438 ns | 0.1303 ns | 0.1219 ns |         - |
| Contains_IRect              | 3.7002 ns | 0.1800 ns | 0.1684 ns |         - |
| Contains_Point              | 1.8831 ns | 0.1552 ns | 0.1376 ns |         - |
| Contains_IntInt             | 0.0671 ns | 0.0657 ns | 0.0615 ns |         - |
| IsEquivalentTo_Rect         | 4.1632 ns | 0.1818 ns | 0.1701 ns |         - |
| IsEquivalentTo_IRect        | 4.7800 ns | 0.3425 ns | 0.3203 ns |         - |
| IsEquivalentTo_PointPoint   | 3.7967 ns | 0.1910 ns | 0.1786 ns |         - |
| IsEquivalentTo_IntIntIntInt | 4.1109 ns | 0.1834 ns | 0.1715 ns |         - |

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
<sub>
The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randomizing points and constructing a list of them.
The surface is filled with a separation of 5.
</sub>

| Method              | Mean          | Error       | StdDev      | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------------- |--------------:|------------:|------------:|------:|--------:|-----------:|------------:|
| Fill_1000x1000      | 16,163.716 μs | 674.5125 μs | 630.9394 μs | 27.37 |    2.07 | 1410.69 KB |        1.84 |
| Fill_1000x1000_Base |    593.162 μs |  42.6133 μs |  39.8605 μs |  1.00 |    0.09 |  768.53 KB |        1.00 |
|                     |               |             |             |       |         |            |             |
| Fill_100x100        |    152.325 μs |   5.8146 μs |   5.4390 μs | 44.82 |    3.24 |   19.94 KB |        1.63 |
| Fill_100x100_Base   |      3.412 μs |   0.2379 μs |   0.2225 μs |  1.00 |    0.09 |    12.2 KB |        1.00 |
|                     |               |             |             |       |         |            |             |
| Fill_500x500        |  3,909.891 μs | 183.3170 μs | 171.4748 μs | 33.75 |    2.37 |  357.18 KB |        1.86 |
| Fill_500x500_Base   |    116.192 μs |   6.9466 μs |   6.4978 μs |  1.00 |    0.08 |  192.33 KB |        1.00 |

All of the following benchmarks fill a surface of the specified size with a poisson disc set of random values with a separation of 5.

# MapboxDelaunatorFactory Benchmarks
| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     60.32 us |   0.193 us |   0.181 us |   47.63 KB |
| Create_500x500   |  2,893.19 us |  13.911 us |  10.861 us | 1168.35 KB |
| Create_1000x1000 | 14,129.65 us | 107.994 us | 101.018 us | 4681.92 KB |

# D3DelaunayFactory Benchmarks
| Method           | Mean         | Error      | StdDev     | Allocated   |
|----------------- |-------------:|-----------:|-----------:|------------:|
| Create_100x100   |     81.46 μs |   3.924 μs |   3.671 μs |   102.25 KB |
| Create_500x500   |  3,866.33 μs |  93.106 μs |  87.092 μs |  2562.34 KB |
| Create_1000x1000 | 17,588.62 μs | 303.054 μs | 268.649 μs | 10281.43 KB |


# D3VoronoiFactory Benchmarks
| Method           | Mean         | Error       | StdDev      | Allocated   |
|----------------- |-------------:|------------:|------------:|------------:|
| Create_100x100   |     391.3 μs |    19.21 μs |    17.97 μs |   433.41 KB |
| Create_500x500   |  22,846.6 μs | 1,438.23 μs | 1,345.32 μs | 11854.63 KB |
| Create_1000x1000 | 121,492.1 μs | 7,361.32 μs | 6,885.78 μs | 48395.72 KB |

# SimpleQuadTreeNode Benchmarks
| Method | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------- |---------:|----------:|----------:|-------:|----------:|
| Query  | 6.245 μs | 0.2259 μs | 0.2113 μs | 0.2136 |   1.34 KB |

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

