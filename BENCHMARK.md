# Performance measurements

## P2 allocation and representation boundaries

Measured against pre-P2 production at `61fcfe5`, in the same .NET 10.0.13,
SDK 11 preview, BenchmarkDotNet 0.15.8, i7-9700K environment as the preceding
measurements. The before run completed 35 cases; the after run completed those
35 plus three visitor cases. Setup checks and seeded fixtures remain enabled.
Times are short-run measurements, not guaranteed speedups.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*TriangleRepresentationBenchmarks*' '*SearchableVoronoiBenchmarks*' '*MapboxDelaunatorFactoryBenchmarks*' '*D3DelaunayFactoryBenchmarks*' '*VoronoiWorkloadBenchmarks*' '*SimpleQuadTreeNodeBenchmarks.Query*' '*IntegerRasterizerBenchmarks*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\performance-p2-after
```

The matched run used triangle counts 16 and 4,096. The committed fixture also
includes 2,048 and 16,384, added for the subsequent size sweep; running the full
command now includes those extra 12 cases.

### Queries

Times are nanoseconds; allocation is per operation after warmup. Error is half
the 99.9% confidence interval. Visitor cases are new APIs, not before/after
comparisons with equivalent materialization.

| Operation | Before mean | After mean | After error | After StdDev | Before allocation | After allocation |
|---|---:|---:|---:|---:|---:|---:|
| Seeded 10,000-item tree query | 4869 | 1582 | 134.4 | 34.9 | 23.34 KB | 776 B |
| Tree counting visitor | - | 1406 | 342.0 | 88.8 | - | 0 B |
| Voronoi search, 20x20 | 777.1 | 273.0 | 65.76 | 17.08 | 3.45 KB | 88 B |
| Voronoi counting visitor, 20x20 | - | 215.6 | 21.84 | 5.67 | - | 0 B |
| Voronoi search, 200x200 | 11090.8 | 7719.9 | 628.31 | 163.17 | 29.02 KB | 8384 B |
| Voronoi counting visitor, 200x200 | - | 2731.1 | 138.59 | 35.99 | - | 0 B |

There is one growing result list per materialized query, with no recursive
lists or per-candidate boxing. Search directly projects indexed cell references;
it no longer materializes rectangles or uses a bounds-to-cell dictionary.
This also fixes distinct cells with equal bounds being returned as duplicates
of the last cell. Generic/ref-struct visitors can write into caller-owned spans.

### Rasterizers

Times are microseconds. Every after case reports zero managed allocation.

| Operation | Before mean | After mean | After error | After StdDev | Before allocation |
|---|---:|---:|---:|---:|---:|
| Small raw fill, span sink | 0.1944 | 0.2349 | 0.05099 | 0.01324 | 0 B |
| Small validated fill, span sink | 0.1425 | 0.1468 | 0.02466 | 0.00640 | 0 B |
| Polygon fill, scalar sink | 702.8405 | 348.5144 | 13.81973 | 3.58894 | 29792 B |
| Polygon fill, span sink | 94.3312 | 86.7085 | 2.02079 | 0.52479 | 0 B |
| Triangle fill, scalar sink | 327.0646 | 200.7778 | 6.45518 | 1.67639 | 30752 B |
| Triangle fill, span sink | 56.5977 | 56.6419 | 7.81448 | 1.20930 | 0 B |
| Outline, scalar sink | 5.2581 | 5.2771 | 0.58262 | 0.15130 | 0 B |
| Outline, span sink | 5.3787 | 5.4333 | 0.62853 | 0.16323 | 0 B |

The required explicit span callback removes default-interface boxing. Scalar
sinks use the shared generic emitter, while optimized whole-span sinks retain
their implementations. The small raw-fill increase is recorded rather than
claimed as an improvement. Unit tests separately verify zero warm allocation
for the delegate adapter.

### Triangulation and public diagrams

Times are microseconds. Both duplicate-validation settings are measured for the
raw kernel. Allocation uses 1 KB = 1,024 B.

| Operation | Before mean | After mean | After error | After StdDev | Before allocation | After allocation |
|---|---:|---:|---:|---:|---:|---:|
| Kernel 100x100, no duplicate check | 61.91 | 64.69 | 2.614 | 0.679 | 47.63 KB | 19.05 KB |
| Kernel 500x500, no duplicate check | 3050.68 | 2955.29 | 142.971 | 37.129 | 1168.43 KB | 470.30 KB |
| Kernel 1000x1000, no duplicate check | 15929.09 | 13779.61 | 458.268 | 119.011 | 4677.41 KB | 1878.12 KB |
| Kernel 100x100, duplicate check | 64.96 | 74.03 | 9.320 | 1.442 | 47.63 KB | 19.05 KB |
| Kernel 500x500, duplicate check | 3183.40 | 3038.69 | 216.443 | 33.495 | 1168.43 KB | 470.30 KB |
| Kernel 1000x1000, duplicate check | 16188.88 | 14186.10 | 809.693 | 210.275 | 4674.97 KB | 1878.12 KB |
| Delaunay 100x100 | 74.60 | 78.35 | 4.681 | 1.216 | 91.71 KB | 46.21 KB |
| Delaunay 500x500 | 4288.72 | 3855.38 | 1775.962 | 274.832 | 2324.57 KB | 1167.76 KB |
| Delaunay 1000x1000 | 21293.89 | 16097.14 | 1464.339 | 226.608 | 9333.48 KB | 4678.88 KB |
| Voronoi grid | 458.38 | 376.53 | 27.103 | 4.194 | 374.69 KB | 346.11 KB |
| Voronoi jittered grid | 565.77 | 571.98 | 87.888 | 22.824 | 515.68 KB | 485.67 KB |
| Voronoi high degree | 33.16 | 31.54 | 1.455 | 0.378 | 56.73 KB | 52.59 KB |

Small triangulations measured slower; pooling and span-based scratch are not
claimed to be free. The substantial allocation reduction, simpler result
ownership, and favorable large-workload results justify the change, but the
wide Delaunay 500x500 interval does not establish a precise speedup.

Seven temporary arrays become a 512-int stack buffer plus one partitioned
integer rental and one double rental. Only exact logical spans are used.
Results own both capacity-sized mesh arrays and the exact hull array; no pool
lease escapes. Logical mesh spans avoid two trimming allocations and copies.
For an all-hull 32-site mesh, each capacity array has 177 entries versus 90
logical entries: nearly twice the retained mesh payload, although construction
allocates less. Public Delaunay/Voronoi factories discard this temporary mesh
after building their public results. Shared pool caches also retain memory, so
these allocation figures are not measurements of process working set.

### Triangle representation and size sensitivity

The after run compares production `Triangle` with a benchmark-local immutable
reference record. A follow-up completed 24 cases using the same short-job
settings and `--filter '*TriangleRepresentationBenchmarks*'`, with artifacts
under `performance-p2-triangle-sizes`. The following table is from that single
follow-up, not a mixture of runs. Times are microseconds; copying is into
preallocated arrays and neither traversal nor copying allocates.

| Count | Reference construction | Value construction | Reference traversal | Value traversal | Reference copy | Value copy | Reference allocation | Value allocation |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 16 | 0.0956 | 0.0421 | 0.0104 | 0.0089 | 0.0104 | 0.0095 | 792 B | 408 B |
| 2048 | 15.2173 | 5.1667 | 1.6274 | 1.3168 | 0.4069 | 1.0205 | 98328 B | 49176 B |
| 4096 | 35.4728 | 51.7054 | 3.8344 | 2.7947 | 1.0527 | 2.5751 | 196632 B | 98338 B |
| 16384 | 921.5807 | 62.4428 | 17.3105 | 12.1450 | 3.9092 | 12.0764 | 786467 B | 393249 B |

Value triangles halve per-element storage and improve traversal, but full-array
copies move 24-byte payloads instead of 8-byte references. At 4,096 entries the
value array's 98,304-byte payload exceeds the default 85,000-byte large-object
threshold, while the reference-slot array does not. At 2,048 both arrays are
below it; at 16,384 both are above it. These size effects are consistent with
the construction crossover, not proof that GC placement alone explains every
timing difference.

The 16,384-reference construction result is especially noisy: mean 921.581 us,
99.9% error 629.835 us, StdDev 163.566 us, versus value mean 62.443 us, error
5.810 us, StdDev 1.509 us. Do not extrapolate a universal 15x advantage.
The earlier matched after run also showed the 4,096 construction penalty
(27.577 us reference versus 43.314 us value) and roughly doubled copy cost.
The value representation is accepted for owned compact immutable geometry and
lower per-triangle allocation, not as a claim that every operation is faster.

Topology compilation storage is unchanged in P2; locality/index maintenance and
their memory/build costs remain P3 work.

## C7/A1 topology ownership and lifetime

Measured on 2026-10-19 against pre-C7 production at `0c850b7`, using the same
environment and short-run settings as P1. All 28 matched cases completed before
and after. Existing seeded fixtures and workloads are unchanged.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*GridAssemblyBenchmarks.TryAttach_TryDetach*' '*GridAssemblyBenchmarks.TryRemoveCell_TryAddCell_Interior*' '*CompiledGridAssemblyBenchmarks.TryRemoveCell_TryAddCell_Propagated*' '*ConnectivityBuilderBenchmarks*' '*GridCompilerBenchmarks*' '*DenseGridSourceBenchmarks*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-c7-after
```

