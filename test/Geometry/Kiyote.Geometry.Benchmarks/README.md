
# Edge
|       Method |     Mean |    Error |   StdDev | Allocated |
|------------- |---------:|---------:|---------:|----------:|
| Intersection | 31.69 ns | 0.571 ns | 0.506 ns |      24 B |

# Polygon
|             Method |           Mean |        Error |       StdDev | Allocated |
|------------------- |---------------:|-------------:|-------------:|----------:|
|               Clip |     2,369.0 ns |     25.15 ns |     22.29 ns |    1808 B |
|           Contains |       148.1 ns |      1.57 ns |      1.40 ns |         - |
| Contains_1000x1000 | 3,055,203.8 ns | 39,404.41 ns | 32,904.46 ns |       2 B |
|      Intersections |       522.4 ns |     10.47 ns |     11.21 ns |     184 B |

# FastRandom Benchmarks vs System.Random
|                  Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
|    FastRandom_NextBytes | 56.185 ns | 0.3779 ns | 0.3535 ns |  3.11 |    0.02 |         - |          NA |
|  SystemRandom_NextBytes | 18.065 ns | 0.1179 ns | 0.1103 ns |  1.00 |    0.00 |         - |          NA |
|   FastRandom_NextDouble |  1.783 ns | 0.0075 ns | 0.0070 ns |  0.69 |    0.01 |         - |          NA |
| SystemRandom_NextDouble |  2.586 ns | 0.0293 ns | 0.0274 ns |  1.00 |    0.00 |         - |          NA |
|    FastRandom_NextFloat |  2.565 ns | 0.0256 ns | 0.0227 ns |  0.95 |    0.01 |         - |          NA |
|  SystemRandom_NextFloat |  2.698 ns | 0.0204 ns | 0.0190 ns |  1.00 |    0.00 |         - |          NA |
|      FastRandom_NextInt |  1.540 ns | 0.0182 ns | 0.0170 ns |  0.68 |    0.01 |         - |          NA |
|    SystemRandom_NextInt |  2.268 ns | 0.0140 ns | 0.0131 ns |  1.00 |    0.00 |         - |          NA |

# FastRandom Benchmarks
|                                    Method |      Mean |     Error |    StdDev | Allocated |
|------------------------------------------ |----------:|----------:|----------:|----------:|
|                        FastRandom_NextInt |  1.556 ns | 0.0188 ns | 0.0167 ns |         - |
|             FastRandom_NextInt_UpperBound |  2.417 ns | 0.0191 ns | 0.0169 ns |         - |
|   FastRandom_NextInt_LowerBoundUpperBound |  2.680 ns | 0.0086 ns | 0.0076 ns |         - |
|                       FastRandom_NextByte |  1.615 ns | 0.0112 ns | 0.0100 ns |         - |
|                       FastRandom_NextBool |  1.938 ns | 0.0076 ns | 0.0072 ns |         - |
|                       FastRandom_NextUInt |  1.751 ns | 0.0099 ns | 0.0088 ns |         - |
|                     FastRandom_NextDouble |  1.772 ns | 0.0104 ns | 0.0097 ns |         - |
|                      FastRandom_NextFloat |  1.820 ns | 0.0174 ns | 0.0136 ns |         - |
| FastRandom_NextFloat_LowerBoundUpperBound |  2.977 ns | 0.0304 ns | 0.0284 ns |         - |
|                      FastRandom_NextBytes | 55.924 ns | 0.2913 ns | 0.2725 ns |         - |

# PoissonDiscPointFactory Benchmarks
|              Method |          Mean |       Error |      StdDev |  Ratio | RatioSD |  Allocated | Alloc Ratio |
|-------------------- |--------------:|------------:|------------:|-------:|--------:|-----------:|------------:|
|        Fill_100x100 |    776.223 us |   6.9070 us |   6.1229 us | 200.95 |    4.55 |   22.14 KB |        1.52 |
|   Fill_100x100_Base |      3.848 us |   0.0612 us |   0.0704 us |   1.00 |    0.00 |   14.54 KB |        1.00 |
|        Fill_500x500 | 19,185.253 us |  90.9911 us |  75.9817 us | 221.28 |    1.91 |  424.93 KB |        1.53 |
|   Fill_500x500_Base |     86.649 us |   0.7026 us |   0.6572 us |   1.00 |    0.00 |  277.13 KB |        1.00 |
|      Fill_1000x1000 | 78,901.840 us | 896.6513 us | 700.0463 us | 140.30 |    2.33 | 1650.45 KB |        1.49 |
| Fill_1000x1000_Base |    562.109 us |   5.8548 us |   5.4766 us |   1.00 |    0.00 | 1105.36 KB |        1.00 |

<sub>The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randomizing points and constructing a list of them.<sub>

