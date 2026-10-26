# D3 performance and memory refactor plan

## Objective and decision policy

Improve the execution speed of `Kiyote.Geometry.DelaunayVoronoi.D3` first, then
reduce allocation volume, retained memory, and peak working memory. All D3
classes and their dependencies are in scope. Public and internal API
compatibility is not a constraint.

This is an implementation plan, not a report of measured improvements. It is
based on source revision `4c73cae`, inspected on 2026-10-26, and the checked-in
[benchmark results][benchmark-results]. No new benchmarks or tests were run to
produce this document. Benefits below are hypotheses unless explicitly labeled
as existing measurements or storage calculations.

Use these rules when selecting an implementation:

1. Correct geometry, explicit failures, ownership, and thread safety are gates,
   not tradeable performance features.
2. Prefer a reproducible runtime improvement over a memory-only improvement,
   provided peak memory remains acceptable. Record the additional memory.
3. For statistically indistinguishable runtime, prefer fewer allocated bytes,
   fewer objects, and then lower retained/peak memory.
4. Do not accept extra complexity for a presumed optimization that the .NET 10
   JIT already performs. Inspect generated code where relevant.
5. Use the runtime, existing packages, and existing Kiyote infrastructure. No new
   production dependency is necessary for the recommended work.
6. Do not preserve obsolete object models or add compatibility adapters to hot
   paths just to avoid breaking an API. Migrate repository consumers instead.

### Scope and current responsibilities

| Source | Current role | Relevant options |
|---|---|---|
| [MapboxDelaunatorFactory][mapbox-factory] | Seed selection, radial sort, advancing hull, edge legalization | O1, O7, O9, O11, O12 |
| [MapboxDelaunator][mapbox-result] | Owns triangle, halfedge, and hull storage; exposes logical prefixes | O9, O10 |
| [D3DelaunayFactory][delaunay-factory] | Public Delaunay construction and Voronoi traversal-index construction | O5, O7, O9, O10 |
| [D3Delaunay][delaunay-topology] | Owns incoming-edge and hull-position arrays | O5, O8, O9 |
| [D3VoronoiFactory][voronoi-factory] | Circumcenters, clipping, quantization, cells, neighbours, global edges | O2-O9, O13, O14 |
| [D3 registration extensions][registration] | Registers singleton factories | O9, O13; preserve stateless singleton operation |
| [DiagramSites][sites] and [coordinate conversion][coordinate-conversion] | Validation, sanitization, snapshot, double-coordinate conversion | O7, O11, O12 |
| [Polygon][polygon], [Cell][cell], [Voronoi][voronoi-result], [Delaunay][delaunay-result] | Published geometry and its storage | O2, O4, O6, O10 |
| [Point][point], [Edge][edge], [Triangle][triangle], [Rect][rect] | Value geometry and equality/bounds semantics | O2, O4, O6, O10-O12 |
| [IVoronoi][voronoi-api], [IDelaunay][delaunay-api], factory interfaces | Consumer-facing output and creation contracts | All representation changes |
| [Searchable Voronoi][searchable], [IndexedCell][indexed-cell] | Downstream bounding-box index and queries | O2, O4, O6, O10 |

Keep the triangulation kernel, bounded clipping, geometry ownership, and public
creation orchestration separate. Do not turn the factory into a general-purpose
stateful engine. Helpers should have one responsibility and concrete/span-based
hot-path entry points, not an interface call per edge or vertex.

## Evidence before prioritization

### Existing measurements

The grid fixture names describe coordinate extents, not site counts. With
`DistanceApart = 5`, the three sizes contain 400, 10,000, and 40,000 sites.

| Sites | Core, reject duplicates | Delaunay factory | Voronoi factory |
|---:|---:|---:|---:|
| 400 | 67.92 us / 19.05 KB | 88.36 us / 46.21 KB | 371.2 us / 346.11 KB |
| 10,000 | 3,149.22 us / 470.4 KB | 3,630.19 us / 1,167.74 KB | 15,670.5 us / 10,035.29 KB |
| 40,000 | 14,952.44 us / 1,878.34 KB | 16,986.10 us / 4,679.08 KB | 77,850.1 us / 41,427.86 KB |

The existing Voronoi workload fixtures also report:

- Grid, 400 sites: 388.17 us / 346.11 KB.
- Jittered grid, 400 sites: 588.25 us / 485.67 KB.
- High degree, 41 sites: 34.23 us / 52.59 KB.

These are historical results on the hardware/runtime recorded in the benchmark
document, not a fresh baseline for this revision. They suggest investigating
both the triangulation core and Voronoi output construction. Do not subtract
independent benchmark means and present the difference as a measured phase
profile. The core source identifies `Legalize` as its largest contributor, but
that comment also needs confirmation with current profiling.

### Static scan execution checklist

The following recipes were run against the six D3 C# files. Counts are matching
source lines unless occurrences are explicitly stated. Dependencies were then
read for ownership and compound-allocation behavior; these counts do not claim
to cover the whole repository.

| Recipe | Result |
|---|---:|
| Literal `IndexOf`, `StartsWith`/`EndsWith`, or string `Contains` without comparison | 0 / 0 / 0 |
| `Substring`, parameterless `ToLower`/`ToUpper`, triple `Replace` chain | 0 / 0 / 0 |
| `params` signatures; `All`/`Any` with `char` predicates | 0 / 0 |
| Static readonly `Dictionary`; static readonly `FrozenDictionary` | 0 / 0 |
| Explicit `new List<T>`; explicit `new Dictionary<T>` in method bodies | 5 / 1 |
| `StringComparer.CurrentCulture` | 0 |
| LINQ `Select`/`Where`/`Cast`/`Take`/`Aggregate` | 0 |
| Unsealed nonabstract/nonstatic classes; sealed classes | 0 / 5 |
| Static classes | 1 |
| Blocking-async signals; `ValueTask` signals | 0 / 0 |
| `stackalloc` sites | 2, both outside loops |
| `ContainsKey`; `string.Format` | 0 / 0 |
| Regex signals; serialization/networking signals | 0 / 0 |
| `Math.Pow` | 4 lines, 6 calls |
| `ToArray` | 2 lines: neighbour-array publication and global-edge publication |
| `ArrayPool` rents; returns | 2 / 2 |
| New `double[]`; new `int[]` expressions | 4 / 5 |
| Target-typed `HashSet<Edge>` construction | 1 |
| Paired scalar `List.Insert` calls; `RemoveRange` | 2 calls / 1 call |