### Attachment, edits, and connectivity

All times below are microseconds. Allocation remains zero for the edit/update
rows after warmup. Attachment/detachment rises by 32 B per pair in both fixtures:
32,176 -> 32,208 B at size 64, and 330,424 -> 330,456 B at size 256.
Ownership is one stable object per source, not a per-edit allocation or global
registry. Read-only views are created per owner/snapshot, not per cell.

| Method | Size | Fragmented | Before mean (us) | C7 mean (us) | C7 error (us) | C7 StdDev (us) |
|---|---:|---|---:|---:|---:|---:|
| Attach/detach | 64 | - | 262.4169 | 589.7755 | 1943.15566 | 300.70549 |
| Attach/detach | 256 | - | 5409.0250 | 5026.1850 | 751.62662 | 195.19505 |
| Interior remove/add | 64 | - | 0.4567 | 0.4910 | 0.02036 | 0.00315 |
| Interior remove/add | 256 | - | 1.6969 | 1.8625 | 0.35868 | 0.09315 |
| Propagated remove/add | 64 | false | 2.404 | 2.502 | 0.5095 | 0.1323 |
| Propagated remove/add | 64 | true | 27.348 | 26.551 | 5.6009 | 1.4545 |
| Propagated remove/add | 256 | false | 17.672 | 18.418 | 3.9880 | 1.0357 |
| Propagated remove/add | 256 | true | 474.377 | 413.671 | 79.9610 | 20.7656 |
| Update single cell | 64 | - | 0.8872 | 1.118 | 0.2999 | 0.0779 |
| Update seam cell | 64 | - | 1.0051 | 1.282 | 0.2100 | 0.0545 |
| Update 32x32 | 64 | - | 86.5445 | 140.319 | 200.4756 | 52.0629 |
| Update single cell | 256 | - | 1.3495 | 1.423 | 0.0564 | 0.0147 |
| Update seam cell | 256 | - | 1.5752 | 1.411 | 0.0804 | 0.0124 |
| Update 32x32 | 256 | - | 258.4342 | 134.933 | 122.9671 | 31.9342 |

