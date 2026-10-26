# Benchmarks

## Incremental D3 refactoring (2026-10-26)

**Timing status:** the user reported concurrent compilation during the earlier
measurement window. The incremental timing tables below are retained as raw
historical observations, but their speedup/regression conclusions are
provisional. Use the subsequent isolated reference/candidate comparison for
runtime decisions. Allocation figures and deterministic storage calculations
are separate from this CPU-load concern.

These focused runs use BenchmarkDotNet 0.15.8, .NET SDK
11.0.100-rc.2.26504.105, .NET runtime 10.0.13, and the Intel Core i7-9700K.
Each column uses two process launches, four warmups, twelve measured iterations,
and a requested 300 ms iteration time. Values are means, not guaranteed
improvements; some distributions were multimodal and allocations were unchanged
by O7. G0 is the correctness baseline at `ea40865`, not the older suite below.
Grid extents 100/500/1000 contain 400/10,000/40,000 sites respectively.

| Workload | G0 mean | O7 fused preparation mean |
|---|---:|---:|
| Delaunay, 400 sites | 82.33 us | 74.09 us |
| Delaunay, 10,000 sites | 3,800.41 us | 3,774.25 us |
| Delaunay, 40,000 sites | 18,256.22 us | 17,903.06 us |
| Voronoi, 400 sites | 398.78 us | 383.91 us |
| Voronoi, 10,000 sites | 17,315.27 us | 16,556.34 us |
| Voronoi, 40,000 sites | 91,459.14 us | 85,275.77 us |
| Voronoi workload, grid | 384.73 us | 371.83 us |
| Voronoi workload, jittered grid | 573.88 us | 568.82 us |
| Voronoi workload, high degree | 35.85 us | 30.69 us |

O7 fuses site validation, ownership, and coordinate conversion; it does not
eliminate the retained point snapshot or the temporary double-coordinate array.
No representation, deferred-output, or geometry approximation change is included.

### O8: on-demand hull rays

The next isolated change removes the `4n`-double vector array and derives rays
only for hull sites. Same two-launch settings as O7:

| Voronoi workload | O7 mean | O8 mean | O7 allocated | O8 allocated |
|---|---:|---:|---:|---:|
| 400 sites | 383.91 us | 342.69 us | 346.11 KB | 333.59 KB |
| 10,000 sites | 16,556.34 us | 12,881.30 us | 10,035.70 KB | 9,720.50 KB |
| 40,000 sites | 85,275.77 us | 73,218.06 us | 41,428.42 KB | 40,178.40 KB |
| Workload grid | 371.83 us | 350.82 us | 346.11 KB | 333.59 KB |
| Workload jittered grid | 568.82 us | 535.62 us | 485.67 KB | 473.15 KB |
| Workload high degree | 30.69 us | 31.21 us | 52.59 KB | 51.28 KB |

The high-degree runtime intervals overlap; no speedup is claimed there.
The 40,000-site runtime distribution is bimodal. The payload saving is
deterministically `32n` bytes plus one array header, independent of timing noise.

### O2, incremental variant: CSR adjacency

Replace cell-keyed dictionaries and per-cell neighbour arrays/wrappers with
owned offset/index arrays and `GetNeighbourIndices(int)`. All geometry and
adjacency are still computed eagerly. Same two-launch settings:

| Voronoi workload | O8 mean | CSR mean | O8 allocated | CSR allocated |
|---|---:|---:|---:|---:|
| 400 sites | 342.69 us | 318.88 us | 333.59 KB | 295.19 KB |
| 10,000 sites | 12,881.30 us | 10,002.88 us | 9,720.50 KB | 8,782.84 KB |
| 40,000 sites | 73,218.06 us | 56,807.16 us | 40,178.40 KB | 36,335.53 KB |
| Workload grid | 350.82 us | 310.44 us | 333.59 KB | 295.19 KB |
| Workload jittered grid | 535.62 us | 505.47 us | 473.15 KB | 434.26 KB |
| Workload high degree | 31.21 us | 28.01 us | 51.28 KB | 46.94 KB |

The 40,000-site means improve about 22% and allocated bytes about 9.6%
relative to O8. The 400-site factory distribution has multiple modes; its
timing is less conclusive than the large-input result. Neighbour traversal is
covered by a zero-allocation regression and compared with primal-mesh adjacency.
Cells and polygon objects remain unchanged; this is not the full flat-vertex
representation proposed in O2.

### O6: pre-sized compact directed-edge keys

Final polygon side counts size the edge set once. Keys contain two packed
32-bit points, use mixed endpoint hashing rather than plain `ulong` XOR hashing,
and preserve edge direction. Published output still contains complete `Edge`
values, produced eagerly. Same two-launch settings:

| Voronoi workload | CSR mean | Compact/pre-sized mean | CSR allocated | Compact/pre-sized allocated |
|---|---:|---:|---:|---:|
| 400 sites | 318.88 us | 295.17 us | 295.19 KB | 233.21 KB |
| 10,000 sites | 10,002.88 us | 9,330.42 us | 8,782.84 KB | 5,655.49 KB |
| 40,000 sites | 56,807.16 us | 57,296.33 us | 36,335.53 KB | 22,877.57 KB |
| Workload grid | 310.44 us | 300.38 us | 295.19 KB | 233.21 KB |
| Workload jittered grid | 505.47 us | 420.09 us | 434.26 KB | 269.34 KB |
| Workload high degree | 28.01 us | 25.98 us | 46.94 KB | 29.95 KB |

