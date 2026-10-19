# Performance measurements

## C4 arithmetic follow-up

Measured on 2026-10-19 with the same machine, SDK/runtime, isolated toolchain,
job settings, and fixtures as P1 below. All 21 cases completed with no measured
managed allocation. The interpolation benchmark was renamed with the API.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*EdgeBenchmarks*' '*IntersectBenchmarks*' '*GeometryWorkloadBenchmarks*' '*PointBenchmarks.*Distance*' '*PointBenchmarks.IsAdjacentTo*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-c4-final
```

Public range validation and correct contact/arithmetic handling add approximately
3-4 ns per predicate and 6-8 ns per unique-point query in the batched fixtures.
This is an explicit correctness cost relative to P1, not a performance improvement.
An initial integer quotient/remainder rounding implementation measured
28.6-30.1 ns per unique-point query. Exact integer numerators followed by bounded
double division reduced that to 17.1-18.0 ns without changing regression assertions.
The nearest-even proof is recorded in [CORRECTNESS_PLAN.md](CORRECTNESS_PLAN.md).

Short-run uncertainty and constructor near-zero warnings still apply. Point
distances were not remeasured in P1, so their historical timings are not a matched
before/after comparison. Polygon/factory results below remain P1 snapshots, not
post-C4 measurements; those components have further correctness work pending.

### Batched geometry comparison

All times are ns per edge pair; `Error` and `StdDev` describe C4.

| Method | Workload | P1 mean | C4 mean | Error | StdDev | Allocated |
|---|---|---:|---:|---:|---:|---:|
| HasIntersection | Small | 5.546 | 9.253 | 1.6746 | 0.4349 | - |
| TryFindIntersection | Small | 10.759 | 17.284 | 0.9227 | 0.1428 | - |
| HasIntersection | Screen | 5.569 | 9.161 | 0.8911 | 0.1379 | - |
| TryFindIntersection | Screen | 10.218 | 17.428 | 1.4844 | 0.3855 | - |
| HasIntersection | Boundary | 5.398 | 8.747 | 0.3742 | 0.0579 | - |
| TryFindIntersection | Boundary | 10.562 | 18.048 | 4.4917 | 1.1665 | - |
| HasIntersection | Irregular | 5.538 | 8.938 | 0.3610 | 0.0938 | - |
| TryFindIntersection | Irregular | 10.024 | 17.060 | 1.1787 | 0.1824 | - |

### C4 individual primitives

| Benchmark | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---:|---:|---:|
| Edge.HasIntersection | 7.9260 | 0.3887 | 0.0602 | - |
| Edge.TryFindIntersection | 15.6570 | 0.8849 | 0.2298 | - |
| Edge.GetBoundingBox | 2.7102 | 0.2034 | 0.0528 | - |
| Edge.Interpolate | 5.2409 | 0.2472 | 0.0383 | - |
| Edge.Ctor_PointPoint | 0.1999 | 0.0271 | 0.0070 | - |
| Edge.Ctor_IntIntIntInt | 0.0116 | 0.0470 | 0.0122 | - |
| Intersect.HasIntersection | 9.306 | 0.3911 | 0.1016 | - |
| Intersect.TryFindIntersection | 16.691 | 0.5107 | 0.0790 | - |
| Point.IsAdjacentTo_Orthogonal | 3.457 | 0.5115 | 0.1328 | - |
| Point.IsAdjacentTo_Diagonal | 3.438 | 1.0729 | 0.2786 | - |
| Point.ManhattanDistance | 3.244 | 0.5678 | 0.0879 | - |
| Point.ChebyshevDistance | 3.148 | 0.0707 | 0.0109 | - |
| Point.DistanceSquared | 3.323 | 1.9018 | 0.4939 | - |

## P1 baseline

Measured on 2026-10-19 against production code at `eb28f3b`, with the P1
benchmark repairs. These are exploratory measurements, not performance targets.
`Error` is half the 99.9% confidence interval; `Allocated` is managed allocation
per operation (`KB` = 1024 bytes). A dash means no measured allocation.

```text
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.9448/24H2/2024Update/HudsonValley)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 11.0.100-rc.2.26504.105
Host and child processes: .NET 10.0.13, X64 RyuJIT x86-64-v3
Default isolated-process toolchain
IterationCount=5  IterationTime=100ms  LaunchCount=1  WarmupCount=3
```

Reproduction command (after a Release build):

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*EdgeBenchmarks*' '*IntersectBenchmarks*' '*RectangleBenchmarks*' '*GeometryWorkloadBenchmarks*' '*VoronoiWorkloadBenchmarks*' '*CompiledGridAssemblyBenchmarks*' '*PolygonBenchmarks*' '*SimpleQuadTreeNodeBenchmarks*' '*D3DelaunayFactoryBenchmarks*' '*D3VoronoiFactoryBenchmarks*' '*MapboxDelaunatorFactoryBenchmarks*' '*PointBenchmarks.Ctor*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-p1
```