All five nonstatic classes are already sealed: 5/5. The five lists already have
initial capacities. Triangles are already value types. The core already pools
hull/ID/hash scratch and distances, returns rentals in `finally`, and exposes
only populated triangle/halfedge prefixes. Do not propose these as new wins.
Async, regex, string processing, and I/O recipes have no applicable hot path.

Three moderate hot-path pattern findings motivate immediate experiments:

- **F1 - redundant scalar operations:** six `Math.Pow(delta, 2)` calls in
  `D3VoronoiFactory.Step`, whereas the sibling triangulator's `Dist` already
  uses multiplication. Address in O3; actual speedup depends on JIT lowering.
- **F2 - mutable clipping machinery:** one reusable `List<double>` is edited by
  paired inserts and range removals. Address in O3; it is not a list allocated
  once per cell.
- **F3 - output materialization chain:** neighbour publication allocates one
  array and one read-only wrapper per site; global edges additionally pass
  through a mutable hash set and a final array. Address in O2/O6.

Here, moderate means worth investigating in the identified hot path, not a
claim of a particular speedup. None of the scan findings establishes a critical
performance regression.

### Compound allocations and storage model

The Voronoi creation chain currently includes, for every cell:

- A `Cell` object and a `Polygon` object.
- An owned polygon point array and an eagerly generated polygon edge array.
- Boxing `ImmutableArray<Point>` into `Polygon.Points`, whose type is
  `IReadOnlyList<Point>`. This is one cached box per polygon, not per getter call.
- A neighbour array and its `ReadOnlyCollection<Cell>` wrapper.
- A dictionary entry keyed by the `Cell` reference.

This is the stronger allocation target than changing already-preallocated
lists. Confirm exact object sizes with an allocation profile rather than
hard-coding object-header assumptions.

Let `n` be sites, `h` hull sites, and `t` populated triangles. Array payloads,
excluding object headers, pool bucket rounding, and alignment, are:

| Storage | Current payload |
|---|---:|
| Owned site snapshot | `8n` bytes |
| Double coordinate copy | `16n` bytes |
| Core triangle and halfedge capacity | `8 * max(6n - 15, 0)` bytes together |
| Circumcenters | `16t` bytes |
| Dense hull vectors | `32n` bytes, despite only hull sites being populated |
| Incoming-edge and hull-index arrays | `8n` bytes together |
| Core pooled integer scratch | `4 * (4n + ceil(sqrt(n)))` logical bytes |
| Core pooled distance scratch | `8n` logical bytes |
| Materialized Delaunay triangles | `24t` bytes |

For a valid connected planar triangulation using every distinct input site,
`t = 2n - 2 - h` and the undirected Delaunay edge count is `3n - 3 - h`.
Use actual lengths in production and validate assumptions before using these
identities as test or capacity bounds.

Coordinates, circumcenters, dense vectors, and D3 index arrays alone account
for `56n + 16t` temporary payload bytes, approximately `88n` for small hulls.
At 40,000 sites that is about 3.36 MiB. Pooling can reduce bytes allocated per
call without eliminating these live bytes or reducing process retention.

## G0 - establish correctness and measurement gates first

This is a prerequisite for every optimization, not an optimization to claim.

### Correctness-sensitive source hazards to characterize

Before changing traversal, seed selection, or numeric evaluation:

1. In `MapboxDelaunatorFactory.CreateCore`, seed-center X is currently
   `(minX + maxY) / 2`, not `(minX + maxX) / 2`. Add translated, anisotropic
   fixtures, correct the axis mix-up, and record changed seed/order behavior
   separately from performance experiments.
2. Direct internal core calls can index seed sentinels before checking whether
   valid second/third sites were found. Put minimum-count, distinctness, and
   collinearity failure checks before seed indexing. Public factory validation
   does not protect direct tests, profilers, and benchmarks. Preserve explicit
   argument/degeneracy failures, not incidental index exceptions.
3. `Legalize` currently discards pending work when its 512-entry stack is full.
   Introduce a cold growable pooled fallback, returned on every exit, or an
   explicitly documented failure. Prefer completing valid input. Test the
   overflow path using a deterministic small-capacity test seam if a natural
   fixture cannot reliably reach it.
4. A failed visible-hull search currently skips a site as a likely
   near-duplicate. For validated distinct bounded integer sites, diagnose this
   path and verify that every site participates; do not silently publish a
   partial triangulation as the fallback for a numeric failure.
5. Orientation on these bounded integer coordinates is exactly representable
   with the current double products, but the degree-four in-circle determinant
   is not guaranteed exact. Add an independent `Int128` test oracle. If it
   exposes wrong signs, use a proven floating-point error filter with a cold
   exact fallback, not an arbitrary epsilon or unconditional expensive math.
   Derive and document the overflow bound for the integer-domain fallback.

Do not rewrite exact-order fixture expectations merely to make a new kernel
pass. Where a seed or cocircular tie deliberately changes a valid diagonal,
test triangulation invariants and document the tie policy.

### Preserve these semantics unless explicitly designing a separate mode

- Domain is the bounded integer range in [GeometryLimits][limits].
- Preserve site order and stable snapshots; sanitization retains first
  occurrences and never excuses out-of-domain coordinates.
- Rectangle endpoints are inclusive. Geometric clipping uses `X1/X2/Y1/Y2`,
  not `X + Width` as a replacement for `X2`.
- Keep doubles through circumcenter calculation and clipping. Apply
  `Math.Floor` only to final cell vertices, including negative coordinates.
- Rounded cells are the convex hull of the rounded candidate vertices.
  Reject collapsed cells, rather than repairing arbitrary public polygon input.
- Pre-clipping traversal limit is 64 vertices, projected capacity is 66,
  clipped scratch bound is 140, and final polygon limit is 128. These are
  different limits. The open-cell-at-limit regression must continue to pass.
- Preserve neighbour completeness, reciprocity, site/cell correspondence,
  bounds, and current `IsOpen` behavior, including box-corner cases.
- Global `Edges` currently deduplicates **directed** `Edge` values.
  `A -> B` and `B -> A` are different. Delaunay edges are not interchangeable
  with quantized/clipped polygon edges.
- Results remain immutable and stable after future calls, input mutation,
  failures, concurrent creation, and pool reuse.

API breakage is allowed; accidental geometry or lifetime changes are not.

### Measurement work