# FastPoissonDiscPointFactory Benchmarks
|              Method |          Mean |       Error |      StdDev | Ratio | RatioSD |  Allocated | Alloc Ratio |
|-------------------- |--------------:|------------:|------------:|------:|--------:|-----------:|------------:|
|        Fill_100x100 |    323.778 us |   3.4292 us |   3.2077 us | 77.37 |    1.69 |   24.44 KB |        1.68 |
|   Fill_100x100_Base |      4.189 us |   0.0799 us |   0.0708 us |  1.00 |    0.00 |   14.54 KB |        1.00 |
|        Fill_500x500 |  8,561.601 us | 129.0620 us | 120.7246 us | 90.89 |    2.03 |  469.03 KB |        1.69 |
|   Fill_500x500_Base |     94.172 us |   1.5462 us |   1.5186 us |  1.00 |    0.00 |  277.13 KB |        1.00 |
|      Fill_1000x1000 | 33,922.275 us | 667.1607 us | 843.7447 us | 55.70 |    1.64 | 1850.32 KB |        1.67 |
| Fill_1000x1000_Base |    608.733 us |  11.5738 us |  10.8261 us |  1.00 |    0.00 | 1105.36 KB |        1.00 |

# DelaunatorFactory Benchmarks
|           Method |         Mean |      Error |     StdDev |      Gen0 |      Gen1 |      Gen2 |  Allocated |
|----------------- |-------------:|-----------:|-----------:|----------:|----------:|----------:|-----------:|
|   Create_100x100 |     68.63 us |   1.352 us |   2.258 us |    9.7656 |    0.3662 |         - |   60.13 KB |
|   Create_500x500 |  3,075.05 us |  57.286 us |  53.586 us |  292.9688 |  292.9688 |  292.9688 | 1343.25 KB |
| Create_1000x1000 | 12,486.23 us | 223.543 us | 209.102 us | 1031.2500 | 1000.0000 | 1000.0000 | 5448.11 KB |

# DelaunayFactory Benchmarks
|           Method |         Mean |      Error |     StdDev |      Gen0 |      Gen1 |     Gen2 |  Allocated |
|----------------- |-------------:|-----------:|-----------:|----------:|----------:|---------:|-----------:|
|   Create_100x100 |     35.66 us |   0.706 us |   0.989 us |   12.7563 |    1.9531 |        - |   78.28 KB |
|   Create_500x500 |  4,141.50 us |  80.913 us | 110.755 us |  343.7500 |  320.3125 | 109.3750 | 1930.36 KB |
| Create_1000x1000 | 16,926.01 us | 334.606 us | 446.690 us | 1281.2500 | 1031.2500 | 375.0000 |  7707.8 KB |

# VoronoiFactory Benchmarks
|           Method |        Mean |       Error |      StdDev |      Gen0 |      Gen1 |      Gen2 |   Allocated |
|----------------- |------------:|------------:|------------:|----------:|----------:|----------:|------------:|
|   Create_100x100 |    310.8 us |     2.34 us |     2.19 us |   43.4570 |    9.7656 |         - |   268.96 KB |
|   Create_500x500 | 20,135.8 us |   390.04 us |   383.08 us | 1156.2500 |  875.0000 |  312.5000 |  6575.67 KB |
| Create_1000x1000 | 86,006.6 us | 1,701.96 us | 2,547.41 us | 4166.6667 | 2500.0000 | 1000.0000 | 26003.08 KB |

# Voronoi Benchmarks
|                      Method |         Mean |       Error |      StdDev |   Gen0 | Allocated |
|---------------------------- |-------------:|------------:|------------:|-------:|----------:|
|        IterateCells_100x100 |     887.4 ns |     3.87 ns |     3.43 ns | 0.0048 |      32 B |
|        IterateEdges_100x100 |   6,031.1 ns |    35.21 ns |    31.22 ns |      - |      40 B |
|   IterateNeighbours_100x100 | 295,500.9 ns | 1,424.60 ns | 1,262.87 ns |      - |      56 B |
|                             |              |             |             |        |           |
|        IterateCells_500x500 |  22,626.4 ns |    48.29 ns |    45.17 ns |      - |      32 B |
|        IterateEdges_500x500 | 162,847.8 ns |   277.65 ns |   231.85 ns |      - |      40 B |
|   IterateNeighbours_500x500 | 294,434.5 ns |   962.55 ns |   803.77 ns |      - |      56 B |
|                             |              |             |             |        |           |
|      IterateCells_1000x1000 |  90,101.8 ns |   711.96 ns |   631.13 ns |      - |      32 B |
|      IterateEdges_1000x1000 | 646,625.6 ns | 2,252.80 ns | 2,107.27 ns |      - |      40 B |
| IterateNeighbours_1000x1000 | 294,802.2 ns | 1,434.12 ns | 1,271.31 ns |      - |      56 B |