The 40,000-site runtime intervals overlap; this case is a memory win, not a
demonstrated incremental speedup. Allocated bytes decrease about 37% there.
An initial compact-key variant without capacity sizing regressed the small grid
workload (330.61 us versus 310.44 us); it was not accepted. Sizing from finalized
side counts removed repeated set growth and produced the results above.

### O1: local legalization cursor and invariant hull hashing

Keep the legalization cursor/span local while the call-scoped owner handles
only cold pooled growth. Hoist the hull probe key outside its inner loop.
Same two-launch settings; core/Delaunay allocation is unchanged.

| Workload | Relevant previous mean | O1 mean |
|---|---:|---:|
| Core, 40,000 sites, unchecked | 14,866.50 us (G0) | 12,693.43 us |
| Core, 40,000 sites, duplicate check | 14,985.24 us (G0) | 13,400.10 us |
| Delaunay, 400 sites | 74.09 us (O7) | 65.28 us |
| Delaunay, 10,000 sites | 3,774.25 us (O7) | 3,232.74 us |
| Delaunay, 40,000 sites | 17,903.06 us (O7) | 15,440.40 us |
| Voronoi, 40,000 sites | 57,296.33 us (O6) | 54,421.19 us |

The core and Delaunay code did not change between their named previous stage
and O1. The checked-core distribution has multiple modes. The main core result
is an approximately 11-15% mean reduction relative to G0, retaining the
growable-stack correctness fix rather than returning to silent overflow.

### Execution status and limits of these measurements

Accepted work covers G0's seed/degeneracy/stack gates, O7 preparation, O8 hull
rays, O2's incremental CSR variant, O6's compact/pre-sized directed-edge
publication, and O1's local cursor/invariant-key changes. The original result
still owns `Cell`/`Polygon` objects; no lazy output or reduced-work mode was used.

For 40,000-site full Voronoi creation, the earlier G0 versus O1 observation was
91.46 ms versus 54.42 ms, and 41,427.87 KB versus approximately 22,878 KB allocated
per call. This is an observed 40.5% lower mean time and 44.8% lower allocation,
not a confirmed uncontended speedup. These
are sequential, two-launch benchmark runs, not a randomized interleaved
experiment. Runtime variance, JIT tiering, hardware load, and GC can influence
the comparison. Allocated bytes are not retained-memory or peak-RSS figures.

The full flattened-cell/vertex rewrite, bounded clipping writers, polygon fast
paths, shared traversal, expanded workspace pooling, indexed Delaunay output,
alternative sorts/layouts, parallel creation, and half-plane clipping remain
unimplemented options. Further work should first collect current CPU/allocation
profiles and include irregular/clustered, translated thin, mostly-hull, and
sanitization workloads; existing end-to-end fixtures do not cover that complete
matrix. No claim is made that all fourteen plan options are complete or that
the sampled exact-predicate tests prove every integer-domain input correct.

Regression coverage now also includes a warm-pool full-diagram allocation
ceiling (400-site grid, 240 KiB) and old-result stability across concurrent
creation, duplicate rejection, and subsequent successful reuse.

### O7 follow-up: heavily sanitized input

The normal, unsanitized path still snapshots and fills coordinates in one pass.
The sanitized path now determines the accepted sites before allocating their
coordinates. This deliberately uses a second pass over accepted sites rather
than retaining coordinate capacity for every rejected input site.

For 10,000 input sites reduced to three, coordinate payload falls from 160,000
bytes in the initial O7 variant to 48 bytes. Regressions verify exact coordinate
length/order and total preparation allocation at most 82,048 bytes, including
the input-sized point snapshot, for both duplicate-heavy and outside-heavy
inputs. These tests failed before the fix and pass afterward. This restores
the original sanitized path's coordinate-memory scaling; no sanitized runtime
improvement is claimed.

## Earlier full-suite measurements

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.8893/24H2/2024Update/HudsonValley)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.400-preview.0.26322.102
  [Host] : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3