Extend the existing [D3 benchmark classes][benchmark-directory] and
[VoronoiWorkloadBenchmarks][workload-benchmarks], rather than starting a new
benchmark application:

1. Rebaseline on the same revision, machine, configuration, SDK, and power
   settings. Record mean, error, distribution, allocated bytes, GC counts, and
   site/triangle/hull/vertex counts.
2. Keep the three existing grid sizes and both core duplicate-check modes.
   Add deterministic irregular/clustered sites, long thin translated bounds,
   mostly-hull sites, near-collinear valid sets, and repeated equal-distance
   cases. Validate generated inputs outside timed code.
3. Exercise sanitization separately: all valid, duplicate-heavy, filtered
   outside sites, and post-filter degeneracy. Add degrees 63/64 and the explicit
   above-limit failure; do not benchmark an invalid set as a successful diagram.
4. Add stage benchmarks for preprocessing, core, topology indexing, clipping
   plus quantization, polygon ownership, adjacency, and global-edge publication.
   Keep fixture construction outside measurements. Use reusable prepared
   inputs or invocation setup as appropriate, and measure cold behavior
   separately so repeated use does not hide allocations.
5. Collect a sampled CPU profile and an allocation profile for the complete
   Delaunay and Voronoi factory calls. Add diagnostic-only counters for
   legalization tests/flips, maximum stack depth, hull probes/walks, clipped
   vertex counts, and polygon fallback rates. Remove or compile out counters
   from timed production measurements.
6. Measure warm-pool allocation, cold-pool behavior, retained result size, peak
   live memory, and concurrent calls separately. Do not report
   `GC.GetAllocatedBytesForCurrentThread` as retained memory or as complete
   allocation accounting for parallel work.
7. Consume returned results. For indexed/lazy/selective output, benchmark both
   construction and construction followed by full traversal/materialization.

**Suggested acceptance policy:** accept a speed-focused change after at least
two comparable runs show a reproducible >=3% improvement in its target
end-to-end workload, outside measurement noise, and no unexplained >3% regression
on another representative workload. If runtime is indistinguishable, a >=10%
reduction in target allocation or retained/peak bytes can justify a simple
memory-focused change. These are decision thresholds, not promised gains.
Remeasure longer when the checked-in level of variance obscures them. Any
exception needs an explicit workload-specific rationale.

## Ranked implementation options

Expected upside and risk are qualitative. O1/O2 target the largest plausible
runtime costs; O3/O4 are smaller, cheaper experiments that can be delivered
first. O8/O9 primarily address temporary memory and should not displace
runtime work merely because their byte savings are easier to calculate.

| Rank | Option | Primary effect | Risk | Prerequisites |
|---:|---|---|---|---|
| 1 | O1: streamline edge legalization and hull insertion | Core CPU | Medium | G0 |
| 2 | O2: indexed, contiguous Voronoi result | Output CPU, object count, retained memory | High | G0 |
| 3 | O3: span-based clipping scratch and scalar cleanup | Cell CPU, scratch allocation | Medium | G0 |
| 4 | O4: generated-polygon fast path and unboxed vertex access | Polygon CPU and per-cell objects | Medium | G0 |
| 5 | O5: share ordered topology walks and build indices into spans | Traversal CPU, intermediate storage | Medium | G0; integrate with O2/O3 |
| 6 | O6: selective outputs and cheaper directed-edge publication | Unneeded work and hashing | Medium | G0; coordinate with O2 |
| 7 | O7: owned/span preprocessing pipeline | Validation/conversion CPU, copies | Low-medium | G0 |
| 8 | O8: remove dense hull-vector storage | Temporary bytes and initialization | Low | G0 |
| 9 | O9: call-scoped pooled workspace | Allocation/GC and peak overlap | Medium-high | G0; finalize buffer contracts first |
| 10 | O10: direct or indexed Delaunay output | Delaunay materialization/storage | Low-high by variant | G0; coordinate with O7/O9 |
| 11 | O11: alternative duplicate checking and radial sorting | Large-input preprocessing CPU | Medium | G0, phase profile |
| 12 | O12: coordinate layout, vectorization, and numeric specialization | Core CPU/cache behavior | Medium-high | G0, core profile |
| 13 | O13: coarse parallel cell construction | Large-input latency | High | G0, stable O2/O3/O9 |
| 14 | O14: neighbour-half-plane clipping kernel | Alternative cell algorithm | High | G0, O5; research branch |

### O1 - streamline legalization and advancing-hull insertion

**Where:** [MapboxDelaunatorFactory][mapbox-factory], especially `Legalize`,
`InCircle`, `HashKey`, and `CreateCore`.

**How to implement:**

1. Profile legalizations, no-flip exits, flip exits, and rare boundary repair
   separately. Work on the dominant branch, not just the visually large method.
2. Make the no-flip/boundary completion path straightforward; keep hull repair
   and stack growth cold. Preserve the exact halfedge link updates and pending
   edge ordering while restructuring.
3. Cache triangle/coordinate spans, indices, and repeated coordinate loads in
   locals where disassembly shows repeated loads or bounds checks. Compare
   kernel code size and register spills; do not add blanket inlining attributes
   to the large legalization routine.
4. Hoist the probe's `HashKey(x, y, ...)` out of the inner hash-probing loop.
   Compare increment-and-wrap probing with repeated remainder. Preserve the
   lookup sequence, sentinel checks, and wrap behavior.
5. Inspect division/remainder by three in triangle navigation before replacing
   it; the JIT can strength-reduce constants. Use one small shared helper only
   if it inlines without regressions.
6. If the rare hull-repair walk is unexpectedly significant, prototype a reverse
   boundary-edge-to-hull-site map and update it on every affected flip.
   Keep it only if its CPU win exceeds initialization/cache costs: an `int`
   entry per capacity halfedge adds roughly `24n` payload bytes.

**Tradeoff:** hot arithmetic/control-flow cleanup should not allocate. A reverse
map deliberately spends memory for speed and is not the default recommendation.
Floating-point reassociation/FMA is a separate numeric experiment, not a free
cleanup.

**Verification:** compare topology invariants against the G0 exact oracle and
existing fixtures; exercise stack overflow, cocircular ties, all-hull inputs,
and failures followed by reuse. Benchmark core plus both public factories.
Stop when profile share shifts elsewhere rather than continuing speculative
micro-optimization.

### O2 - replace the Voronoi object graph with an indexed result