These short runs are visibly noisy: post-change size-64 attachment has a
456.291 us median and a confidence interval much wider than its mean; pre-change
size-256 32x32 update has a 99.873 us median versus its 258.434 us mean. Do not
interpret those means as reliable regression/speedup ratios. Some small edits
and updates do have higher measured costs; ownership and full-int neighbour/
padding validation are not claimed to be free. Existing full run/seam scans
remain, and the fragmented propagation results still justify P3 locality work.

### Compilation

The read-only layout and layer-list views add small fixed allocations rather
than per-cell wrappers. Most rows show about 64 B more per compilation; the
largest row's rounded measurement does not show that difference.

| Source size | Chunk size | Before mean (us) | C7 mean (us) | C7 error (us) | C7 StdDev (us) | Before allocation | C7 allocation |
|---|---:|---:|---:|---:|---:|---:|---:|
| 64 | 32 | 35.33 | 39.89 | 4.453 | 1.156 | 73.14 KB | 73.20 KB |
| 64 | 64 | 25.82 | 41.91 | 6.749 | 1.753 | 56.35 KB | 56.41 KB |
| 256 | 32 | 387.22 | 401.34 | 30.317 | 7.873 | 675.86 KB | 675.86 KB |
| 256 | 64 | 199.31 | 207.85 | 11.991 | 1.856 | 401.29 KB | 401.35 KB |