```

## D3DelaunayFactoryBenchmarks

| Method           | Mean         | Error      | StdDev     | Allocated  |
|----------------- |-------------:|-----------:|-----------:|-----------:|
| Create_100x100   |     88.36 μs |   2.785 μs |   1.842 μs |   46.21 KB |
| Create_500x500   |  3,630.19 μs | 151.704 μs |  90.277 μs | 1167.74 KB |
| Create_1000x1000 | 16,986.10 μs | 723.838 μs | 478.774 μs | 4679.08 KB |

## D3VoronoiFactoryBenchmarks

| Method           | Mean        | Error       | StdDev      | Allocated   |
|----------------- |------------:|------------:|------------:|------------:|
| Create_100x100   |    371.2 μs |    10.59 μs |     7.01 μs |   346.11 KB |
| Create_500x500   | 15,670.5 μs |   892.56 μs |   590.38 μs | 10035.29 KB |
| Create_1000x1000 | 77,850.1 μs | 5,578.07 μs | 3,689.55 μs | 41427.86 KB |

## MapboxDelaunatorFactoryBenchmarks

| Method           | RejectDuplicates | Mean         | Error      | StdDev     | Allocated  |
|----------------- |----------------- |-------------:|-----------:|-----------:|-----------:|
| **Create_100x100**   | **False**            |     **67.03 μs** |   **3.202 μs** |   **2.118 μs** |   **19.05 KB** |
| Create_500x500   | False            |  3,070.85 μs | 183.497 μs | 121.372 μs |   470.4 KB |
| Create_1000x1000 | False            | 14,490.84 μs | 895.182 μs | 592.108 μs | 1878.35 KB |
| **Create_100x100**   | **True**             |     **67.92 μs** |   **5.185 μs** |   **3.430 μs** |   **19.05 KB** |
| Create_500x500   | True             |  3,149.22 μs | 217.698 μs | 143.994 μs |   470.4 KB |
| Create_1000x1000 | True             | 14,952.44 μs | 703.016 μs | 465.002 μs | 1878.34 KB |

## EdgeBenchmarks

| Method              | Mean       | Error     | StdDev    | Allocated |
|-------------------- |-----------:|----------:|----------:|----------:|
| HasIntersection     |  8.5958 ns | 0.4683 ns | 0.3098 ns |         - |
| TryFindIntersection | 16.0876 ns | 1.0070 ns | 0.6661 ns |         - |
| GetBoundingBox      |  4.3223 ns | 0.3039 ns | 0.2010 ns |         - |
| Interpolate         |  5.5754 ns | 0.2306 ns | 0.1525 ns |         - |
| Ctor_PointPoint     |  0.0162 ns | 0.0056 ns | 0.0037 ns |         - |
| Ctor_IntIntIntInt   |  0.2642 ns | 0.0121 ns | 0.0080 ns |         - |

## IntersectBenchmarks

| Method              | Mean     | Error    | StdDev   | Allocated |
|-------------------- |---------:|---------:|---------:|----------:|
| HasIntersection     | 10.83 ns | 0.097 ns | 0.064 ns |         - |
| TryFindIntersection | 18.25 ns | 0.075 ns | 0.045 ns |         - |

## GeometryWorkloadBenchmarks

| Method              | Workload  | Mean      | Error     | StdDev    | Allocated |
|-------------------- |---------- |----------:|----------:|----------:|----------:|
| **HasIntersection**     | **Small**     |  **9.517 ns** | **0.0349 ns** | **0.0231 ns** |         **-** |
| TryFindIntersection | Small     | 18.326 ns | 0.0642 ns | 0.0336 ns |         - |
| **HasIntersection**     | **Screen**    |  **9.534 ns** | **0.0885 ns** | **0.0585 ns** |         **-** |
| TryFindIntersection | Screen    | 18.263 ns | 0.1076 ns | 0.0640 ns |         - |
| **HasIntersection**     | **Boundary**  |  **9.472 ns** | **0.0915 ns** | **0.0545 ns** |         **-** |
| TryFindIntersection | Boundary  | 18.301 ns | 0.1013 ns | 0.0603 ns |         - |
| **HasIntersection**     | **Irregular** |  **9.475 ns** | **0.0546 ns** | **0.0286 ns** |         **-** |
| TryFindIntersection | Irregular | 18.109 ns | 0.3414 ns | 0.2031 ns |         - |

## VoronoiWorkloadBenchmarks

| Method | Workload     | Mean      | Error    | StdDev   | Allocated |
|------- |------------- |----------:|---------:|---------:|----------:|
| **Create** | **Grid**         | **388.17 μs** | **6.794 μs** | **4.494 μs** | **346.11 KB** |
| **Create** | **JitteredGrid** | **588.25 μs** | **7.783 μs** | **4.070 μs** | **485.67 KB** |
| **Create** | **HighDegree**   |  **34.23 μs** | **1.846 μs** | **1.221 μs** |  **52.59 KB** |

## PolygonBenchmarks

| Method                                                    | Mean      | Error     | StdDev    | Allocated |
|---------------------------------------------------------- |----------:|----------:|----------:|----------:|
| Ctor                                                      | 153.03 ns |  4.849 ns |  3.208 ns |     216 B |
| Contains_Polygon                                          |  23.61 ns |  0.692 ns |  0.458 ns |         - |
| HasOverlap                                                |  10.67 ns |  0.428 ns |  0.283 ns |         - |
| IsEquivalentTo                                            |  14.70 ns |  0.781 ns |  0.517 ns |         - |
| TryIntersect_WithIntersection                             | 446.70 ns | 23.179 ns | 15.331 ns |     264 B |
| TryIntersect_WithoutIntersection                          | 159.32 ns |  7.352 ns |  4.863 ns |         - |
| TryFindBoundaryIntersections_Edge_WithIntersections       |  72.59 ns |  4.623 ns |  3.058 ns |      64 B |
| TryFindBoundaryIntersections_Edge_WithoutIntersections    |  27.46 ns |  1.824 ns |  1.207 ns |         - |
| TryFindBoundaryIntersections_Polygon_WithIntersections    | 161.78 ns |  4.563 ns |  2.715 ns |      80 B |
| TryFindBoundaryIntersections_Polygon_WithoutIntersections |  98.82 ns |  4.982 ns |  3.295 ns |         - |
| HasIntersection_Edge                                      |  10.67 ns |  0.678 ns |  0.448 ns |         - |
| HasIntersection_Polygon                                   |  10.41 ns |  0.361 ns |  0.239 ns |         - |
| HasIntersection_EdgeArray                                 |  21.02 ns |  1.003 ns |  0.663 ns |         - |

## RectangleBenchmarks

| Method                      | Mean      | Error     | StdDev    | Allocated |
|---------------------------- |----------:|----------:|----------:|----------:|
| Contains_Rect               | 1.7004 ns | 0.1190 ns | 0.0787 ns |         - |
| Contains_IRect              | 2.5669 ns | 0.1576 ns | 0.0824 ns |         - |
| Contains_Point              | 1.1404 ns | 0.0846 ns | 0.0503 ns |         - |
| Contains_IntInt             | 0.4184 ns | 0.1132 ns | 0.0749 ns |         - |
| IsEquivalentTo_Rect         | 2.4333 ns | 0.2554 ns | 0.1689 ns |         - |
| IsEquivalentTo_IRect        | 2.7006 ns | 0.0793 ns | 0.0524 ns |         - |
| IsEquivalentTo_PointPoint   | 0.8603 ns | 0.0500 ns | 0.0331 ns |         - |
| IsEquivalentTo_IntIntIntInt | 0.8164 ns | 0.0636 ns | 0.0421 ns |         - |

## SimpleQuadTreeNodeBenchmarks

| Method             | Job        | InvocationCount | UnrollFactor | Mean            | Error          | StdDev         | Allocated |
|------------------- |----------- |---------------- |------------- |----------------:|---------------:|---------------:|----------:|
| Insert             | Job-NXGAMU | 1               | 1            |   2,320.0000 ns |    750.1955 ns |    496.2078 ns |         - |
| Count              | Job-AKPLXZ | Default         | 16           |       0.0807 ns |      0.1080 ns |      0.0714 ns |         - |
| GetSubTreeContents | Job-AKPLXZ | Default         | 16           | 355,693.4239 ns | 28,252.3682 ns | 18,687.1923 ns |  240103 B |
| Query              | Job-AKPLXZ | Default         | 16           |   2,246.1712 ns |     62.0565 ns |     41.0465 ns |     776 B |
| QueryVisitor       | Job-AKPLXZ | Default         | 16           |   1,482.1865 ns |     23.6556 ns |     14.0771 ns |         - |

## IntegerRasterizerBenchmarks

| Method                     | Mean         | Error        | StdDev       | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |-------------:|-------------:|-------------:|------:|--------:|----------:|------------:|
| FillSmallRaw_PerSpan       |     235.1 ns |      8.46 ns |      5.60 ns | 0.001 |    0.00 |         - |          NA |
| FillSmallValidated_PerSpan |     157.6 ns |      6.34 ns |      4.19 ns | 0.000 |    0.00 |         - |          NA |
| FillPolygon_PerPixel       | 324,961.6 ns | 16,878.52 ns | 11,164.09 ns | 1.001 |    0.05 |         - |          NA |
| FillPolygon_PerSpan        | 103,005.4 ns |  6,230.86 ns |  4,121.33 ns | 0.317 |    0.02 |         - |          NA |
| FillTriangle_PerPixel      | 195,515.6 ns | 31,703.35 ns | 20,969.80 ns | 0.602 |    0.06 |         - |          NA |
| FillTriangle_PerSpan       |  63,514.6 ns |  3,982.96 ns |  2,634.48 ns | 0.196 |    0.01 |         - |          NA |
| Outline_PerPixel           |   6,647.0 ns |    327.10 ns |    216.36 ns | 0.020 |    0.00 |         - |          NA |
| Outline_PerSpan            |   6,148.5 ns |    185.75 ns |    110.53 ns | 0.019 |    0.00 |         - |          NA |

## TriangleRepresentationBenchmarks

| Method             | Count | Mean           | Error           | StdDev         | Allocated |
|------------------- |------ |---------------:|----------------:|---------------:|----------:|
| **ConstructReference** | **16**    |     **123.313 ns** |       **4.7437 ns** |      **2.8229 ns** |     **792 B** |
| ConstructValue     | 16    |      56.520 ns |       4.6741 ns |      2.7815 ns |     408 B |
| TraverseReference  | 16    |      10.236 ns |       0.2998 ns |      0.1784 ns |         - |
| TraverseValue      | 16    |       8.847 ns |       0.2791 ns |      0.1661 ns |         - |
| CopyReference      | 16    |      11.498 ns |       0.7556 ns |      0.4496 ns |         - |
| CopyValue          | 16    |       9.863 ns |       0.5182 ns |      0.3428 ns |         - |
| **ConstructReference** | **2048**  |  **15,399.714 ns** |   **1,648.9538 ns** |  **1,090.6808 ns** |   **98328 B** |
| ConstructValue     | 2048  |   5,656.917 ns |     467.9060 ns |    309.4909 ns |   49176 B |
| TraverseReference  | 2048  |   1,591.787 ns |      37.9386 ns |     19.8426 ns |         - |
| TraverseValue      | 2048  |   1,324.015 ns |      48.1020 ns |     28.6247 ns |         - |
| CopyReference      | 2048  |     399.048 ns |      11.6250 ns |      6.9178 ns |         - |
| CopyValue          | 2048  |   1,050.252 ns |      34.1314 ns |     20.3111 ns |         - |
| **ConstructReference** | **4096**  |  **29,116.643 ns** |   **2,272.6129 ns** |  **1,503.1927 ns** |  **196632 B** |
| ConstructValue     | 4096  |  45,114.322 ns |   4,334.1033 ns |  2,866.7410 ns |   98338 B |
| TraverseReference  | 4096  |   3,467.532 ns |     217.7374 ns |    144.0198 ns |         - |
| TraverseValue      | 4096  |   2,793.185 ns |     272.2243 ns |    180.0595 ns |         - |
| CopyReference      | 4096  |   1,071.790 ns |      47.2039 ns |     31.2225 ns |         - |
| CopyValue          | 4096  |   2,614.137 ns |      44.3099 ns |     26.3681 ns |         - |
| **ConstructReference** | **16384** | **933,290.023 ns** | **114,847.3101 ns** | **68,343.7921 ns** |  **786467 B** |
| ConstructValue     | 16384 |  61,222.409 ns |   2,868.0194 ns |  1,897.0173 ns |  393249 B |
| TraverseReference  | 16384 |  18,010.208 ns |   1,032.4372 ns |    682.8933 ns |         - |
| TraverseValue      | 16384 |  12,997.640 ns |     448.0413 ns |    266.6222 ns |         - |
| CopyReference      | 16384 |   4,173.644 ns |     260.1456 ns |    172.0702 ns |         - |
| CopyValue          | 16384 |  12,474.066 ns |     787.7602 ns |    521.0546 ns |         - |

## SearchableVoronoiBenchmarks

| Method        | QuerySize | Mean       | Error     | StdDev    | Allocated |
|-------------- |---------- |-----------:|----------:|----------:|----------:|
| **Search**        | **20**        |   **289.8 ns** |  **21.52 ns** |  **14.23 ns** |      **88 B** |
| SearchVisitor | 20        |   210.2 ns |  13.10 ns |   8.67 ns |         - |
| **Search**        | **200**       | **7,920.5 ns** | **665.87 ns** | **440.43 ns** |    **8384 B** |
| SearchVisitor | 200       | 2,742.7 ns | 163.49 ns | 108.14 ns |         - |

## PointBenchmarks

| Method                  | Mean       | Error     | StdDev    | Median     | Allocated |
|------------------------ |-----------:|----------:|----------:|-----------:|----------:|
| Deconstruct             |  0.0000 ns | 0.0000 ns | 0.0000 ns |  0.0000 ns |         - |
| ToStringBenchmark       | 43.6398 ns | 3.3793 ns | 2.2352 ns | 43.3599 ns |      48 B |
| IsAdjacentTo_Orthogonal |  3.4669 ns | 0.1904 ns | 0.1260 ns |  3.4567 ns |         - |
| IsAdjacentTo_Diagonal   |  3.3978 ns | 0.2022 ns | 0.1338 ns |  3.3965 ns |         - |
| Add_Point               |  0.2284 ns | 0.0542 ns | 0.0323 ns |  0.2167 ns |         - |
| Add_IntInt              |  0.2482 ns | 0.0760 ns | 0.0503 ns |  0.2314 ns |         - |
| Subtract_Point          |  0.1470 ns | 0.0437 ns | 0.0228 ns |  0.1445 ns |         - |
| Subtract_IntInt         |  0.2945 ns | 0.1764 ns | 0.1049 ns |  0.2884 ns |         - |
| Negate                  |  0.2402 ns | 0.0740 ns | 0.0490 ns |  0.2235 ns |         - |
| Operator_Add            |  0.1882 ns | 0.0281 ns | 0.0147 ns |  0.1850 ns |         - |
| Operator_Subtract       |  0.2169 ns | 0.0381 ns | 0.0199 ns |  0.2110 ns |         - |
| Operator_Negate         |  0.3131 ns | 0.0871 ns | 0.0576 ns |  0.3183 ns |         - |
| ShiftRight              |  0.2138 ns | 0.0718 ns | 0.0475 ns |  0.1890 ns |         - |
| And                     |  0.2022 ns | 0.0571 ns | 0.0340 ns |  0.1919 ns |         - |
| Ctor                    |  0.0193 ns | 0.0405 ns | 0.0268 ns |  0.0064 ns |         - |
| ManhattanDistance       |  3.4305 ns | 0.2316 ns | 0.1532 ns |  3.3571 ns |         - |
| ChebyshevDistance       |  3.3396 ns | 0.2547 ns | 0.1685 ns |  3.2554 ns |         - |
| DistanceSquared         |  3.0078 ns | 0.1460 ns | 0.0869 ns |  2.9831 ns |         - |
| Operator_Equals         |  0.1926 ns | 0.0442 ns | 0.0263 ns |  0.1873 ns |         - |
| Operator_NotEquals      |  0.0214 ns | 0.0391 ns | 0.0259 ns |  0.0073 ns |         - |
| Equals_Object           |  0.2002 ns | 0.0343 ns | 0.0204 ns |  0.1945 ns |         - |
| Equals_IEquatable       |  0.2260 ns | 0.0286 ns | 0.0149 ns |  0.2229 ns |         - |
| GetHashCode_Single      |  0.7552 ns | 0.0369 ns | 0.0193 ns |  0.7502 ns |         - |
| HashSet_Contains        |  5.6320 ns | 0.2535 ns | 0.1677 ns |  5.6136 ns |         - |
| Dictionary_TryGetValue  |  5.8721 ns | 0.2532 ns | 0.1507 ns |  5.8312 ns |         - |
| HashSet_Build           |  7.1023 ns | 0.1668 ns | 0.0872 ns |  7.0853 ns |      22 B |
| Min                     |  0.1805 ns | 0.0713 ns | 0.0472 ns |  0.1542 ns |         - |
| Max                     |  0.2541 ns | 0.0586 ns | 0.0349 ns |  0.2449 ns |         - |

## DenseGridSourceBenchmarks

| Method                              | Size | Mean         | Error       | StdDev      | Allocated |
|------------------------------------ |----- |-------------:|------------:|------------:|----------:|
| **GetOccupiedRuns_Cached**              | **64**   |     **118.7 ns** |     **0.96 ns** |     **0.50 ns** |         **-** |
| TryClearCell_TrySetCell_RebuildRuns | 64   |     115.6 ns |     4.07 ns |     2.70 ns |         - |
| IsOccupied_Scan                     | 64   |   4,209.7 ns |   186.56 ns |   111.02 ns |         - |
| GetCell_Scan                        | 64   |   4,033.9 ns |   228.59 ns |   151.20 ns |         - |
| TryGetRow_Scan                      | 64   |   3,974.3 ns |   126.58 ns |    83.73 ns |         - |
| **GetOccupiedRuns_Cached**              | **256**  |     **494.2 ns** |     **7.41 ns** |     **4.90 ns** |         **-** |
| TryClearCell_TrySetCell_RebuildRuns | 256  |     656.1 ns |    19.30 ns |    11.49 ns |         - |
| IsOccupied_Scan                     | 256  | 124,137.4 ns | 1,280.68 ns |   669.82 ns |         - |
| GetCell_Scan                        | 256  |  65,188.4 ns | 1,777.62 ns | 1,175.79 ns |         - |
| TryGetRow_Scan                      | 256  |  63,142.5 ns | 1,954.48 ns | 1,292.77 ns |         - |

## GridAssemblyBenchmarks

| Method                            | Size | Mean           | Error         | StdDev       | Allocated |
|---------------------------------- |----- |---------------:|--------------:|-------------:|----------:|
| **TryAttach_TryDetach**               | **64**   |   **319,190.4 ns** |   **7,359.74 ns** |  **4,868.01 ns** |   **32208 B** |
| TryGetPlacementAt_Scan            | 64   |   100,632.8 ns |   5,622.71 ns |  3,719.08 ns |         - |
| TryRemoveCell_TryAddCell_Interior | 64   |       500.5 ns |      12.78 ns |      8.46 ns |         - |
| TryRemoveCell_TryAddCell_Seam     | 64   |       699.9 ns |       8.79 ns |      5.81 ns |         - |
| **TryAttach_TryDetach**               | **256**  | **5,383,918.1 ns** | **125,850.17 ns** | **74,891.42 ns** |  **330456 B** |
| TryGetPlacementAt_Scan            | 256  | 1,815,641.0 ns |  78,979.68 ns | 52,240.17 ns |         - |
| TryRemoveCell_TryAddCell_Interior | 256  |     1,899.1 ns |      72.96 ns |     48.26 ns |         - |
| TryRemoveCell_TryAddCell_Seam     | 256  |     2,310.7 ns |     116.52 ns |     77.07 ns |         - |

## GridCompilerBenchmarks

| Method  | Size | ChunkSize | Mean      | Error     | StdDev    | Allocated |
|-------- |----- |---------- |----------:|----------:|----------:|----------:|
| **Compile** | **64**   | **32**        |  **55.19 μs** |  **3.266 μs** |  **2.160 μs** | **114.28 KB** |
| **Compile** | **64**   | **64**        |  **42.24 μs** |  **2.930 μs** |  **1.938 μs** |  **93.23 KB** |
| **Compile** | **256**  | **32**        | **510.79 μs** | **19.282 μs** | **10.085 μs** | **937.03 KB** |
| **Compile** | **256**  | **64**        | **306.80 μs** | **23.948 μs** | **15.840 μs** | **595.37 KB** |

## CompiledGridAssemblyBenchmarks

| Method                              | Size | Fragmented | Mean             | Error          | StdDev         | Allocated |
|------------------------------------ |----- |----------- |-----------------:|---------------:|---------------:|----------:|
| **Commit_AllChunksDirty**               | **64**   | **False**      |    **11,765.612 ns** |    **478.5538 ns** |    **284.7797 ns** |         **-** |
| Commit_OneChunkDirty                | 64   | False      |       876.892 ns |     51.4922 ns |     34.0589 ns |         - |
| Commit_NothingDirty                 | 64   | False      |         8.565 ns |      0.6523 ns |      0.4314 ns |         - |
| TryRemoveCell_TryAddCell_Propagated | 64   | False      |     1,175.320 ns |     53.1676 ns |     35.1671 ns |         - |
| **Commit_AllChunksDirty**               | **64**   | **True**       |    **88,298.359 ns** |  **5,443.0678 ns** |  **3,600.2524 ns** |         **-** |
| Commit_OneChunkDirty                | 64   | True       |     6,229.585 ns |    123.1655 ns |     73.2938 ns |         - |
| Commit_NothingDirty                 | 64   | True       |         9.612 ns |      0.4560 ns |      0.3016 ns |         - |
| TryRemoveCell_TryAddCell_Propagated | 64   | True       |     1,805.689 ns |     70.7145 ns |     46.7733 ns |         - |
| **Commit_AllChunksDirty**               | **256**  | **False**      |   **184,416.575 ns** |  **9,558.4927 ns** |  **5,688.1057 ns** |         **-** |
| Commit_OneChunkDirty                | 256  | False      |       788.243 ns |     60.3433 ns |     39.9134 ns |         - |
| Commit_NothingDirty                 | 256  | False      |         8.413 ns |      0.3486 ns |      0.2074 ns |         - |
| TryRemoveCell_TryAddCell_Propagated | 256  | False      |     1,918.436 ns |     56.8822 ns |     33.8497 ns |         - |
| **Commit_AllChunksDirty**               | **256**  | **True**       | **1,459,640.657 ns** | **76,319.1440 ns** | **45,416.2984 ns** |         **-** |
| Commit_OneChunkDirty                | 256  | True       |     5,970.446 ns |    430.4545 ns |    284.7190 ns |         - |
| Commit_NothingDirty                 | 256  | True       |         9.619 ns |      1.1278 ns |      0.7459 ns |         - |
| TryRemoveCell_TryAddCell_Propagated | 256  | True       |     2,353.191 ns |     25.2697 ns |     13.2165 ns |         - |

## CompiledGridAssemblyMutationBenchmarks

| Method                    | Size | Mean        | Error       | StdDev      | Allocated  |
|-------------------------- |----- |------------:|------------:|------------:|-----------:|
| **Bind**                      | **64**   |    **186.0 μs** |    **16.86 μs** |     **8.82 μs** |   **64.77 KB** |
| ConnectivityBuilder_Build | 64   | 10,011.1 μs | 4,901.05 μs | 3,241.74 μs |   16.77 KB |
| CreateVacuumLayer         | 64   |    179.8 μs |    10.33 μs |     6.15 μs |   16.77 KB |
| **Bind**                      | **256**  |    **905.5 μs** |    **52.66 μs** |    **27.54 μs** | **1033.21 KB** |
| ConnectivityBuilder_Build | 256  | 17,113.0 μs |   320.70 μs |   190.84 μs |   265.2 KB |
| CreateVacuumLayer         | 256  |  2,545.9 μs |   260.44 μs |   172.27 μs |   265.2 KB |

## TopologyLocalityBenchmarks

| Method         | Scale | Workload      | Mean             | Error           | StdDev          | Allocated |
|--------------- |------ |-------------- |-----------------:|----------------:|----------------:|----------:|
| **EditUnobserved** | **16**    | **Chunks**        |       **251.765 ns** |       **5.5172 ns** |       **2.8856 ns** |         **-** |
| EditCompiled   | 16    | Chunks        |       725.738 ns |      14.3506 ns |       7.5056 ns |         - |
| CommitClean    | 16    | Chunks        |         8.573 ns |       0.4258 ns |       0.2534 ns |         - |
| CommitOneDirty | 16    | Chunks        |       719.007 ns |      14.6720 ns |       7.6737 ns |         - |
| CommitAllDirty | 16    | Chunks        |    12,120.412 ns |     737.4470 ns |     438.8429 ns |         - |
| CompileAndBind | 16    | Chunks        |    46,659.182 ns |   3,238.4821 ns |   2,142.0554 ns |  163248 B |
| **EditUnobserved** | **16**    | **Fragmentation** |       **270.246 ns** |      **10.1740 ns** |       **6.7295 ns** |         **-** |
| EditCompiled   | 16    | Fragmentation |       738.842 ns |      53.3080 ns |      35.2599 ns |         - |
| CommitClean    | 16    | Fragmentation |        10.082 ns |       0.5018 ns |       0.3319 ns |         - |
| CommitOneDirty | 16    | Fragmentation |       769.299 ns |      20.8474 ns |      13.7893 ns |         - |
| CommitAllDirty | 16    | Fragmentation |     6,051.967 ns |     276.5237 ns |     182.9033 ns |         - |
| CompileAndBind | 16    | Fragmentation |    31,023.728 ns |   2,520.8407 ns |   1,667.3800 ns |  130432 B |
| **EditUnobserved** | **16**    | **Placements**    |       **258.697 ns** |      **15.0213 ns** |       **9.9356 ns** |         **-** |
| EditCompiled   | 16    | Placements    |       699.712 ns |      29.2246 ns |      19.3303 ns |         - |
| CommitClean    | 16    | Placements    |         9.513 ns |       0.6481 ns |       0.4287 ns |         - |
| CommitOneDirty | 16    | Placements    |       721.478 ns |       8.8754 ns |       4.6420 ns |         - |
| CommitAllDirty | 16    | Placements    |     5,977.214 ns |      92.3156 ns |      61.0611 ns |         - |
| CompileAndBind | 16    | Placements    |   164,270.713 ns |  20,078.7597 ns |  13,280.8563 ns | 1281008 B |
| **EditUnobserved** | **16**    | **Seams**         |       **255.269 ns** |      **11.3269 ns** |       **7.4921 ns** |         **-** |
| EditCompiled   | 16    | Seams         |       878.230 ns |      44.5774 ns |      29.4852 ns |         - |
| CommitClean    | 16    | Seams         |         9.782 ns |       0.5783 ns |       0.3825 ns |         - |
| CommitOneDirty | 16    | Seams         |       804.853 ns |      32.5554 ns |      21.5334 ns |         - |
| CommitAllDirty | 16    | Seams         |   103,639.204 ns |   6,057.0202 ns |   4,006.3438 ns |         - |
| CompileAndBind | 16    | Seams         | 1,981,111.458 ns | 101,690.0054 ns |  67,261.6423 ns | 2408584 B |
| **EditUnobserved** | **256**   | **Chunks**        |       **220.080 ns** |       **8.4788 ns** |       **5.6082 ns** |         **-** |
| EditCompiled   | 256   | Chunks        |       824.831 ns |      29.7204 ns |      19.6582 ns |         - |
| CommitClean    | 256   | Chunks        |         9.821 ns |       0.2495 ns |       0.1485 ns |         - |
| CommitOneDirty | 256   | Chunks        |       846.219 ns |      26.9033 ns |      16.0097 ns |         - |
| CommitAllDirty | 256   | Chunks        |   218,207.967 ns |  13,251.5853 ns |   8,765.1032 ns |         - |
| CompileAndBind | 256   | Chunks        | 1,069,587.422 ns |  74,229.2707 ns |  49,098.0666 ns | 2295365 B |
| **EditUnobserved** | **256**   | **Fragmentation** |       **206.289 ns** |      **13.5063 ns** |       **8.9336 ns** |         **-** |
| EditCompiled   | 256   | Fragmentation |       761.548 ns |      33.8980 ns |      20.1722 ns |         - |
| CommitClean    | 256   | Fragmentation |         8.336 ns |       0.4653 ns |       0.3078 ns |         - |
| CommitOneDirty | 256   | Fragmentation |       757.086 ns |      49.2656 ns |      32.5862 ns |         - |
| CommitAllDirty | 256   | Fragmentation |    44,961.288 ns |     953.1461 ns |     498.5139 ns |         - |
| CompileAndBind | 256   | Fragmentation |   293,592.599 ns |  36,003.2117 ns |  23,813.8954 ns |  622174 B |
| **EditUnobserved** | **256**   | **Placements**    |       **266.369 ns** |       **7.7893 ns** |       **4.6353 ns** |         **-** |
| EditCompiled   | 256   | Placements    |       768.025 ns |      32.8946 ns |      21.7578 ns |         - |
| CommitClean    | 256   | Placements    |         8.078 ns |       0.3488 ns |       0.2307 ns |         - |
| CommitOneDirty | 256   | Placements    |       796.888 ns |      31.4749 ns |      20.8187 ns |         - |
| CommitAllDirty | 256   | Placements    |     6,895.566 ns |     396.6891 ns |     262.3853 ns |         - |
| CompileAndBind | 256   | Placements    |   156,615.204 ns |  12,707.4396 ns |   7,561.9935 ns | 1301240 B |
| **EditUnobserved** | **256**   | **Seams**         |       **701.348 ns** |      **30.6582 ns** |      **18.2442 ns** |         **-** |
| EditCompiled   | 256   | Seams         |     1,223.856 ns |      47.1661 ns |      31.1974 ns |         - |
| CommitClean    | 256   | Seams         |        10.256 ns |       0.3019 ns |       0.1796 ns |         - |
| CommitOneDirty | 256   | Seams         |       849.091 ns |      38.1032 ns |      25.2029 ns |         - |
| CommitAllDirty | 256   | Seams         |   109,254.610 ns |   5,111.5005 ns |   3,380.9411 ns |         - |
| CompileAndBind | 256   | Seams         | 2,886,456.429 ns | 270,227.5840 ns | 178,738.8155 ns | 3522478 B |

## ConnectivityBuilderBenchmarks

| Method            | Size | Mean       | Error     | StdDev    | Allocated |
|------------------ |----- |-----------:|----------:|----------:|----------:|
| **Update_SingleCell** | **64**   |   **1.023 μs** | **0.0453 μs** | **0.0300 μs** |         **-** |
| Update_SeamCell   | 64   |   1,127 μs | 0.0556 μs | 0.0368 μs |         - |
| Update_32x32      | 64   | 107.906 μs | 6.7642 μs | 4.4741 μs |         - |
| **Update_SingleCell** | **256**  |   **1.440 μs** | **0.1060 μs** | **0.0701 μs** |         **-** |
| Update_SeamCell   | 256  |   1.485 μs | 0.0720 μs | 0.0428 μs |         - |
| Update_32x32      | 256  | 105.604 μs | 6.0142 μs | 3.9780 μs |         - |

## DirectionExtensionsBenchmarks

| Method            | Mean     | Error     | StdDev    | Allocated |
|------------------ |---------:|----------:|----------:|----------:|
| ToOffset          | 2.770 ns | 0.1357 ns | 0.0897 ns |         - |
| Opposite          | 2.882 ns | 0.1251 ns | 0.0828 ns |         - |
| TryGetOrthogonals | 1.302 ns | 0.0444 ns | 0.0264 ns |         - |