**Where:** [D3VoronoiFactory][voronoi-factory], [IVoronoi][voronoi-api],
[Voronoi][voronoi-result], [Cell][cell], [Polygon][polygon], and
[searchable Voronoi][searchable].

**Recommended architecture:** one immutable diagram owns sites, cell metadata,
contiguous vertex storage, and compressed-sparse-row (CSR) neighbour IDs.
Cell identity is its site index. Prefer this to a large public `Cell` value
copied repeatedly or to retaining object adapters around every cell.

**How to implement:**

1. Introduce an internal cell metadata value containing a vertex range, bounds,
   and flags. Derive the center from the owned site array. Expose cell IDs and
   read-only span/ref-readonly access; do not expose mutable array aliases.
2. Store all final polygon vertices in one owned `Point[]`, with per-cell
   ranges. Start with a growable per-build contiguous buffer and one final
   ownership transfer/copy. Compare against a two-pass exact-size build:
   clipping every cell twice is not automatically worth the smaller capacity.
3. Store neighbours as `offsets[n + 1]` and one `int[]` of neighbour IDs.
   Enumerate each site's one-ring in its existing order, count/prefix-sum/fill,
   and include the final hull neighbour. Preserve reciprocal, duplicate-free
   adjacency even for cocircular edges whose dual segment has zero length.
4. Remove `Dictionary<Cell, IReadOnlyList<Cell>>`, per-site neighbour arrays,
   per-site wrappers, and reference-key lookup from the primary API. A read-only
   span slice is sufficient for lookup by cell ID.
5. Provide allocation-free polygon operations over vertex slices, reusing
   extracted geometry kernels from `Polygon`. Make exporting a standalone
   `Polygon` explicit. Do not allocate/cache a polygon from an ordinary cell
   getter or reconstruct one for every query.
6. Migrate the quadtree adapter, query visitors, visualizer, mocks, tests, and
   benchmark consumers in the same change. Preserve distinct cell identity and
   multiplicity even when two cells have identical bounds. Avoid boxing
   `IRect` per insertion/query or copying large cell metadata in inner loops.
7. If a complete representation change is too large for one commit, first
   introduce CSR neighbour IDs while retaining existing cells. Remove the
   object facade in a subsequent commit; do not make it the permanent hot path.

**Tradeoff:** removes multiple objects/arrays per site and improves locality, but
may increase accessor complexity or copying if IDs/spans are used poorly.
Do not also retain complete per-cell polygon/edge objects, which would erase
the benefit. Measure tree-index item size if its adapter starts storing both a
diagram reference and an ID.

**Verification:** compare all cells geometrically, neighbour sets/order, flags,
and bounds with the reference implementation. Benchmark create-only, full-cell
iteration, neighbour traversal, polygon containment, searchable construction,
and queries. Record object count and retained bytes in addition to B/op.
Require no allocations in repeated span/ID traversal after construction.

### O3 - replace mutable clipping lists with bounded span writers

**Where:** [D3VoronoiFactory][voronoi-factory]: `Clip`, `ClipFinite`,
`ClipInfinite`, `Edge`, `InsertPoint`, `Simplify`, and `Step`.

**How to implement:**

1. Allocate fixed scratch once per creation, outside the cell loop. Use a
   bounded span writer for coordinate pairs or an internal double-coordinate
   value type. Keep the 64/66/140 vertex capacities distinct and explicit.
2. Replace `List<double>` with a populated-length span. Append pairs together;
   for a required middle insertion, shift the tail once, not twice. Check
   capacity before writing either coordinate and retain explicit failure.
3. Replace repeated `RemoveRange` simplification with a compaction pass and
   explicit closing-seam cleanup. Preserve the current horizontal/vertical
   simplification conditions and cyclic behavior; do not substitute a broader
   epsilon-based collinearity rule.
4. Stack-allocate small bounded buffers outside the loop only after calculating
   the total nested stack budget, including polygon hull scratch. Otherwise
   obtain one call-scoped rental. Current heap double buffers together are
   2,080 payload bytes; clipped scratch adds another 2,240 bytes if represented
   as doubles, and quantized scratch is already 1,120 bytes.
5. Replace the six square-power calls in `Step` with local deltas and
   `dx * dx + dy * dy`, consistent with core `Dist`. Verify assembly first:
   .NET may already optimize constant powers.
6. Keep a reference clipping implementation available in tests while changing
   storage and simplification. Do not change the clipping algorithm and buffer
   representation in the same initial experiment.

**Tradeoff:** eliminates list bookkeeping and paired shifting, with a small
bounded stack/rental footprint. Interior cells typically have few vertices, so
expectation must be verified rather than inferred from worst-case complexity.

**Verification:** differential tests for every box edge/corner, open and closed
cells, negative coordinates, repeated seam vertices, exact limits, and explicit
overflow. Confirm `IsOpen`, vertex hull, and bounding boxes remain unchanged.
Benchmark regular, jittered, and high-degree workloads separately.

### O4 - accelerate generated polygons without weakening validation

**Where:** [Polygon][polygon], its generated-hull path, and the quantization
loop in [D3VoronoiFactory][voronoi-factory].

**How to implement:**

1. Remove the cached `ImmutableArray<Point>` interface box by exposing typed
   immutable/span access for performance consumers. Do not replace it with an
   expression-bodied `IReadOnlyList<Point>` getter returning the struct: that
   would box on every access instead of once. Update consumers accordingly.
2. Keep the current sort-and-monotone-chain hull as the correctness fallback.
   Prototype a small-cell fast path, initially for three/four vertices and
   then at most a measured small threshold such as eight.
3. Remove adjacent duplicates and the repeated closing point in scratch. Check
   exact integer half-plane signs against every other vertex for the small
   candidate: all nonincident vertices must lie strictly on the same interior
   side. Local turn signs alone do not reject every star/self-intersection.
4. Only on success, rotate to the lexicographically smallest vertex and ensure
   the same winding as the fallback, then transfer a final owned copy through
   a narrow internal construction path. For collinear, concave, repeated,
   collapsed, or uncertain candidates, use the existing full hull unchanged.
5. Reuse the quantization pass to compute bounds. Preserve inclusive `+1`
   dimensions. For O2, write normalized points directly into the diagram's
   final vertex storage and avoid standalone polygon construction entirely.
6. Compare eager `Edge[]` storage with an allocation-free adjacent-vertex edge
   enumerator. Measure containment/intersection consumers before removing
   cached edges: memory savings must not become an unmeasured query regression.