No throughput improvement is claimed. The small-assembly/chunk-64 timing increase
is retained here rather than attributed entirely to validation or dismissed.
Compiler representation/allocation work remains P2.

### Dense sources and other layer paths

All ten dense-source cases still report zero steady-state managed allocation.
Cached-run means change from 121.4 -> 122.5 ns (size 64) and 463.4 -> 473.1 ns
(size 256). Clear/set with run rebuilding changes from 115.3 -> 114.5 ns and
653.5 -> 663.1 ns respectively. Existing-cell access does not acquire ownership
or add per-cell objects.

An additional run under `correctness-c7-layers`, with filters
`*CompiledGridAssemblyMutationBenchmarks*` and
`*CompiledGridAssemblyBenchmarks.Commit*`, completed all 18 cases covering bind,
connectivity build, vacuum generation, and clean/one-dirty/all-dirty commits.
These are coverage measurements, not a new matched before/after comparison.
Public chunk-value access remains span-based; padding copies use caller-owned
scratch. Correctness tests separately verify these APIs after removal of mutable
layout and raw layer-storage exposure.

## C6 rasterizer follow-up

Measured on 2026-10-19 against pre-C6 production at `b5a4cfc`, using the same
environment and short-run settings as P1. Two new small-square cases isolate raw
validation from the already-validated polygon path. Polygon construction is
outside measurement; untimed setup checks every buffer pixel against an
independent inclusive-square reference.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*IntegerRasterizerBenchmarks*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-c6-final
```

All eight cases complete. Raw fills now share the allocation-free bounded
polygon normalizer; validated polygons bypass it. Coordinates are checked at
raw/line entry points, not within the Bresenham pixel loop. The larger short-run
timing changes warranted a second run, under `correctness-c6-steady`, with
`--iterationCount 8 --warmupCount 5 --iterationTime 500`. No code changed between
these two C6 runs.

| Method | Before mean (ns) | C6 short mean (ns) | C6 longer mean (ns) | Longer error (ns) | Longer StdDev (ns) | Allocation before / after |
|---|---:|---:|---:|---:|---:|---:|
| FillSmallRaw_PerSpan | 124.9 | 218.3 | 225.4 | 16.53 | 8.64 | 0 / 0 B |
| FillSmallValidated_PerSpan | 143.1 | 135.3 | 151.8 | 9.00 | 4.70 | 0 / 0 B |
| FillPolygon_PerPixel | 482565.4 | 738948.3 | 476885.0 | 20129.20 | 10527.96 | 29792 / 29792 B |
| FillPolygon_PerSpan | 86084.6 | 107102.2 | 97537.2 | 8872.24 | 4640.35 | 0 / 0 B |
| FillTriangle_PerPixel | 323036.0 | 296817.1 | 276643.9 | 16533.26 | 7340.87 | 30752 / 30752 B |
| FillTriangle_PerSpan | 53811.5 | 61132.5 | 60726.6 | 5559.17 | 2907.55 | 0 / 0 B |
| Outline_PerPixel | 4214.6 | 5496.1 | 5553.6 | 212.57 | 111.18 | 0 / 0 B |
| Outline_PerSpan | 5231.0 | 5530.9 | 4840.9 | 271.14 | 141.81 | 0 / 0 B |

The longer run did not reproduce the initial polygon per-pixel increase, so the
short measurement cannot establish a 53% kernel regression. It is not a paired
long-run baseline, however: several C6 cases remain slower than the original
short baseline, especially per-pixel outlines. Do not attribute all differences
to validation or claim a general speedup. The small-square comparison consistently
shows roughly 75-85 ns extra for raw validation versus an already-owned polygon.
Callers repeatedly rendering a shape should construct its polygon once.

Span-aware sinks and outlines report no managed allocation after pool warmup;
the validator adds no owned arrays or boxed helper. The per-pixel fill cases
retain their pre-existing default-interface allocation (32 B per emitted row for
these fixtures). A struct sink alone is therefore not an allocation-free promise.
That measured boundary cost is recorded for P2 rather than changing the sink
contract as part of C6.

## C2 quadtree follow-up

Measured on 2026-10-19 against the pre-C2 production implementation at `e9cdf35`,
with the same environment and short-run settings as P1 below. All four cases
complete. The seeded 10,000-entry input and query are unchanged; new untimed setup
checks entry multiplicity, total contents, and query results against an independent
inclusive-bounds reference.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*SimpleQuadTreeNodeBenchmarks*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-c2-final
```

