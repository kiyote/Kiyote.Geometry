# Geometry

# Edge
|    Method |     Mean |    Error |   StdDev | Allocated |
|---------- |---------:|---------:|---------:|----------:|
| Intersect | 28.02 ns | 0.253 ns | 0.224 ns |         - |

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
|              Method |          Mean |       Error |      StdDev |  Ratio | RatioSD | Allocated |
|-------------------- |--------------:|------------:|------------:|-------:|--------:|----------:|
|        Fill_100x100 |    868.108 us |  10.5752 us |   9.3747 us | 224.06 |    2.82 |     22 KB |
|   Fill_100x100_Base |      3.875 us |   0.0337 us |   0.0299 us |   1.00 |    0.00 |     15 KB |
|        Fill_500x500 | 21,688.316 us | 224.1246 us | 209.6463 us | 247.16 |    3.29 |    422 KB |
|   Fill_500x500_Base |     87.626 us |   1.0238 us |   0.9075 us |   1.00 |    0.00 |    277 KB |
|      Fill_1000x1000 | 87,430.211 us | 665.7273 us | 590.1499 us | 169.09 |    1.62 |  1,649 KB |
| Fill_1000x1000_Base |    516.773 us |   4.8951 us |   4.0877 us |   1.00 |    0.00 |  1,105 KB |

<sub>The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randominzing points and constructing a list of them.<sub>

# FastPoissonDiscPointFactory Benchmarks
|              Method |          Mean |       Error |        StdDev | Ratio | RatioSD |  Allocated | Alloc Ratio |
|-------------------- |--------------:|------------:|--------------:|------:|--------:|-----------:|------------:|
|        Fill_100x100 |    317.517 us |   6.1552 us |     7.5592 us | 75.21 |    3.25 |   24.07 KB |        1.66 |
|   Fill_100x100_Base |      4.213 us |   0.0828 us |     0.1556 us |  1.00 |    0.00 |   14.54 KB |        1.00 |
|        Fill_500x500 |  8,404.661 us | 162.7798 us |   238.6005 us | 95.17 |    3.95 |  473.44 KB |        1.71 |
|   Fill_500x500_Base |     88.403 us |   1.7088 us |     2.0985 us |  1.00 |    0.00 |  277.13 KB |        1.00 |
|      Fill_1000x1000 | 33,153.867 us | 661.5251 us | 1,086.9047 us | 56.04 |    2.41 | 1852.21 KB |        1.68 |
| Fill_1000x1000_Base |    592.301 us |  11.7908 us |    18.7014 us |  1.00 |    0.00 | 1105.36 KB |        1.00 |

# DelaunatorFactory Benchmarks
|           Method |         Mean |      Error |     StdDev |      Gen0 |      Gen1 |      Gen2 |  Allocated |
|----------------- |-------------:|-----------:|-----------:|----------:|----------:|----------:|-----------:|
|   Create_100x100 |     68.63 us |   1.352 us |   2.258 us |    9.7656 |    0.3662 |         - |   60.13 KB |
|   Create_500x500 |  3,075.05 us |  57.286 us |  53.586 us |  292.9688 |  292.9688 |  292.9688 | 1343.25 KB |
| Create_1000x1000 | 12,486.23 us | 223.543 us | 209.102 us | 1031.2500 | 1000.0000 | 1000.0000 | 5448.11 KB |

# DelaunayFactory Benchmarks
|           Method |         Mean |      Error |     StdDev |      Gen0 |      Gen1 |     Gen2 |  Allocated |
|----------------- |-------------:|-----------:|-----------:|----------:|----------:|---------:|-----------:|
|   Create_100x100 |     38.97 us |   0.777 us |   1.949 us |   15.8691 |    2.3804 |        - |   97.59 KB |
|   Create_500x500 |  5,544.64 us | 109.261 us | 185.533 us |  421.8750 |  359.3750 | 132.8125 |  2315.2 KB |
| Create_1000x1000 | 24,462.18 us | 485.447 us | 811.072 us | 1687.5000 | 1281.2500 | 718.7500 | 9253.15 KB |

# VoronoiFactory Benchmarks
|           Method |        Mean |       Error |      StdDev |      Gen0 |      Gen1 |      Gen2 |   Allocated |
|----------------- |------------:|------------:|------------:|----------:|----------:|----------:|------------:|
|   Create_100x100 |    288.0 us |     2.39 us |     2.12 us |   44.4336 |   11.7188 |         - |   273.41 KB |
|   Create_500x500 | 18,602.1 us |   440.59 us | 1,299.09 us | 1187.5000 |  906.2500 |  343.7500 |   6812.2 KB |
| Create_1000x1000 | 86,408.3 us | 1,723.77 us | 2,180.01 us | 4500.0000 | 3000.0000 | 1333.3333 | 27077.96 KB |

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