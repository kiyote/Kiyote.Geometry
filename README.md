# Geometry

# FastRandom Benchmarks vs System.Random
```
|                  Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
|    FastRandom_NextBytes | 56.185 ns | 0.3779 ns | 0.3535 ns |  3.11 |    0.02 |         - |          NA |
|  SystemRandom_NextBytes | 18.065 ns | 0.1179 ns | 0.1103 ns |  1.00 |    0.00 |         - |          NA |
|                         |           |           |           |       |         |           |             |
|   FastRandom_NextDouble |  1.783 ns | 0.0075 ns | 0.0070 ns |  0.69 |    0.01 |         - |          NA |
| SystemRandom_NextDouble |  2.586 ns | 0.0293 ns | 0.0274 ns |  1.00 |    0.00 |         - |          NA |
|                         |           |           |           |       |         |           |             |
|    FastRandom_NextFloat |  2.565 ns | 0.0256 ns | 0.0227 ns |  0.95 |    0.01 |         - |          NA |
|  SystemRandom_NextFloat |  2.698 ns | 0.0204 ns | 0.0190 ns |  1.00 |    0.00 |         - |          NA |
|                         |           |           |           |       |         |           |             |
|      FastRandom_NextInt |  1.540 ns | 0.0182 ns | 0.0170 ns |  0.68 |    0.01 |         - |          NA |
|    SystemRandom_NextInt |  2.268 ns | 0.0140 ns | 0.0131 ns |  1.00 |    0.00 |         - |          NA |
```

# FastRandom Benchmarks
```
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
```

# PoissonDiscPointFactory Benchmarks
```
|              Method |          Mean |       Error |      StdDev |  Ratio | RatioSD | Allocated |
|-------------------- |--------------:|------------:|------------:|-------:|--------:|----------:|
|        Fill_100x100 |    868.108 us |  10.5752 us |   9.3747 us | 224.06 |    2.82 |     22 KB |
|   Fill_100x100_Base |      3.875 us |   0.0337 us |   0.0299 us |   1.00 |    0.00 |     15 KB |
|                     |               |             |             |        |         |           |
|        Fill_500x500 | 21,688.316 us | 224.1246 us | 209.6463 us | 247.16 |    3.29 |    422 KB |
|   Fill_500x500_Base |     87.626 us |   1.0238 us |   0.9075 us |   1.00 |    0.00 |    277 KB |
|                     |               |             |             |        |         |           |
|      Fill_1000x1000 | 87,430.211 us | 665.7273 us | 590.1499 us | 169.09 |    1.62 |  1,649 KB |
| Fill_1000x1000_Base |    516.773 us |   4.8951 us |   4.0877 us |   1.00 |    0.00 |  1,105 KB |
```
<sub>The `_Base` versions allocate an approximate number of random points in order to be able to judge roughly
how much of the cost of the routine came simply from randominzing points and constructing a list of them.<sub>

# FastPoissonDiscPointFactory Benchmarks
|              Method |          Mean |       Error |      StdDev | Ratio | RatioSD | Allocated |
|-------------------- |--------------:|------------:|------------:|------:|--------:|----------:|
|        Fill_100x100 |    330.969 us |   5.7507 us |   8.7820 us | 82.94 |    2.72 |     24 KB |
|   Fill_100x100_Base |      4.004 us |   0.0744 us |   0.0764 us |  1.00 |    0.00 |     15 KB |
|                     |               |             |             |       |         |           |
|        Fill_500x500 |  8,505.631 us | 129.7736 us | 121.3903 us | 93.58 |    2.59 |    473 KB |
|   Fill_500x500_Base |     91.014 us |   1.7164 us |   1.9766 us |  1.00 |    0.00 |    277 KB |
|                     |               |             |             |       |         |           |
|      Fill_1000x1000 | 34,982.665 us | 686.4222 us | 842.9882 us | 58.21 |    2.56 |  1,852 KB |
| Fill_1000x1000_Base |    607.515 us |  11.8809 us |  20.4940 us |  1.00 |    0.00 |  1,105 KB |