Subtree occupancy is now cached rather than recursively counted. This both fixes
the populated-leaf emptiness error and avoids adding recursive scans to queries.
Knowing the complete count also permits one exact-sized result allocation for
`GetSubTreeContents`, rather than repeatedly growing its list.

| Method | Before mean (us) | C2 mean (us) | C2 error (us) | C2 StdDev (us) | Before allocation | C2 allocation |
|---|---:|---:|---:|---:|---:|---:|
| Insert | 5.620 | 8.6500 | 24.2102 | 3.7466 | 200 B | 200 B |
| Count | 86.138 | below resolution | - | - | - | - |
| GetSubTreeContents | 764.524 | 275.0210 | 40.7958 | 6.3132 | 786746 B | 240104 B |
| Query | 4.986 | 4.3014 | 0.4222 | 0.0653 | 22808 B | 23904 B |

Insertion still resets its seeded tree outside timing and runs one invocation per
iteration. Its very short iteration and wide confidence interval cannot establish
a small regression or improvement. Query now traverses corrected disjoint child
bounds; the old query implementation was known to miss entries. Do not treat its
timing/allocation as an equivalent correct-work baseline or preserve its incorrect
pruning to save allocations. Recursive result lists and interface-based item
access still warrant the separate P2 query-path work.

The first C2 measurement exposed 40 B of extra insertion boxing from a generic
null check, and additional query boxing of value-type node bounds. A non-boxing
null test and a concrete internal query rectangle removed those introduced costs
without changing the public generic query interface. The final numbers above
retain the remaining costs rather than claiming allocation-free queries.

Count was also measured with `--disasm` and filter
`*SimpleQuadTreeNodeBenchmarks.Count`, under
`BenchmarkDotNet.Artifacts\correctness-c2-count`. Its generated code is eight
bytes: two field loads and a return. The benchmark reported `ZeroMeasurement`;
this establishes retained work and constant-time access, not a literal zero-cost
operation or a meaningful numeric speedup ratio.

## C1 diagram follow-up

