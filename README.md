
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


# FastPoissonDiscPointFactory Benchmarks
<sub>
The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randomizing points and constructing a list of them.
The surface is filled with a separation of 5.
</sub>

|              Method |          Mean |       Error |        StdDev |  Ratio | RatioSD |  Allocated | Alloc Ratio |
|-------------------- |--------------:|------------:|--------------:|-------:|--------:|-----------:|------------:|
|        Fill_100x100 |    357.744 us |   6.5474 us |     6.1244 us | 131.23 |    3.25 |   17.31 KB |        2.11 |
|   Fill_100x100_Base |      2.724 us |   0.0536 us |     0.0769 us |   1.00 |    0.00 |    8.21 KB |        1.00 |
|        Fill_500x500 |  9,185.087 us | 180.4538 us |   252.9710 us | 162.86 |    5.37 |  297.72 KB |        2.32 |
|   Fill_500x500_Base |     56.388 us |   1.0657 us |     1.0944 us |   1.00 |    0.00 |   128.3 KB |        1.00 |
|      Fill_1000x1000 | 37,575.765 us | 746.8535 us | 1,247.8244 us | 110.17 |    5.07 | 1166.02 KB |        2.28 |
| Fill_1000x1000_Base |    340.854 us |   6.5919 us |     9.2409 us |   1.00 |    0.00 |  512.39 KB |        1.00 |

All of the following benchmarks fill a surface of the specified size with a poisson disc set of random values with a separation of 5.

# D3DelaunayFactory Benchmarks
|           Method |         Mean |      Error |     StdDev |  Allocated |
|----------------- |-------------:|-----------:|-----------:|-----------:|
|   Create_100x100 |     89.95 us |   0.681 us |   0.604 us |   73.06 KB |
|   Create_500x500 |  4,025.99 us |  64.720 us |  57.373 us |  1790.8 KB |
| Create_1000x1000 | 17,113.73 us | 197.587 us | 184.823 us |  7173.9 KB |


# D3VoronoiFactory Benchmarks
|           Method |        Mean |       Error |      StdDev |   Allocated |
|----------------- |------------:|------------:|------------:|------------:|
|   Create_100x100 |    332.6 us |     6.60 us |     6.18 us |   321.88 KB |
|   Create_500x500 | 10,961.3 us |   123.76 us |   115.77 us |  7480.06 KB |
| Create_1000x1000 | 61,192.8 us | 5,057.99 us | 4,731.25 us | 30640.84 KB |