**Tradeoff:** a bounded quadratic check can beat generic sorting for tiny cells,
but is unsuitable for high-degree cells. O2 supersedes per-cell storage work;
retain only normalization-kernel improvements that still matter.

**Verification:** compare exact hull vertex order as well as geometric
equivalence against the fallback over seeded rounded candidates. Cover
quantization-induced concavity, collinear/seam duplicates, winding, stars,
negative fractions, collapse, and 128-vertex output. Keep public construction
rejection tests unchanged. Benchmark polygon construction and complete diagrams.

### O5 - share topology traversal and remove temporary wrapper ownership

**Where:** [D3DelaunayFactory][delaunay-factory],
[D3Delaunay][delaunay-topology], and cell/neighbour traversal in
[D3VoronoiFactory][voronoi-factory].

**How to implement:**

1. Split traversal-index construction from result ownership:
   `BuildIndices(..., Span<int> inedges, Span<int> hullIndex)` fills caller-owned
   buffers. Initialize both logical ranges to `-1`; preserve exterior-edge
   preference. The factory, not a separate heap wrapper, owns their lifetime.
2. Extract a small allocation-free one-ring traversal kernel with concrete
   spans. Avoid `yield`, captured callbacks, or interface visitors per halfedge.
3. When collecting a cell's circumcenters, also collect its ordered neighbour
   IDs. The incoming halfedge walk already reads the same topology needed by
   the later neighbour pass. Preserve the additional terminal hull neighbour.
4. With O2, append IDs to CSR construction buffers. Without O2, retain flat
   integer IDs/ranges until all `Cell` instances exist, then resolve references.
   Benchmark the extra integer writes against simply walking topology twice.
5. Keep `Contains`/`Step`'s distance comparisons specialized; sharing iteration
   must not force extra work or allocations into its clipping queries.

**Tradeoff:** removes duplicate irregular memory walks and the `D3Delaunay`
wrapper. Temporary neighbour IDs can offset savings if the object API is kept.
Do not add a generic graph abstraction to save a few lines.

**Verification:** count every expected neighbour for degrees 25, 26, 40,
63, and 64 plus open-cell limits. Check the hull's first/final neighbour,
incoming-edge orientation, and reciprocity. Benchmark topology construction and
complete creation; reject fusion if additional writes cost more than the walk.

### O6 - avoid unnecessary outputs and cheapen directed-edge publication

**Where:** [IVoronoi][voronoi-api], [Voronoi][voronoi-result],
[D3VoronoiFactory][voronoi-factory], and [Edge][edge].

**How to implement:**

1. Separate cells-only construction from topology/full-edge materialization
   with clearly named creation methods or result capabilities, not a collection
   of boolean flags inside every inner loop. Keep full construction available.
2. Let callers that only clip/rasterize cells omit global edges and neighbour
   publication. Do not retain intermediate mesh storage after creation just to
   support a surprise lazy getter. Explicit materialization should identify
   its cost and ownership.
3. For full directed-edge output, compare pre-sizing the existing set using the
   actual sum of final polygon side counts with a compact key implementation.
   Do not reserve an unnecessarily huge worst-case capacity on small diagrams.
4. Validated coordinates fit losslessly in a 32-bit point key:
   `((uint)(x - MinimumCoordinate) << 16) | (uint)(y - MinimumCoordinate)`.
   Pack a **directed** pair into `ulong`, A in the high half and B in the low
   half. Prototype `HashSet<ulong>` to reduce key payload from 16 to 8 bytes
   and avoid nested point/edge hashing.
5. Preserve first-seen edge values/order if required by chosen output semantics;
   either decode packed keys or append original values only on successful set
   insertion. Benchmark the extra list/array against decoding a final key set.
6. If undirected geometric edges are desirable, make that an explicit separate
   representation with tests and documentation. Never silently canonicalize
   endpoints as a performance change.

**Tradeoff:** omitting unwanted outputs can remove entire passes, but is not an
equivalent full-workload speedup. Packed hashing reduces table payload but adds
packing/decoding. Clipped polygon sides cannot simply be replaced by unique
primal Delaunay edges.

**Verification:** compare full directed-edge sets, reversed pairs, box edges,
and quantization-collapsed/shared boundaries. Test cells-only and full modes
independently. Report equivalent-work benchmarks, plus optional-output savings
clearly labeled as less work.

### O7 - make preprocessing an owned, span-based pipeline

**Where:** [DiagramSites][sites], [coordinate conversion][coordinate-conversion],
and both public D3 factory entry points.

**How to implement:**

1. Return an internally owned array plus logical count from validation rather
   than immediately wrapping it as `IReadOnlyList<Point>`. Keep external
   publication read-only. This lets internal loops use spans without casts or
   interface indexing.
2. Snapshot every caller input once. Add a `ReadOnlySpan<Point>` entry point if
   useful, but do not borrow mutable caller arrays for a supposedly immutable
   result. Do not perform an avoidable second snapshot through another factory.
3. Fill coordinate scratch while validating/copying accepted sites. Fuse bounds
   accumulation and the exact noncollinearity test where practical, continuing
   to validate every input site even after noncollinearity is established.
4. When sanitizing, compact first occurrences in place, preserve order, and
   validate the surviving set. Choose either a final trim or logical-count
   storage based on retained-memory measurement; do not expose unused capacity.
5. Delay read-only wrappers until publication, or publish typed immutable/span
   surfaces. Never return mutable arrays behind an interface and assume that
   declaring it read-only prevents a consumer from casting it back.

**Tradeoff:** mostly removes dispatch, repeated passes, and avoidable wrappers;
the initial snapshot is necessary. Coordinate fusion reduces passes but adds
cleanup requirements if its destination is rented and validation throws.

**Verification:** array/list/custom-list inputs, impossible count before index
access, out-of-domain sites even under sanitization, outside-bound filtering,
duplicates, first-occurrence order, mutation after return, and failures followed
by reuse. Benchmark preprocessing separately and end to end at all three sizes.

### O8 - calculate hull rays on demand instead of allocating `4n` doubles

**Where:** `CalculateVectors` and `Clip` in
[D3VoronoiFactory][voronoi-factory], using existing hull-position metadata.

**How to implement:**

1. Determine finite versus infinite cells from `hullIndex[i] >= 0`, instead of
   testing a mostly-zero dense vector array.