The run attempted 70 cases: 69 produced measurements; the high-degree Voronoi
fixture failed setup with the known C1 buffer defect. The runner correctly exited
with code 1. That fixture has **no valid performance result** until C1 is fixed.
Setup checks in the new workloads validate known intersections, site containment,
and topology run counts outside the timed operation.

The old discarded-output measurements are withdrawn, including the old Edge,
Intersect, and Rect near-zero results. Old continuously growing tree insertion
results are also withdrawn. Do not calculate speedups against those measurements
or across the changed runtime/toolchain. Use matched before/after runs instead.

Short iterations produced minimum-iteration-time warnings, and several results
have wide confidence intervals. Repeat longer runs before deciding marginal
performance changes. Existing polygon/tree scenarios measure current behavior,
not proof that the pending correctness defects have been repaired.

Four suspicious primitive cases were also run with the same job settings and
`--disasm`, filtered to `*EdgeBenchmarks.Ctor*`, `*PointBenchmarks.Ctor*`, and
`*RectangleBenchmarks.Contains_IntInt*`. Generated code retains field loads and
returned values for the constructors (31 bytes per Edge overload, 14 for Point),
and all four bounds comparisons for Rect containment (39 bytes). Nevertheless,
their overhead-subtracted near-zero durations cannot resolve useful differences;
they are not evidence of free work. Prefer the batched geometry workloads for
predicate comparisons. Raw reports and disassembly are under the ignored
`BenchmarkDotNet.Artifacts\correctness-p1*` directories.

The previous report remains in commit `eb28f3b` (`git show eb28f3b:BENCHMARK.md`).
Poisson-disc generation, random generation, noisy edges, remaining Point methods,
and other topology classes were not remeasured by this focused P1 run. Their
historical tables are not current baselines. The old environment was Windows build
26100.8893, SDK 10.0.400-preview.0.26322.102, .NET 10.0.10,
MediumRun/InProcessNoEmit, 15 measurement iterations, 10 warmups, and one launch.

## D3DelaunayFactory

| Method | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---:|---:|---:|---:|
| Create_100x100 | 70.76 | 12.19 | 3.166 | 89.98 KB |
| Create_500x500 | 3,880.33 | 891.42 | 137.948 | 2251.7 KB |
| Create_1000x1000 | 20,250.93 | 5,494.61 | 850.296 | 9029.05 KB |

## D3VoronoiFactory

| Method | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---:|---:|---:|---:|
| Create_100x100 | 332.7 | 41.96 | 10.90 | 399.3 KB |
| Create_500x500 | 16,351.8 | 5,068.61 | 1,316.30 | 11429.51 KB |
| Create_1000x1000 | 74,729.2 | 11,630.56 | 3,020.42 | 47033.84 KB |

## MapboxDelaunatorFactory

| Method | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---:|---:|---:|---:|
| Create_100x100 | 60.37 | 1.860 | 0.288 | 47.63 KB |
| Create_500x500 | 2,928.71 | 125.407 | 32.568 | 1168.28 KB |
| Create_1000x1000 | 15,843.81 | 2,274.603 | 590.707 | 4674.97 KB |

