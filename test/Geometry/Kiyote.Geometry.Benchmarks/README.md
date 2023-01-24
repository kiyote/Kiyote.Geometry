
# Edge
|              Method |     Mean |    Error |   StdDev | Allocated |
|-------------------- |---------:|---------:|---------:|----------:|
| TryFindIntersection | 13.10 ns | 0.174 ns | 0.154 ns |         - |

# Polygon
|              Method |            Mean |        Error |       StdDev | Allocated |
|-------------------- |----------------:|-------------:|-------------:|----------:|
| TryFindIntersection |     1,372.21 ns |    15.803 ns |    14.009 ns |    1712 B |
|            Contains |        66.90 ns |     0.674 ns |     0.597 ns |         - |
|  Contains_1000x1000 | 1,243,724.41 ns | 8,193.126 ns | 6,841.632 ns |       1 B |
|       Intersections |       280.64 ns |     4.764 ns |     3.978 ns |      88 B |

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

<sub>The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randomizing points and constructing a list of them.<sub>

# FastPoissonDiscPointFactory Benchmarks
|              Method |          Mean |       Error |      StdDev |  Ratio | RatioSD |  Allocated | Alloc Ratio |
|-------------------- |--------------:|------------:|------------:|-------:|--------:|-----------:|------------:|
|        Fill_100x100 |    339.100 us |   1.3914 us |   1.3015 us | 130.61 |    3.32 |   17.31 KB |        2.11 |
|   Fill_100x100_Base |      2.608 us |   0.0500 us |   0.0684 us |   1.00 |    0.00 |    8.21 KB |        1.00 |
|        Fill_500x500 |  8,760.725 us |  39.8753 us |  35.3484 us | 167.03 |    0.76 |  297.57 KB |        2.32 |
|   Fill_500x500_Base |     52.452 us |   0.2337 us |   0.2072 us |   1.00 |    0.00 |   128.3 KB |        1.00 |
|      Fill_1000x1000 | 35,532.069 us | 153.0145 us | 143.1299 us | 110.82 |    0.97 | 1169.78 KB |        2.28 |
| Fill_1000x1000_Base |    320.649 us |   3.3537 us |   3.1371 us |   1.00 |    0.00 |  512.39 KB |        1.00 |

# DelaunatorFactory Benchmarks
|           Method |         Mean |      Error |     StdDev |      Gen0 |      Gen1 |      Gen2 |  Allocated |
|----------------- |-------------:|-----------:|-----------:|----------:|----------:|----------:|-----------:|
|   Create_100x100 |     57.89 us |   0.321 us |   0.285 us |    8.4229 |    0.3662 |         - |   51.71 KB |
|   Create_500x500 |  2,522.49 us |  13.255 us |  12.399 us |  296.8750 |  296.8750 |  296.8750 | 1193.65 KB |
| Create_1000x1000 | 11,275.40 us | 126.288 us | 118.130 us | 1000.0000 | 1000.0000 | 1000.0000 | 4697.59 KB |

# DelaunayFactory Benchmarks
|           Method |        Mean |      Error |    StdDev |     Gen0 |     Gen1 |     Gen2 |  Allocated |
|----------------- |------------:|-----------:|----------:|---------:|---------:|---------:|-----------:|
|   Create_100x100 |    29.69 us |   0.350 us |  0.327 us |   9.2163 |   1.4038 |        - |   56.73 KB |
|   Create_500x500 | 1,006.57 us |  13.298 us | 12.439 us | 283.2031 | 275.3906 | 164.0625 | 1413.92 KB |
| Create_1000x1000 | 7,553.71 us | 103.056 us | 96.399 us | 937.5000 | 914.0625 | 492.1875 | 5673.65 KB |

           Method |         Mean |      Error |     StdDev |      Gen0 |     Gen1 |     Gen2 |  Allocated |
|----------------- |-------------:|-----------:|-----------:|----------:|---------:|---------:|-----------:|
|   Create_100x100 |     30.20 us |   0.587 us |   0.520 us |   11.4136 |   1.6174 |        - |   69.98 KB |
|   Create_500x500 |  2,596.86 us |  51.847 us | 134.758 us |  328.1250 | 320.3125 | 117.1875 | 1752.01 KB |
| Create_1000x1000 | 11,598.71 us | 229.628 us | 336.586 us | 1140.6250 | 828.1250 | 328.1250 | 7018.55 KB |

# VoronoiFactory Benchmarks
|           Method |        Mean |       Error |      StdDev |      Gen0 |      Gen1 |      Gen2 |   Allocated |
|----------------- |------------:|------------:|------------:|----------:|----------:|----------:|------------:|
|   Create_100x100 |    472.1 us |     8.24 us |     7.71 us |   57.1289 |   14.1602 |         - |   350.22 KB |
|   Create_500x500 | 15,222.2 us |   147.43 us |   130.69 us | 1218.7500 | 1156.2500 |  437.5000 |     6805 KB |
| Create_1000x1000 | 68,526.1 us | 1,319.98 us | 1,850.43 us | 3666.6667 | 2555.5556 | 1000.0000 | 25685.65 KB |

# Voronoi Benchmarks
|                      Method |         Mean |       Error |      StdDev |   Gen0 | Allocated |
|---------------------------- |-------------:|------------:|------------:|-------:|----------:|
|        IterateCells_100x100 |     899.5 ns |     8.88 ns |     7.41 ns | 0.0048 |      32 B |
|        IterateEdges_100x100 |   4,967.7 ns |    18.65 ns |    16.53 ns | 0.0076 |      48 B |
|   IterateNeighbours_100x100 | 328,075.9 ns | 1,959.68 ns | 1,833.09 ns |      - |      56 B |
|        IterateCells_500x500 |  22,644.1 ns |    79.22 ns |    70.23 ns |      - |      32 B |
|        IterateEdges_500x500 | 125,492.1 ns | 1,005.46 ns |   839.61 ns |      - |      48 B |
|   IterateNeighbours_500x500 | 328,746.8 ns | 3,010.59 ns | 2,816.10 ns |      - |      56 B |
|      IterateCells_1000x1000 |  89,901.2 ns |   263.49 ns |   246.47 ns |      - |      32 B |
|      IterateEdges_1000x1000 | 516,312.4 ns | 2,282.10 ns | 2,023.02 ns |      - |      48 B |
| IterateNeighbours_1000x1000 | 326,517.3 ns | 1,169.91 ns | 1,037.09 ns |      - |      56 B |