Measured on 2026-10-19 with the same environment and short-run job settings as
P1 below. All 12 selected cases complete, including the previously failing
HighDegree Voronoi workload. These measurements include validated sites,
read-only result ownership, bounded cell scratch, and unrounded circumcentres.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*VoronoiWorkloadBenchmarks*' '*D3DelaunayFactoryBenchmarks*' '*MapboxDelaunatorFactoryBenchmarks*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-c1
```

### Voronoi workloads

The before column is the C3 run with identical sites and job settings. HighDegree
has no valid pre-fix timing. New validation has a cost, and short-run intervals
overlap; these results do not establish a precise throughput regression.
Per-diagram bounded quantization scratch replaces per-cell temporary lists,
reducing allocation even with the new owned site snapshot and read-only wrappers.
Deferring intermediate rounding intentionally changes some output vertices, so
these are correctness-cost comparisons, not interchangeable-algorithm speedups.

| Workload | C3 mean (us) | C1 mean (us) | C1 error (us) | C1 StdDev (us) | C3 allocation | C1 allocation |
|---|---:|---:|---:|---:|---:|---:|
| Grid | 337.8 | 365.26 | 46.854 | 7.251 | 402.42 KB | 374.69 KB |
| JitteredGrid | 587.4 | 646.12 | 138.313 | 35.919 | 575.05 KB | 515.68 KB |
| HighDegree | failed | 33.81 | 4.452 | 0.689 | unavailable | 56.73 KB |

### Isolated duplicate-validation cost

The internal triangulation benchmark now runs with and without duplicate
rejection on the same already-valid coordinates. Untimed setup verifies identical
triangle, half-edge, and hull sequences. This isolates the duplicate check, not
all public-boundary validation or snapshot costs. Its sorted exact coordinate
keys reuse the existing distance array; no additional per-site buffer/hash set
is introduced on the default factory path.

Short-run means add about 5 us, 0.30 ms, and 1.62 ms at 400, 10,000, and 40,000
sites respectively (roughly 9-11%); larger-case uncertainty is substantial.
Managed allocation is similar, but an exact zero-delta claim is not supported.
A trial whole-kernel thread-allocation assertion gave deltas of +6,080 and -2,080
bytes at 40,000 sites in successive runs. No stable exact budget was established,
so that assertion was not retained; the benchmark measurements below are used
instead, as required by the plan.

| Sites | Reject duplicates | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---|---:|---:|---:|---:|
| 400 | false | 58.70 | 2.196 | 0.340 | 47.63 KB |
| 400 | true | 63.82 | 3.388 | 0.880 | 47.63 KB |
| 10000 | false | 3087.41 | 163.110 | 42.359 | 1167.95 KB |
| 10000 | true | 3386.04 | 993.691 | 153.775 | 1168.43 KB |
| 40000 | false | 14909.03 | 2611.298 | 404.101 | 4674.95 KB |
| 40000 | true | 16528.53 | 1366.625 | 354.908 | 4674.97 KB |

### Complete Delaunay factory

Current validated/owned-result snapshots; no new matched pre-C1 factory run was
made. Grid dimensions 100, 500, and 1000 correspond to the site counts below at
spacing five.

| Sites | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---:|---:|---:|---:|
| 400 | 80.54 | 7.252 | 1.883 | 91.71 KB |
| 10000 | 4128.98 | 1629.495 | 252.166 | 2324.63 KB |
| 40000 | 20757.48 | 3157.062 | 488.559 | 9333.48 KB |

## C3 polygon follow-up

Measured on 2026-10-19 with the same environment and job settings as P1 below.
The matched polygon baseline uses production at `a70d952` (after C4/C5); the
final run completes all 13 polygon cases and both selected Voronoi workloads.
Setup independently checks known polygon vertices/predicates and diagram site
containment outside the timed operation. HighDegree remains a C1 defect and is
not included in this selection.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*PolygonBenchmarks*' '*VoronoiWorkloadBenchmarks*Grid*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-c3-final
```

Construction now validates convexity and owns a stable vertex snapshot: about
137 ns versus 45 ns, and 216 B versus 208 B. This once-per-construction cost buys
the invariant reused by subsequent operations. Clipping builds owned edges once
and uses bounded stack scratch: 1,136 B falls to 264 B for the six-corner result;
disjoint clipping drops from 32 B to no measured allocation. The initial C3 hull
sort allocated 88 B per call through a struct comparer; the final static
comparison delegate removes that per-call overhead.

These are not equivalent algorithms: containment now checks the entire target,
intersection includes containment/contact, and boundary queries deduplicate
rounded points. Do not interpret the timing differences as interchangeable-work
speedups. Short iterations and wide confidence intervals still limit conclusions
about small timing differences.

### Polygon comparison