## Voronoi workloads

Grid and jittered-grid fixtures each contain 400 sites. Jitter uses seed 7291.
HighDegree uses the origin and 40 integer-rounded sites on a radius-10,000 ring;
it currently throws `IndexOutOfRangeException` during cell traversal.

| Method | Workload | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---|---:|---:|---:|---:|
| Create | Grid | 394.4 | 169.10 | 26.17 | 399.3 KB |
| Create | JitteredGrid | 498.7 | 42.48 | 11.03 | 578.29 KB |
| Create | HighDegree | blocked by C1 | - | - | - |

## Edge

| Method | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---:|---:|---:|
| HasIntersection | 4.7857 | 0.4266 | 0.1108 | - |
| TryFindIntersection | 8.5679 | 0.8906 | 0.2313 | - |
| GetBoundingBox | 1.4390 | 0.1788 | 0.0464 | - |
| GetMidpoint | 3.7416 | 0.8640 | 0.2244 | - |
| Ctor_PointPoint | 0.0853 | 0.3048 | 0.0792 | - |
| Ctor_IntIntIntInt | 0.2970 | 0.3040 | 0.0790 | - |

## Intersect

| Method | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---:|---:|---:|
| HasIntersection | 5.909 | 1.0779 | 0.2799 | - |
| TryFindIntersection | 9.638 | 0.9537 | 0.2477 | - |

## Batched geometry workloads

Each invocation processes 256 edge pairs; times and allocations are per pair
using `OperationsPerInvoke = 256`, including loop and input-access overhead.
The returned totals consume both intersection existence and coordinates.
Small spans four units, Screen spans 0..1919, Boundary spans -32768..32767,
and Irregular varies position and span using seed 7291. All pairs intersect.
These fixtures deliberately use slender crossings with independently known
integer intersections; Boundary is not an arithmetic-overflow stress test.
C4 adds the large-determinant correctness regressions before widening arithmetic.

| Method | Workload | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---|---:|---:|---:|---:|
| HasIntersection | Small | 5.546 | 0.4746 | 0.1233 | - |
| TryFindIntersection | Small | 10.759 | 0.7221 | 0.1875 | - |
| HasIntersection | Screen | 5.569 | 0.6858 | 0.1781 | - |
| TryFindIntersection | Screen | 10.218 | 1.1312 | 0.2938 | - |
| HasIntersection | Boundary | 5.398 | 0.1521 | 0.0235 | - |
| TryFindIntersection | Boundary | 10.562 | 0.4948 | 0.0766 | - |
| HasIntersection | Irregular | 5.538 | 0.2042 | 0.0530 | - |
| TryFindIntersection | Irregular | 10.024 | 0.7059 | 0.1833 | - |

## Polygon

| Method | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---:|---:|---:|
| Ctor | 42.66 | 7.623 | 1.980 | 208 B |
| Contains_Polygon | 27.97 | 3.944 | 1.024 | - |
| HasOverlap | 27.34 | 2.599 | 0.675 | - |
| IsEquivalentTo | 30.57 | 2.807 | 0.729 | - |
| TryIntersect_WithIntersection | 711.54 | 48.534 | 7.511 | 1128 B |
| TryIntersect_WithoutIntersection | 80.92 | 7.600 | 1.974 | 24 B |
| TryFindIntersections_Edge_WithIntersections | 52.26 | 4.028 | 0.623 | 88 B |
| TryFindIntersections_Edge_WithoutIntersections | 16.60 | 5.631 | 0.871 | - |
| TryFindIntersections_Polygon_WithIntersections | 131.35 | 38.905 | 10.103 | 88 B |
| TryFindIntersections_Polygon_WithoutIntersections | 70.36 | 3.513 | 0.912 | - |
| HasIntersection_Edge | 11.09 | 0.357 | 0.093 | - |
| HasIntersection_Polygon | 58.12 | 12.085 | 3.138 | - |
| HasIntersection_EdgeArray | 51.28 | 0.563 | 0.146 | - |