2. For hull position `k`, read the previous, current, and next site, wrapping
   at both ends. Compute the incoming ray as
   `(previousY - currentY, currentX - previousX)` and outgoing ray as
   `(currentY - nextY, nextX - currentX)`, matching current assignments.
3. Pass those four values to the existing infinite-cell clipping path. Interior
   sites compute no rays.
4. Compare this with a hull-only `4h` double table only if repeated accesses make
   caching worthwhile. Avoid another length-`n` mapping when `hullIndex` already
   supplies it.

**Tradeoff:** saves exactly `32n` payload bytes and their zero-initialization,
1.22 MiB at 40,000 sites. Adds a few loads/subtractions per hull site; may improve
runtime by avoiding memory traffic, but do not assume that.

**Verification:** triangle hulls, many-hull diagrams, wraparound positions,
horizontal/vertical hull sides, and translated/negative coordinates. Compare
projected endpoints and `IsOpen` with the reference path. Measure both small-hull
grids and mostly-hull inputs.

### O9 - introduce one call-scoped workspace for temporary data

**Where:** [MapboxDelaunatorFactory][mapbox-factory],
[MapboxDelaunator][mapbox-result], both factories, and traversal-index building.

**How to implement:**

1. Draw a lifetime/ownership map for site snapshot, coordinates, core scratch,
   mesh arrays, D3 indices, circumcenters, and cell scratch. Separate persistent
   output from temporary data before adding more pooling.
2. Keep one triangulation kernel that writes into supplied spans. The existing
   owned core-result path remains for direct consumers/tests; public diagram
   factories can use a scoped temporary mesh workspace when they consume and
   discard it before returning.
3. Rent coordinates, circumcenters, D3 indices, and temporary mesh arrays where
   profiling justifies it. Reuse nonoverlapping lifetime regions; never reuse
   mesh/index storage still needed by clipping or adjacency.
4. Use a small internal disposable/ref-struct owner or explicit `try/finally`.
   Slice every rental to logical length. Initialize all read-before-write
   values, especially `-1` sentinels; pooled primitive arrays contain dirty data.
5. Return core hull/distance scratch as soon as triangulation ends, rather than
   retaining it for the whole Voronoi call merely because a unified workspace
   makes that convenient.
6. Keep final sites, vertices, adjacency, triangle outputs, and edge outputs in
   owned storage. No public result or span may outlive a returned rental.
   Do not pool persistent results without a deliberate disposable-result API.
7. Keep singleton factories stateless. Do not put a shared scratch array, mutable
   workspace, or unbounded per-thread cache on a registered singleton.

**Tradeoff:** can remove substantial steady-state allocation and GC work, but
pool buckets, cold rents, and concurrent workers may increase retained/peak
memory. ArrayPool is not a guarantee of zero allocation. Avoid fragmenting
workspace ownership across many independently pooled objects.

**Verification:** extend [TriangulationStorageTests][storage-tests] to dirty
oversized buffers, exceptions at each allocation/validation phase, repeated
creation while holding old results, parallel calls, and logical lengths.
Keep allocation budgets tied to owned outputs, not relaxed arbitrarily.
Measure cold/warm pools, concurrent peak memory, and end-to-end speed.

### O10 - choose a deliberate Delaunay output representation

**Where:** [D3DelaunayFactory][delaunay-factory],
[Delaunay][delaunay-result], [IDelaunay][delaunay-api], and
[Triangle][triangle].

**How to implement, low-risk variant:**

1. Allocate final `Triangle[]` and hull `Point[]` at known logical lengths and
   fill them directly, using spans over the owned site snapshot.
2. Transfer ownership to typed immutable/read-only publication exactly once.
   This replaces list objects and list indexing, not per-triangle heap objects:
   `Triangle` is already a 24-byte value type and the current lists are sized.
3. Remove the redundant nonnegative hull-index branch only after establishing
   and testing that the core hull contract never exposes sentinels.

**How to implement, larger alternative:**

1. Publish site-index triples with the owned site snapshot, rather than eagerly
   expanding each triangle into three points. Expose cheap index spans and an
   explicit materialization method for consumers that need `Triangle` values.
2. Keep only needed topology. Do not retain halfedges as a hidden cost if the
   public result only needs triangles and hull.
3. Compare prefix exposure of existing capacity storage against copying to
   exact-length arrays. Mostly-hull inputs have much more unused capacity than
   dense interior inputs; trimming adds a pass and peak overlap.

**Tradeoff:** index triples use `12t` payload bytes instead of `24t`, plus the
already-required site snapshot. They add indirection and point gathers during
consumption, so indexed output is not automatically faster for rasterization
or repeated full-triangle traversal. Do not claim create-only gains as total
workload gains.

**Verification:** ordered triangle equality, orientation/hull invariants, output
immutability, [Delaunay ownership tests][delaunay-tests], and the existing
[triangle representation benchmarks][triangle-benchmarks]. Add indexed versus
materialized create-and-consume benchmarks before choosing the default.

### O11 - benchmark duplicate detection and radial-sort alternatives

**Where:** [DiagramSites][sites] and [MapboxDelaunatorFactory][mapbox-factory].

**Why conditional:** the existing 40,000-site core measurements are 14,490.84 us
without duplicate rejection and 14,952.44 us with it, with overlapping reported
variation. This is not evidence that duplicate validation dominates.

**How to implement:**

1. Benchmark the current packed-into-double key sort against packed `uint` keys
   in rented storage and a capacity-sized hash set for sanitized input.
   All 32 key bits currently fit exactly in double; this is not a precision fix.
2. Consider two 16-bit radix passes only for sufficiently large inputs and only
   after accounting for the histogram's initialization and additional key
   buffer. Avoid per-call 65,536-bucket work for small diagrams.
3. Keep site IDs/order unchanged while detecting duplicates. Sanitization must
   compact original first occurrences, not return key-sorted sites.
4. Separately compare the custom radial `Quicksort` with the runtime paired
   key/value span sort. If sorting distances in place alongside IDs, insertion
   must iterate the resulting IDs and must not later treat reordered distances
   as indexed by original site ID.
5. Specify deterministic equal-distance handling. Test both valid cocircular
   diagonals through geometric invariants; do not rely on accidental stability
   of a replacement sort.

**Tradeoff:** hash tables/radix passes spend memory and initialization to reduce
comparison work. A runtime sort may be better hardened against poor partitions
but adds key movement. Keep only the measured winner or a justified size cutoff.

