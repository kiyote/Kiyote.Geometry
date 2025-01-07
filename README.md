![CI](https://github.com/kiyote/Kiyote.Geometry/actions/workflows/ci.yml/badge.svg?branch=main)
![coverage](https://github.com/kiyote/Kiyote.Geometry/blob/badges/.badges/main/coverage.svg?raw=true)

```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5247/22H2/2022Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 8.0.404
  [Host] : .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
```

# Polygon
| Method              | Mean          | Error        | StdDev       | Allocated |
|-------------------- |--------------:|-------------:|-------------:|----------:|
| TryFindIntersection |     949.39 ns |     7.404 ns |     6.563 ns |    1712 B |
| Contains            |      44.16 ns |     1.713 ns |     1.602 ns |         - |
| Contains_1000x1000  | 796,306.07 ns | 9,612.120 ns | 8,520.894 ns |       1 B |
| Intersections       |     208.09 ns |     5.550 ns |     5.192 ns |      88 B |

# FastRandom Benchmarks
| Method                                    | Mean      | Error     | StdDev    | Allocated |
|------------------------------------------ |----------:|----------:|----------:|----------:|
| FastRandom_NextInt                        |  1.842 ns | 0.4487 ns | 0.4197 ns |         - |
| FastRandom_NextInt_UpperBound             |  2.933 ns | 0.0324 ns | 0.0303 ns |         - |
| FastRandom_NextInt_LowerBoundUpperBound   |  3.714 ns | 0.0464 ns | 0.0388 ns |         - |
| FastRandom_NextBool                       |  1.897 ns | 0.0094 ns | 0.0073 ns |         - |
| FastRandom_NextUInt                       |  1.340 ns | 0.0049 ns | 0.0046 ns |         - |
| FastRandom_NextDouble                     |  1.511 ns | 0.0187 ns | 0.0156 ns |         - |
| FastRandom_NextFloat                      |  2.416 ns | 0.0159 ns | 0.0133 ns |         - |
| FastRandom_NextFloat_LowerBoundUpperBound |  3.093 ns | 0.0085 ns | 0.0066 ns |         - |
| FastRandom_NextBytes                      | 66.925 ns | 0.5067 ns | 0.4492 ns |         - |

# FastRandom Benchmarks vs System.Random
| Method                  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| FastRandom_NextBytes    | 57.411 ns | 1.3711 ns | 1.2155 ns |  3.29 |    0.16 |         - |          NA |
| SystemRandom_NextBytes  | 17.486 ns | 0.8653 ns | 0.8094 ns |  1.00 |    0.06 |         - |          NA |
| FastRandom_NextDouble   |  1.909 ns | 0.1365 ns | 0.1277 ns |  1.00 |    0.06 |         - |          NA |
| SystemRandom_NextDouble |  1.909 ns | 0.0107 ns | 0.0089 ns |  1.00 |    0.01 |         - |          NA |
| FastRandom_NextFloat    |  1.811 ns | 0.0529 ns | 0.0469 ns |  0.82 |    0.02 |         - |          NA |
| SystemRandom_NextFloat  |  2.197 ns | 0.0091 ns | 0.0071 ns |  1.00 |    0.00 |         - |          NA |
| FastRandom_NextInt      |  1.275 ns | 0.0108 ns | 0.0101 ns |  0.59 |    0.00 |         - |          NA |
| SystemRandom_NextInt    |  2.145 ns | 0.0065 ns | 0.0061 ns |  1.00 |    0.00 |         - |          NA |

# FastPoissonDiscPointFactory Benchmarks
<sub>
The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randomizing points and constructing a list of them.
The surface is filled with a separation of 5.
</sub>

| Method              | Mean          | Error      | StdDev     | Ratio | RatioSD | Allocated  | Alloc Ratio |
|-------------------- |--------------:|-----------:|-----------:|------:|--------:|-----------:|------------:|
| Fill_100x100        |    136.684 us |  0.9002 us |  0.8421 us | 77.79 |    0.63 |   15.95 KB |        1.94 |
| Fill_100x100_Base   |      1.757 us |  0.0113 us |  0.0100 us |  1.00 |    0.01 |    8.21 KB |        1.00 |
| Fill_500x500        |  3,508.666 us | 29.2334 us | 27.3449 us | 98.69 |    0.93 |  293.35 KB |        2.29 |
| Fill_500x500_Base   |     35.552 us |  0.2458 us |  0.2053 us |  1.00 |    0.01 |   128.3 KB |        1.00 |
| Fill_1000x1000      | 14,250.438 us | 95.6436 us | 89.4650 us | 53.21 |    0.48 | 1154.39 KB |        2.25 |
| Fill_1000x1000_Base |    267.810 us |  2.3522 us |  1.8365 us |  1.00 |    0.01 |  512.43 KB |        1.00 |

All of the following benchmarks fill a surface of the specified size with a poisson disc set of random values with a separation of 5.

# D3DelaunayFactory Benchmarks
| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     66.84 us |   0.670 us |   0.627 us |   73.06 KB |
| Create_500x500   |  3,520.56 us |  36.546 us |  34.185 us | 1790.98 KB |
| Create_1000x1000 | 14,597.12 us | 138.340 us | 129.403 us | 7176.54 KB |


# D3VoronoiFactory Benchmarks
| Method           | Mean        | Error       | StdDev      | Allocated   |
|----------------- |------------:|------------:|------------:|------------:|
| Create_100x100   |    297.0 us |     1.81 us |     1.42 us |   327.42 KB |
| Create_500x500   | 11,444.7 us |   170.96 us |   159.92 us |  9633.56 KB |
| Create_1000x1000 | 59,514.2 us | 1,877.80 us | 1,664.62 us | 39851.02 KB |

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