## Rect

| Method | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---:|---:|---:|
| Contains_Rect | 0.8689 | 0.1610 | 0.0418 | - |
| Contains_IRect | 2.8042 | 0.4863 | 0.1263 | - |
| Contains_Point | 0.7875 | 0.0654 | 0.0101 | - |
| Contains_IntInt | 0.0875 | 0.2893 | 0.0751 | - |
| IsEquivalentTo_Rect | 3.4651 | 0.8594 | 0.2232 | - |
| IsEquivalentTo_IRect | 4.3529 | 0.2848 | 0.0740 | - |
| IsEquivalentTo_PointPoint | 1.4631 | 0.1268 | 0.0196 | - |
| IsEquivalentTo_IntIntIntInt | 1.5335 | 0.2699 | 0.0701 | - |

## SimpleQuadTreeNode

Insertion reconstructs the same seeded 10,000-item tree outside each measured
iteration. BenchmarkDotNet confirmed `InvocationCount=1` and `UnrollFactor=1`;
insertion no longer accumulates state. Single-operation iterations are too short
for reliable small timing comparisons and include cold-state effects. Other
methods use the default invocation count and unroll factor 16.

| Method | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---:|---:|---:|---:|
| Insert | 8.550 | 3.5394 | 0.5477 | 200 B |
| Count | 111.718 | 59.6410 | 15.4886 | - |
| GetSubTreeContents | 663.606 | 218.2108 | 56.6686 | 786746 B |
| Query | 3.962 | 0.9143 | 0.2375 | 22928 B |

## Point constructor

| Method | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---:|---:|---:|
| Ctor | 0.0569 | 0.2234 | 0.0580 | - |

This measurement is indistinguishable from harness overhead; see the
disassembly qualification above.

## CompiledGridAssembly

Four placements with either dense occupancy or checkerboard holes. The fragmented
case deliberately increases run count, not just grid area. All cases measured no
managed allocation. Locality changes still need P3's independent scaling of
unrelated placements/runs; this table alone does not establish asymptotic cost.

| Method | Size | Fragmented | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---|---:|---:|---:|---:|
| Commit_AllChunksDirty | 64 | False | 12,432.0 | 1,119.5 | 173.24 | - |
| Commit_OneChunkDirty | 64 | False | 2,264.6 | 189.8 | 29.37 | - |
| Commit_NothingDirty | 64 | False | 924.6 | 270.2 | 70.18 | - |
| TryRemoveCell_TryAddCell_Propagated | 64 | False | 2,379.4 | 434.8 | 112.92 | - |
| Commit_AllChunksDirty | 64 | True | 100,532.5 | 9,422.7 | 1,458.18 | - |
| Commit_OneChunkDirty | 64 | True | 29,620.0 | 6,673.9 | 1,733.19 | - |
| Commit_NothingDirty | 64 | True | 17,829.9 | 3,377.8 | 877.21 | - |
| TryRemoveCell_TryAddCell_Propagated | 64 | True | 29,441.5 | 6,015.1 | 1,562.11 | - |
| Commit_AllChunksDirty | 256 | False | 220,999.8 | 50,427.2 | 7,803.66 | - |
| Commit_OneChunkDirty | 256 | False | 24,593.5 | 1,310.2 | 202.75 | - |
| Commit_NothingDirty | 256 | False | 17,218.8 | 1,244.0 | 323.07 | - |
| TryRemoveCell_TryAddCell_Propagated | 256 | False | 18,953.7 | 4,357.9 | 674.39 | - |
| Commit_AllChunksDirty | 256 | True | 1,988,864.6 | 31,714.6 | 4,907.87 | - |
| Commit_OneChunkDirty | 256 | True | 427,008.0 | 34,529.5 | 8,967.20 | - |
| Commit_NothingDirty | 256 | True | 302,327.2 | 45,826.1 | 11,900.89 | - |
| TryRemoveCell_TryAddCell_Propagated | 256 | True | 541,991.5 | 241,090.3 | 62,610.37 | - |