Times are ns; error and standard deviation describe C3. Boundary-query rows use
the new `TryFindBoundaryIntersections` name; the baseline used
`TryFindIntersections` with the same inputs.

| Method | Before mean | C3 mean | Error | StdDev | Before allocation | C3 allocation |
|---|---:|---:|---:|---:|---:|---:|
| Ctor | 44.97 | 136.919 | 10.4299 | 1.6140 | 208 B | 216 B |
| Contains_Polygon | 30.08 | 23.297 | 1.8635 | 0.4839 | - | - |
| HasOverlap | 28.57 | 9.854 | 0.5423 | 0.1408 | - | - |
| IsEquivalentTo | 29.57 | 13.854 | 1.7874 | 0.4642 | - | - |
| TryIntersect_WithIntersection | 791.61 | 427.734 | 63.4212 | 16.4703 | 1136 B | 264 B |
| TryIntersect_WithoutIntersection | 202.07 | 159.034 | 13.0970 | 3.4012 | 32 B | - |
| TryFindBoundaryIntersections_Edge_WithIntersections | 86.47 | 73.418 | 2.7673 | 0.4282 | 88 B | 64 B |
| TryFindBoundaryIntersections_Edge_WithoutIntersections | 47.80 | 23.699 | 1.0361 | 0.2691 | - | - |
| TryFindBoundaryIntersections_Polygon_WithIntersections | 253.92 | 156.570 | 20.0674 | 5.2114 | 88 B | 80 B |
| TryFindBoundaryIntersections_Polygon_WithoutIntersections | 189.65 | 98.086 | 3.5179 | 0.5444 | - | - |
| HasIntersection_Edge | 12.06 | 10.558 | 0.7853 | 0.2040 | - | - |
| HasIntersection_Polygon | 77.78 | 9.882 | 0.7576 | 0.1967 | - | - |
| HasIntersection_EdgeArray | 76.87 | 20.663 | 2.5960 | 0.6742 | - | - |

### Voronoi integration measurements

The first C3 run rejected a jittered cell whose existing floor quantization
introduced concavity. A permanent red-green integration regression now checks
all 400 cells. The producer takes the convex hull of its floored vertices without
weakening public polygon validation. Both workloads complete after that repair.
These measurements are current snapshots, not matched factory speedup claims;
C1 site/capacity validation and diagram ownership are still pending.

| Workload | Mean (us) | Error (us) | StdDev (us) | Allocated |
|---|---:|---:|---:|---:|
| Grid | 337.8 | 35.73 | 9.28 | 402.42 KB |
| JitteredGrid | 587.4 | 59.71 | 15.51 | 575.05 KB |

## C5 rectangle follow-up

Measured on 2026-10-19 with the same environment and job settings as P1 below.
All eight cases completed with no measured managed allocation. Empty-state
checks and canonical equality are included; the benchmark fixtures remain
nonempty. Short-run and subnanosecond measurement caveats still apply, so this
does not establish small speedups or empty-workload throughput.

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj --configuration Release --no-build --no-restore -- --filter '*RectangleBenchmarks*' --job short --iterationCount 5 --warmupCount 3 --iterationTime 100 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\correctness-c5
```

| Method | Mean (ns) | Error (ns) | StdDev (ns) | Allocated |
|---|---:|---:|---:|---:|
| Contains_Rect | 1.5841 | 0.3814 | 0.0590 | - |
| Contains_IRect | 2.2548 | 0.8615 | 0.1333 | - |
| Contains_Point | 1.0234 | 0.1014 | 0.0263 | - |
| Contains_IntInt | 0.2579 | 0.0552 | 0.0143 | - |
| IsEquivalentTo_Rect | 2.2112 | 0.2083 | 0.0541 | - |
| IsEquivalentTo_IRect | 3.1152 | 0.6399 | 0.1662 | - |
| IsEquivalentTo_PointPoint | 0.7882 | 0.3809 | 0.0590 | - |
| IsEquivalentTo_IntIntIntInt | 0.7046 | 0.0769 | 0.0119 | - |

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