**Verification:** negative/extreme-coordinate key round trips, duplicates far
apart in input, dirty rentals, first-order sanitization, equal-distance grids,
and adversarial radial distributions. Measure total preprocessing/core time,
not just an isolated sort with artificially favorable inputs.

### O12 - test coordinate layout and vectorization after profiling

**Where:** [MapboxDelaunatorFactory][mapbox-factory],
[coordinate conversion][coordinate-conversion], and circumcenter calculation.

**How to implement:**

1. Compare the current interleaved `double[]` with direct `ReadOnlySpan<Point>`
   coordinates and, separately, X/Y double spans. Point storage is half the
   interleaved-double payload, but every numeric use then needs conversion.
2. Keep double intermediates in geometric predicates. Do not switch the kernel
   to float or narrow global `Point` storage merely because the accepted diagram
   domain is bounded; `Point` also represents wider coordinates/displacements.
3. Vectorize independent scans such as min/max and radial distance generation
   only when they have meaningful profile share. Start with portable
   `System.Numerics.Vector<T>` or runtime-supported APIs and retain scalar tails.
4. Do not assume gather-heavy `InCircle`/legalization or irregular circumcenter
   traversal benefits from SIMD. Test batching only if layout/profile supports
   it, and count extra temporary arrays in the comparison.
5. Change arithmetic association/FMA only with the G0 numeric oracle and a
   documented tie policy; tiny sign changes can alter topology.

**Tradeoff:** direct point access can remove `16n` temporary coordinate bytes;
SoA/SIMD can add conversion passes and extra buffers. Reject architecture-
specific complexity when end-to-end gains do not clear the acceptance gate.

**Verification:** scalar/vector agreement across the full bounded domain,
nonmultiple vector lengths, sparse hulls, and nearly cocircular inputs.
Benchmark both factories and inspect JIT code on the supported runtime.

### O13 - parallelize only independent, sufficiently large cell stages

**Where:** cell construction in [D3VoronoiFactory][voronoi-factory], after a
read-only mesh/topology is complete.

**How to implement:**

1. Leave the current advancing-hull triangulator serial. Partition cell IDs
   into coarse contiguous ranges after triangulation and circumcenter creation.
2. Give each worker exclusive clipping/quantization scratch and disjoint
   metadata output. For O2 vertices/adjacency, use per-partition append buffers
   plus one prefix-sum/merge, or measured two-pass exact ranges.
3. Preserve cell/site order when merging. Do not mutate a shared list, edge set,
   or dictionary from workers. Build/deduplicate global edges afterward or merge
   explicitly owned partition data.
4. Select a minimum size from benchmarks and bound parallelism. Keep small
   inputs serial and consider callers already parallelizing diagram creation.
5. Preserve explicit failure: no partial diagram escapes if one worker fails.
   Ensure all worker rentals are returned and document exception behavior.

**Tradeoff:** can improve one large diagram's latency while hurting throughput,
small-input speed, determinism of failures, and peak memory. It is not the
default until one-thread improvements are exhausted and multicore benefit is
proven on the target workload.

**Verification:** serial/parallel result equality, simultaneous calls on one
singleton, cancellation/failure cleanup if cancellation is introduced, and
allocation accounting across threads. Measure single-request latency and
many-request throughput, not just an isolated parallel benchmark.

### O14 - prototype clipping cells by neighbour half-planes

**Where:** an experimental alternative to circumcenter/ray/segment clipping in
[D3VoronoiFactory][voronoi-factory], reusing O5 ordered neighbours.

**How to implement:**

1. Start each cell as the clipping rectangle in double precision.
2. For site `p` and each Delaunay neighbour `q`, clip the current polygon against
   `2 * (q - p) dot x <= |q|^2 - |p|^2`. Calculate integer coefficients in wide
   types, then perform intersection division in double precision. No square
   roots or normalized bisectors are necessary.
3. Use two bounded ping-pong span buffers and a Sutherland-Hodgman-style
   half-plane pass. Derive intermediate/final capacity bounds; do not assume
   the existing circumcenter scratch bounds apply to a different algorithm.
4. Retain one-ring degree limits, final floor quantization, convex-hull repair,
   and collapse rejection. Prove Delaunay-neighbour constraints suffice even
   for cocircular degeneracy; redundant neighbours must not be dropped blindly.
5. Reproduce current `IsOpen` behavior with explicit tests. Distinguish
   boundary contact from positive-length clipped boundary and from a site
   merely being on the Delaunay hull.
6. Compare continuous vertex geometry, final integer polygons, flags, and cost
   against the current implementation on the entire workload matrix. Keep the
   original path as a test reference until the experiment is accepted.

**Tradeoff:** could remove circumcenter storage, ray vectors, `Project`,
`ClipSegment`, and corner `Contains` queries, with a simpler geometric kernel.
Repeated half-plane clipping can cost quadratic work in cell degree and may be
slower on rings. Floating-point evaluation order can change floored vertices
near integer boundaries. This is a research alternative, not a guaranteed
replacement or a reason to weaken equivalence tests.

**Verification:** all G0 geometry properties plus fractional circumcenters,
nearly parallel bisectors, full-domain sites, high-degree open/closed cells,
box-corner containment, and quantization collapse. Adopt only after a measured
equivalent-work win; otherwise retain the existing algorithm.

## Execution and validation handoff

### Recommended delivery sequence

1. **Baseline/gates:** G0, existing test status, numeric oracle, and phase/
   allocation profiles. Track pre-existing failures separately.
2. **Small independent experiments:** O1's invariant hash-key work, O3 scalar
   cleanup, O4 unboxed access, O7 internal ownership/spans, and O8 hull rays.
   Use separate measurements so one gain does not hide another regression.
3. **Cell pipeline:** O3 bounded writers, O4 normalized-polygon fast path, and
   O5 traversal sharing. Keep the geometric reference intact during migration.
4. **Primary structural change:** O2 indexed Voronoi storage and O6 output
   capabilities. This is the recommended direction if allocation profiles
   confirm the per-cell graph is a substantial cost. Revisit O4 storage work
   because O2 removes much of it.
5. **Lifetime/output tuning:** O9 scoped temporary mesh/workspace and O10's
   chosen Delaunay representation.
6. **Only with remaining evidence:** O11/O12 kernel experiments. O13 and O14
   belong in separate experimental changes, not the initial rewrite.

