# Geometry

# FastRandom Benchmarks vs System.Random
```
|                  Method |      Mean |     Error |    StdDev | Ratio | Allocated |
|------------------------ |----------:|----------:|----------:|------:|----------:|
|      FastRandom_NextInt |  1.652 ns | 0.0282 ns | 0.0250 ns |  0.69 |         - |
|    SystemRandom_NextInt |  2.399 ns | 0.0095 ns | 0.0085 ns |  1.00 |         - |
|                         |           |           |           |       |           |
|    FastRandom_NextFloat |  1.876 ns | 0.0046 ns | 0.0039 ns |  0.67 |         - |
|  SystemRandom_NextFloat |  2.823 ns | 0.0357 ns | 0.0334 ns |  1.00 |         - |
|                         |           |           |           |       |           |
|   FastRandom_NextDouble |  2.169 ns | 0.0054 ns | 0.0045 ns |  0.81 |         - |
| SystemRandom_NextDouble |  2.662 ns | 0.0216 ns | 0.0202 ns |  1.00 |         - |
|                         |           |           |           |       |           |
|    FastRandom_NextBytes | 57.345 ns | 0.1519 ns | 0.1186 ns |  2.89 |         - |
|  SystemRandom_NextBytes | 19.852 ns | 0.0755 ns | 0.0706 ns |  1.00 |         - |
```

# FastRandom Benchmarks
```
|                                    Method |      Mean |     Error |    StdDev | Allocated |
|------------------------------------------ |----------:|----------:|----------:|----------:|
|                        FastRandom_NextInt |  1.632 ns | 0.0223 ns | 0.0209 ns |         - |
|             FastRandom_NextInt_UpperBound |  2.683 ns | 0.0102 ns | 0.0095 ns |         - |
|   FastRandom_NextInt_LowerBoundUpperBound |  3.113 ns | 0.0264 ns | 0.0234 ns |         - |
|                       FastRandom_NextByte |  1.671 ns | 0.0076 ns | 0.0060 ns |         - |
|                       FastRandom_NextBool |  1.745 ns | 0.0173 ns | 0.0153 ns |         - |
|                       FastRandom_NextUInt |  1.848 ns | 0.0587 ns | 0.0603 ns |         - |
|                     FastRandom_NextDouble |  2.150 ns | 0.0383 ns | 0.0340 ns |         - |
|                      FastRandom_NextFloat |  2.078 ns | 0.0453 ns | 0.0424 ns |         - |
| FastRandom_NextFloat_LowerBoundUpperBound |  3.329 ns | 0.0163 ns | 0.0145 ns |         - |
|                      FastRandom_NextBytes | 61.452 ns | 0.6098 ns | 0.5406 ns |         - |
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