Dependencies are not a demand to implement every option. Stop or change
priority when fresh profiles show that a target is no longer material.

### Existing tests to extend

- [MapboxDelaunatorFactoryTests][core-tests]: orientation, hull/triangle order,
  degeneracy, seed selection, and core topology.
- [TriangulationStorageTests][storage-tests]: dirty oversized pooled scratch,
  output ownership, logical lengths, concurrent calls, and exception cleanup.
- [DiagramRegressionTests][diagram-tests]: input validation, snapshots,
  sanitization, supported/high-degree limits, fractional circumcenters,
  nearest-site half-planes, output immutability, and neighbour reciprocity.
- [D3VoronoiFactoryTests][voronoi-tests]: jittered quantized cells remain convex
  and contain their sites.
- [PolygonRegressionTests][polygon-tests]: strict public construction,
  generated quantization behavior, winding, and storage immutability.
- [AllocationRegressionTests][allocation-tests]: inline triangle storage and
  preservation of distinct cells with equal bounds.
- [SearchableVoronoiRegressionTests][search-tests] and searchable unit tests:
  bounding-box search equivalence, visitors, multiplicity, and index bounds.

Add tests named `Method_Condition_Expectation`. For differential geometry
checks, compare canonical polygons or `IsEquivalentTo`, not only counts.
For the mesh, verify valid indices, reciprocal halfedges, boundary membership,
consistent nonzero triangle winding, every accepted site's participation, and
local Delaunay legality against the independent oracle. Do not use a new kernel
as its own reference.

### Commands for an implementing agent

Run from the repository root on the current NUnit/VSTest projects. Adapt class
filters if a representation change deliberately moves tests; ensure a filter
matches tests rather than accepting a zero-test success.

```powershell
# Focused correctness/storage suite; no restore unless packages are missing.
dotnet test .\test\Kiyote\Geometry\Kiyote.Geometry.IntegrationTests\Kiyote.Geometry.IntegrationTests.csproj -c Release --no-restore --filter "FullyQualifiedName~DelaunayVoronoi"
dotnet test .\test\Kiyote\Geometry\Kiyote.Geometry.UnitTests\Kiyote.Geometry.UnitTests.csproj -c Release --no-restore --filter "FullyQualifiedName~TriangulationStorageTests|FullyQualifiedName~AllocationRegressionTests|FullyQualifiedName~Polygon|FullyQualifiedName~DelaunayVoronoi"

# Existing equivalent-work benchmark families.
dotnet run --project .\test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj -c Release --no-restore -- --filter "*MapboxDelaunatorFactoryBenchmarks*" "*D3DelaunayFactoryBenchmarks*" "*D3VoronoiFactoryBenchmarks*" "*VoronoiWorkloadBenchmarks*"
```

Use a narrower benchmark selector for an individual experiment, then rerun the
relevant end-to-end families before acceptance. Do not run competing benchmark
processes concurrently. Build affected consumers, including the visualizer
and profiler when signatures change; run the full test suites when shared
polygon/value geometry behavior changes.

### Completion checklist for each accepted option

- Record baseline/candidate revision, target workloads, runtime/error, B/op,
  object count where relevant, and retained/peak-memory observations.
- Record whether improvements come from equivalent work, deferred work,
  different outputs, or increased memory consumption.
- Delete unsuccessful experimental paths and diagnostic counters from
  production code; retain meaningful correctness and allocation regressions.
- Verify exceptions, dirty-rental behavior, old-result stability, and singleton
  concurrency for any ownership/buffering change.
- Update [package documentation][package-readme] and [root documentation][root-readme]
  for API, storage, limit, edge-direction, or creation-mode changes.
- Update interface XML documentation, DI registration if needed, all affected
  consumers, benchmark fixtures, and [benchmark results][benchmark-results].
- Do not loosen geometric assertions or allocation budgets merely to permit a
  new implementation. Explain intentional contract changes explicitly.

## Static scan finding summary

This table counts F1-F3 only, not every algorithmic/architectural option.

| Severity | Count | Top issue |
|---|---:|---|
| Critical | 0 | No critical API-usage performance finding established |
| Moderate | 3 | Per-site output materialization and mutable clipping machinery |
| Info | 0 | Existing sealing, pooling, and value triangles already cover common advice |

> These recommendations are AI-assisted and may miss issues or suggest changes
> that are inappropriate for a particular workload. Verify correctness with
> independent tests and performance with repeatable benchmarks before adoption.

[benchmark-results]: ../../../../../../BENCHMARK.md
[root-readme]: ../../../../../../README.md
[package-readme]: ../../README.md
[mapbox-factory]: MapboxDelaunatorFactory.cs
[mapbox-result]: MapboxDelaunator.cs
[delaunay-factory]: D3DelaunayFactory.cs
[delaunay-topology]: D3Delaunay.cs
[voronoi-factory]: D3VoronoiFactory.cs
[registration]: ExtensionMethods.cs
[sites]: ../DiagramSites.cs
[coordinate-conversion]: ../ExtensionMethods.cs
[voronoi-api]: ../IVoronoi.cs
[delaunay-api]: ../IDelaunay.cs
[voronoi-result]: ../Voronoi.cs
[delaunay-result]: ../Delaunay.cs
[cell]: ../Cell.cs
[polygon]: ../../Polygon.cs
[point]: ../../Point.cs
[edge]: ../../Edge.cs
[triangle]: ../../Triangle.cs
[rect]: ../../Rect.cs
[limits]: ../../GeometryLimits.cs
[searchable]: ../QuadTreeSearchableVoronoi.cs
[indexed-cell]: ../IndexedCell.cs
[benchmark-directory]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.Benchmarks/DelaunayVoronoi/D3
[workload-benchmarks]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.Benchmarks/DelaunayVoronoi/VoronoiWorkloadBenchmarks.cs
[triangle-benchmarks]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.Benchmarks/TriangleRepresentationBenchmarks.cs
[core-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/D3/MapboxDelaunatorFactoryTests.cs
[diagram-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/DiagramRegressionTests.cs
[voronoi-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/D3/D3VoronoiFactoryTests.cs
[storage-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.UnitTests/TriangulationStorageTests.cs
[polygon-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.UnitTests/PolygonRegressionTests.cs
[allocation-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.UnitTests/AllocationRegressionTests.cs
[delaunay-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.UnitTests/DelaunayVoronoi/DelaunayTests.cs
[search-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/SearchableVoronoiRegressionTests.cs
