# Correctness and Performance Alignment Plan

## Objective and scope

Address all eleven findings from the library review while preserving the project's
performance-first, minimum-allocation design. Correctness means reliable behavior
inside a deliberately supported domain, not support for every possible integer,
polygon, or workload.

The product/API decisions below have been agreed in a question-by-question review.
Implementation has not started. Checked contract items record decisions, not
completed fixes; unchecked work items describe the implementation and verification
still required. Breaking changes are acceptable. Prefer narrower, explicit
contracts over costly generality. Use the existing NUnit and BenchmarkDotNet
infrastructure; no new runtime or test dependencies are required.

The obsolete `Grids` tests, helpers, benchmark, registration, and published results
have already been removed. Do not restore that namespace. The review baseline
after cleanup was 258 passing unit tests and 52 passing integration tests, with a
successful Release benchmark build. Temporary review probes reproduced the issues
below but were removed; their cases must become permanent regression tests.

## 1. Approved contracts

### Geometry size and arithmetic

Confirmed use case: geometry algorithms primarily operate in screen space, normally
around `(0, 0)..(1920, 1080)`. Support for geometry larger than 65,536 is not
required. Favor a bounded screen-space contract over support for arbitrarily large
world positions. Full-range `int` geometry is not a requirement.

Each coordinate of supported geometry is in `[-32768, 32767]`, inclusive.
This is 65,536 coordinate positions per axis, with a maximum per-axis difference
of 65,535. Negative coordinates allow geometry to extend left of or above the
screen origin before clipping. Retain `int` coordinate storage; this contract
does not require switching to `short`. Topology/grid assemblies have a separate,
larger coordinate domain: signed 32-bit coordinates on each axis. The screen-space
restriction must not implicitly constrain them. Reject unrepresentable placement
bounds, dimensions, and translations rather than allowing arithmetic wraparound.
Coordinate addressing does not imply that arbitrarily large grids can be allocated.

- [x] Establish the intended workload: bounded screen-space geometry, with no
  requirement to support geometry larger than 65,536.
- [x] Support bounded negative coordinates for partially off-screen geometry.
- [x] Keep topology/grid coordinates independent of the screen-space geometry
  limit. Retain shared `int`-based `Point`/`Rect` storage instead of introducing
  separate screen-space and world-space types. Enforce screen-space restrictions
  at bounded geometry algorithm boundaries, not on every shared constructor.
- [x] Retain signed 32-bit topology coordinates with overflow-safe bounds.
  Treat neighbours beyond the representable coordinate interval as outside the
  domain, not as wrapping to the opposite extreme. Test supported edge positions,
  rejected overflowing placements, and unchanged state after rejection.
- [x] Use the inclusive per-axis coordinate interval `[-32768, 32767]`, rather
  than allowing positive coordinates through 65,535.
- [x] Shared coordinate translation and rectangle-bound operations remain usable
  across the topology domain, with checks against unrepresentable arithmetic.
  A shared point at `x=32768` is valid storage but is rejected by a bounded geometry
  algorithm. Distinguish positions from displacement vectors and sizes; a bounded
  geometry calculation may legitimately have a difference larger than 32,767.
  Topology dimensions and allocations must be representable and capacity-checked.
- [x] All coordinates participating in a supported geometry operation use the
  same fixed interval. Their combined per-axis difference is at most 65,535;
  individually small shapes do not permit arbitrarily distant coordinates.
- [ ] Implementation prerequisite, not an unresolved product decision: derive
  bounds for intermediate differences, products, sums, indices, and buffer sizes
  from the approved domains and count limits. Include accumulation length.
- [x] Enforce the supported coordinate range at suitable public construction,
  factory, and operation boundaries in Release as well as Debug. Reject invalid
  input with clear argument exceptions; do not silently clamp or wrap it.
  Internal pixel, edge, and cell kernels may trust validated inputs rather than
  repeating range checks. Measure overhead for frequently constructed primitives.

Documenting a size limit alone does not resolve the observed arithmetic bugs.
The original crossing-diagonal probe used a 65,536-unit span, now outside the
supported domain, so replace it with an in-domain reproducer. An axis-aligned
edge from `(-25000, 0)` to `(25000, 0)` still has length 50,000 and currently
has a `NaN` magnitude despite both endpoints being supported. Promote operands
before arithmetic where required; assigning an overflowing `int` expression to
a wider result is too late.

Prefer `long` or appropriate floating-point intermediates where the supported
domain proves them sufficient. Do not introduce `Int128`, arbitrary precision, or
general full-range geometric-predicate support. Test integer extremes for topology
bounds and safe shared-coordinate operations; test rejection at the boundaries of
bounded geometry algorithms instead of requiring those algorithms to support them.

### Shape, boundary, and ownership contracts

- [x] Support empty rectangles: `default(Rect)` represents empty, not a cell at
  the origin. `Rect(0, 0, 1, 1)` represents that cell. Failed intersections can
  return the supported empty value without creating a misleading occupied cell.
- [x] Normalize a rectangle with zero width or zero height to the canonical
  empty value, discarding its origin and other extent. Negative dimensions are
  invalid and throw, including when the other dimension is zero. Empty rectangles
  do not represent lines; use an edge for that purpose.
- [x] Preserve inclusive rectangle bounds: `Rect(0, 0, 10, 10)` covers `0..9`
  on each axis. Overlap requires at least one shared cell; the rectangle starting
  at `x=10` is adjacent, not overlapping. Keep concrete/interface overloads
  consistent and remove or consolidate the contradictory older overlap method.
- [x] Use practical empty containment: `Contains(empty)` is false, including
  when the receiver is empty. Empty rectangles contain no points and overlap
  no occupied cells.
- [x] Preserve broad `Touches` semantics: true for overlap or immediate
  eight-way adjacency, including corner-only adjacency. Empty rectangles never
  touch. This is intentionally broader than non-overlapping adjacency alone.
- [x] Define segment intersection as any shared point, including shared endpoints
  and collinear overlapping portions, not only proper crossings.
- [x] Keep a unique-point intersection method rather than adding a structured
  intersection result. The boolean intersection predicate returns true for
  overlapping lengths, while the unique-point method returns false for them.
  Name and document that distinction explicitly. A shared endpoint is a unique
  point and succeeds; do not choose an arbitrary endpoint of an overlapping length.
- [x] Support zero-length segments as points. A point-segment intersects a
  nonzero segment exactly when the point lies on it; two point-segments intersect
  exactly when their points agree. A successful unique-point query returns that
  point.
- [x] Return integer unique-intersection coordinates rounded to nearest, with
  exact halves rounded to even. Determine intersection existence before rounding;
  keep intermediate calculations unrounded. Document that the returned grid
  approximation need not lie exactly on the mathematical segments.
- [x] Use directed interpolation: parameter `0` returns the first endpoint and
  `1` returns the second. Intermediate results use nearest-even integer rounding.
  Reject parameters outside `[0, 1]`, NaN, and infinity; do not extrapolate.
  Replace the misleading fractional `GetMidpoint` name with an appropriate
  interpolation name.
- [x] Define polygon equivalence as the same geometric boundary, ignoring the
  starting vertex, clockwise/counterclockwise traversal, and redundant collinear
  vertices along an edge. This is not exact vertex-sequence equality or comparison
  of collision-prone packed coordinate keys.
- [x] Normalize harmless polygon input redundancy at construction: repeated
  closing vertices, consecutive duplicates, and intermediate collinear vertices.
  Reject concave, self-intersecting, or incorrectly ordered input rather than
  sorting it into a different shape. Validate positive area after normalization.
- [x] Support an explicit empty polygon value for absence of a polygon result.
  Reject degenerate nonempty construction input: a nonempty polygon must enclose
  positive area and have at least three distinct, noncollinear corners. Points
  and edges, not degenerate polygons, represent lower-dimensional geometry.
- [x] Use inclusive polygon containment: edge/corner points count as contained;
  the entire nonempty target polygon must lie inside or on the boundary. Identical
  nonempty polygons contain each other. `Contains(empty)` is false.
- [x] A polygon intersection predicate detects any shared point, including
  touching and full containment. Polygon-valued clipping succeeds only for a
  positive-area polygon; edge-only/point-only contact returns false and empty,
  not an exception. Name any boundary-crossing-only operation distinctly.
- [x] Keep clipped polygons on the integer grid: calculate without premature
  rounding, then round output vertices to nearest-even and remove redundancies.
  If quantization collapses the result to zero area, return false and empty.
  Document that snapping approximates the mathematical intersection and may move
  boundaries slightly; the returned polygon must still satisfy the convex invariant.
- [x] Restrict polygon clipping/intersection construction to convex polygons.
  Validate and reject concave inputs at the public boundary; do not approximate
  them or add general multi-region/hole clipping.
- [x] Restrict filled polygon rasterization to convex polygons. Reject concave
  filled inputs before emitting any pixels; do not implement a multi-span concave
  fill algorithm. This restriction does not by itself forbid concave outline tracing.
- [x] Make the polygon type itself convex-only, with the invariant established
  at construction. Do not retain a general concave polygon representation merely
  for point containment or outlines. Public raw-vertex operations must also honor
  their declared shape contracts; validated polygon overloads should reuse the
  invariant instead of rescanning vertices.
- [x] Use an adjustable compile-time hard maximum for Voronoi cell complexity,
  not merely an initial capacity with an unrestricted growth path. Exceeding the
  supported maximum must produce a clear exception, never buffer overflow or
  silently truncated geometry. Changing the constant requires rebuilding.
- [x] Set the initial Voronoi maximum to 64 pre-clipping vertices per cell:
  the vertices emitted by the cell traversal before projection/clipping and
  simplification. Use a named constant and test the supported boundary and
  rejection above it. Derive separate scratch bounds for projection/clipping;
  do not assume their output also fits 64 vertices.
- [x] Cap public polygons at 128 normalized vertices using a separate adjustable
  compile-time constant. Apply the limit to constructed results as well as inputs.
  If clipping would produce an over-limit polygon, fail explicitly without
  truncation; valid individual inputs do not guarantee a supported combined result.
- [x] Do not add a fixed total site-count cap for triangulation/Voronoi creation.
  Validate representable array/buffer sizes and capacity arithmetic; memory
  availability still limits allocations. Per-cell and per-polygon limits remain
  independent of the number of sites.
- [x] Reject degenerate triangulation/Voronoi inputs with a clear argument
  exception: fewer than three distinct sites or a wholly collinear site set.
  Do not add empty, one-point, two-point, or line-only diagram result models.
- [x] Reject duplicate diagram sites by default. Retain explicitly requested
  deduplication; validate that the resulting distinct sites still form a
  nondegenerate input. Reuse existing work storage where practical and measure
  default-path validation cost rather than assuming an extra hash set is acceptable.
- [x] Reject Voronoi sites outside the requested clipping rectangle by default.
  Explicit sanitization may discard them, then must revalidate the remaining
  distinct, noncollinear sites. Do not add outside-site influence or invisible-cell
  result semantics. Geometry coordinates outside the library domain remain invalid.
- [x] Constructed geometry and diagram results own stable input snapshots.
  Caller mutation/reuse of vertex arrays or site lists must not alter an existing
  result or invalidate its cached data. Permit trusted internal ownership transfer
  to avoid redundant copies; do not expose mutable result storage through ordinary
  public collection APIs.
- [x] Enforce exclusive mutable grid-source attachment: one source instance may
  belong to only one placement in one assembly at a time. Reject simultaneous
  reuse, including across assemblies. Detach releases ownership for later reuse;
  failed attachment must not retain ownership or partially change either assembly.
  Independent copies require independent sources, not implicit sharing.
- [x] Reject direct occupancy/shape changes to an attached source. Require
  assembly-coordinated add/remove operations so occupancy, seams, and compiled
  layers stay synchronized. Direct shape editing is allowed again after detach.
  This restriction does not prohibit ordinary existing-cell value edits or the
  documented commit workflow. Custom sources must honor the ownership contract.
- [x] Keep assembly/compiler/layer machinery as a closed implementation family.
  Custom sources, bindings, and connectivity strategies remain supported extension
  points. Consumption interfaces do not promise support for arbitrary replacement
  assembly implementations. Make the construction path explicit and reject
  unsupported implementations early, rather than accepting partial functionality.

Record the decisions in the public XML documentation, the
[package README](src/Kiyote/Geometry/Kiyote.Geometry/README.md), and the
[root README](README.md). Keep validation outside hot loops where possible.
Unsupported input must not produce plausible but incorrect results. When runtime
validation is intentionally omitted, document the precondition explicitly rather
than claiming that invalid input is detected.

## 2. Required red-green workflow

Apply this workflow to each independent behavior change:

1. Add the smallest permanent regression test against the existing public behavior.
   Use independently calculated expected values, not another implementation of the
   same faulty formula.
2. Run the targeted test against the unfixed implementation. Confirm that it fails
   for the intended reason, and record the command and failure in the change
   description. Compilation failure or an unrelated exception is not sufficient.
3. Fix the root cause with the smallest complete implementation change.
4. Run the same test unchanged and confirm it passes. Add adjacent boundary and
   invariant cases, then run the relevant test project.
5. Build affected projects and check the relevant Release benchmarks. Run both test
   projects before completing each milestone.
6. Deliver the test and fix together in a green change; do not leave the shared
   branch intentionally failing between implementation steps.

For a newly restricted domain, first approve the restriction, then write a test
expecting the chosen rejection behavior instead of requiring support that has been
explicitly excluded. Never weaken an in-domain regression merely to make it pass.

For API-breaking changes, first capture the old behavioral defect where possible,
then port the assertion to the new API. Explain the API migration separately from
the red-green evidence.

Performance-only work is different: a timing result is not a deterministic unit
test. Preserve behavior with tests and demonstrate improvement with controlled
before/after benchmarks. Use allocation regression assertions only after defining
a reproducible workload, warm-up, and justified budget. Do not introduce flaky
wall-clock assertions or claim all pooled operations always allocate zero bytes.

## 3. Work packages

### C1. Bound Voronoi scratch storage correctly

Review finding 1. Primary implementation:
[D3VoronoiFactory.cs](src/Kiyote/Geometry/Kiyote.Geometry/DelaunayVoronoi/D3VoronoiFactory.cs).

**Red tests**

- Adapt the reproduced high-degree case to the approved domain: a center site at
  `(0, 0)` surrounded by 40 integer-rounded sites on a radius-10,000 circle,
  within bounds `Rect(-20000, -20000, 40001, 40001)`. The original translated
  case threw `IndexOutOfRangeException`; confirm the same intended red failure
  for this bounded fixture before fixing it.
- Cover the old 25-vertex boundary and the approved hard maximum: initially 63,
  64, and 65 pre-clipping vertices. The first two must succeed and the last must
  fail with the documented exception, not `IndexOutOfRangeException`. Derive
  limit-boundary fixtures from the named constant so raising it updates coverage.
- Include open hull cells and their projected/clipped scratch requirements.
- Add rejection tests for too few distinct sites, collinear sites, duplicate
  sites on the default path, and outside-rectangle sites on the default Voronoi
  path. Test explicit sanitization and revalidation after cleanup.
- Cover coordinate validation, unrepresentable capacity calculations, and input
  snapshot stability without attempting impractically large allocations.
- Assert usable cell geometry and valid neighbour relationships, not merely a
  non-null result.

**Fix and acceptance**

- Replace the undocumented 25-vertex assumption with the named, adjustable hard
  maximum of 64 pre-clipping vertices. Enforce it before an out-of-bounds write;
  do not add an unrestricted growth path.
- Derive separate capacities for projection and clipping from the supported
  pre-clipping limit. Respect the separate 128-normalized-vertex polygon limit.
- Validate cell, projected, and clipped buffer capacities and exception-path returns.
  Benchmark representative low-degree inputs and the supported high-degree case.

### C2. Repair quadtree storage and query invariants

Review finding 2. Primary implementation:
[SimpleQuadTreeNode.cs](src/Kiyote/Geometry/Kiyote.Geometry/Trees/SimpleQuadTreeNode.cs).

**Red tests**

- Insert `Rect(1, 1, 2, 2)` into a `Rect(0, 0, 100, 100)` tree; querying the item
  must return it exactly once. The current tree stores one item but returns none.
- Insert into a `2 x 100` tree; current subdivision throws. Include the transposed
  case and nonempty one-cell-wide/high bounds, including `1 x 1`.
- Exercise odd-sized bounds, split boundaries, items spanning children, empty
  queries, and queries containing a subtree.
- Compare fixed-seed randomized queries with a simple brute-force reference using
  the agreed rectangle semantics.

**Fix and acceptance**

- Distinguish leaf status from subtree emptiness.
- Make subdivision respect inclusive bounds and both dimensions; do not create
  invalid children or omit valid boundary cells.
- Define explicit failure for out-of-bounds insertion instead of silently dropping
  data. Verify rejected insertion leaves the tree unchanged.
- Include searchable-Voronoi integration coverage after the tree fixes.

### C3. Correct polygon containment, intersection, and equivalence

Review finding 3. Primary implementation:
[Polygon.cs](src/Kiyote/Geometry/Kiyote.Geometry/Polygon.cs).

**Red tests**

- A square from `(0, 0)` to `(10, 10)` must not contain a square from `(5, 5)` to
  `(15, 15)`. One contained vertex is not whole-polygon containment.
- Intersecting that outer square with the square from `(2, 2)` to `(4, 4)` must
  return the inner region even though their boundaries never cross.
- Triangles `[(0,0), (1,0), (0,2)]` and `[(0,0), (0,1), (2,0)]` must not compare
  equivalent. The current 32-bit shift-based key collides.
- Cover identical, disjoint, touching, reversed, cyclically reordered,
  repeated-vertex, collinear-redundancy, and empty inputs.
- Reject concave, self-intersecting, and degenerate nonempty construction input.
  Test the 128-normalized-vertex limit, over-limit clipping results, and input
  redundancy that normalizes to a supported size.
- Verify that edge/corner contact and containment count as boolean intersection,
  while polygon-valued clipping returns false and empty for zero-area contact.
- Exercise fractional intersections, nearest-even output rounding, and results
  that collapse to empty after quantization.

**Fix and acceptance**

- Separate containment, boundary intersection, and area overlap semantics.
- Normalize and validate owned convex polygon storage once, then implement
  geometric-boundary equivalence independent of start and winding.
- Replace centroid-angle sorting with a suitable convex clipping algorithm.
  Do not add a general concave/multi-region clipping subsystem.
- Keep intermediate calculations unrounded, then produce the documented
  nearest-even integer-grid approximation. Remove redundancies, handle collapse,
  and maintain the convex invariant; do not silently return a concave result.
- Test exact expected vertices, area, and boundary behavior under the selected
  rounding rules. Do not validate clipping using the equivalence routine being fixed.

### C4. Correct arithmetic within the supported domain

Review finding 4. Primary implementations:
[Intersect.cs](src/Kiyote/Geometry/Kiyote.Geometry/Intersect.cs),
[Point.cs](src/Kiyote/Geometry/Kiyote.Geometry/Point.cs), and
[Edge.cs](src/Kiyote/Geometry/Kiyote.Geometry/Edge.cs).

**Red tests**

- Crossing edges `(-25000,-25000)..(25000,25000)` and
  `(-25000,25000)..(25000,-25000)` must intersect at `(0,0)`. Confirm this bounded
  overflow reproducer fails for the intended reason before applying the fix.
- A horizontal edge from `(-25000,0)` to `(25000,0)` must have finite magnitude
  50,000.
- Squared distance and polygon orientation must remain correct near the supported
  maximum; both axes should be exercised together.
- Quarter interpolation on `(0,10)..(10,0)` must remain on that edge under the
  chosen integer-rounding policy. Cover both directions and interpolation endpoints.
- Test nearest-even ties, rejected extrapolation/NaN/infinity, point-segments,
  shared endpoints, and collinear overlaps. An overlapping length must satisfy
  the intersection predicate but fail the unique-point query.
- Replace the review's full-`int` adjacency/distance expectations with supported-
  boundary and explicit out-of-domain rejection tests. Independently test full-
  `int` topology boundaries and shared-coordinate arithmetic without wraparound.

**Fix and acceptance**

- Widen before arithmetic and preserve direction in interpolation.
- Review related orientation, centroid, rectangle endpoint, raster error, loop-end,
  grid offset, and allocation-size arithmetic for the same in-domain failure mode.
- Use bounded deterministic properties: symmetry of distance/intersection,
  translation invariance within the supported domain, and interpolation endpoints.
- Benchmark representative primitive operations after fixing benchmark consumption.
  Do not pay for arbitrary precision when bounded arithmetic is sufficient.

### C5. Unify rectangle semantics

Review finding 5. Primary implementations:
[Rect.cs](src/Kiyote/Geometry/Kiyote.Geometry/Rect.cs) and
[IRect.cs](src/Kiyote/Geometry/Kiyote.Geometry/IRect.cs).

**Red tests**

- Adjacent `Rect(0,0,10,10)` and `Rect(10,0,10,10)` currently produce asymmetric
  results from the older overlap method.
- Assert canonical empty results for either zero dimension and `default(Rect)`;
  reject negative dimensions even when the other dimension is zero.
- Assert that empty rectangles contain neither points nor rectangles and never
  overlap or touch. Union with empty should leave a nonempty rectangle unchanged.
- Cover shared-cell overlap, eight-way adjacency, and intersection failure.
  `Touches` includes overlapping rectangles, not only adjacent ones.
- Check concrete and interface overloads for identical results.

**Fix and acceptance**

- Consolidate overlapping APIs around one contract; remove redundant public
  operations if they no longer serve distinct purposes.
- Validate both dimensions and endpoint arithmetic.
- Re-run quadtree, polygon, and topology tests that depend on rectangle bounds.

### C6. Make rasterizer shape support explicit

Review finding 6. Primary implementations:
[IntegerRasterizer.cs](src/Kiyote/Geometry/Kiyote.Geometry/Rasterizers/IntegerRasterizer.cs)
and [IRasterizer.cs](src/Kiyote/Geometry/Kiyote.Geometry/Rasterizers/IRasterizer.cs).

**Red tests**

- Use `[(0,0), (6,0), (6,6), (4,6), (4,2), (2,2), (2,6), (0,6)]`.
  The current fill emits `(3,4)`, which lies in the notch.
- Pass the concave fixture through the public raw-vertex filled overload and
  assert rejection before any pixels are emitted. Separately assert that convex-
  only polygon construction rejects it.
- Cover empty/degenerate inputs, border-only rendering, horizontal edges, reversed
  winding, and the supported coordinate boundary.

**Fix and acceptance**

- Keep the single-span fast path for validated convex polygons.
- Do not implement concave multi-span filling. Validate raw polygon inputs at
  the public boundary; reuse the invariant of already validated polygon objects.
- Preserve struct/ref-struct sinks, pooled scratch storage, and span emission.
  Validate emitted runs as well as individual pixels.

### C7. Establish geometry and source ownership

Review finding 7. Primary implementations:
[Polygon.cs](src/Kiyote/Geometry/Kiyote.Geometry/Polygon.cs) and
[GridAssembly.cs](src/Kiyote/Geometry/Kiyote.Geometry/Topology/GridAssembly.cs).

**Red tests**

- Construct a polygon from an array, then change the array. Public vertices and
  cached edges currently disagree. Assert the chosen immutable ownership contract.
- Construct diagram results from a site list, mutate/reuse the list, and assert
  that the results and their cached topology remain unchanged.
- Attempt to attach one source at two non-overlapping positions. Require explicit
  rejection of the second attachment, reproducing the old aliasing defect without
  making shared attachment a supported operation.
- Exercise source reuse across assemblies, detach/reattach, and rejected attachment
  without partial changes.
- Reject direct source shape mutation while attached; verify assembly-coordinated
  edits still work and direct editing becomes available after detach. Cover a
  custom source implementing the approved lifecycle contract.

**Fix and acceptance**

- Own immutable geometry and diagram input storage. Permit trusted internal
  ownership transfer so factory-created geometry avoids unnecessary second copies.
- Enforce exclusive attachment across assemblies and custom sources, and require
  coordinated mutation while attached. Use an explicit source ownership/lifecycle
  contract rather than an unbounded global ownership registry.
- Do not implement shared-source propagation.
- Preserve documented source-value edits and commit behavior.

### P1. Repair the measurement foundation

Review finding 8. Primary files:
[EdgeBenchmarks.cs](test/Kiyote/Geometry/Kiyote.Geometry.Benchmarks/EdgeBenchmarks.cs),
[IntersectBenchmarks.cs](test/Kiyote/Geometry/Kiyote.Geometry.Benchmarks/IntersectBenchmarks.cs),
[RectangleBenchmarks.cs](test/Kiyote/Geometry/Kiyote.Geometry.Benchmarks/RectangleBenchmarks.cs),
and [BENCHMARK.md](BENCHMARK.md).

- [x] Return or explicitly consume all relevant results in the 15 identified
  discarded-output benchmark methods. Consume intersection outputs as well as
  success flags where appropriate.
- [x] Correct benchmark names/overloads and use nonconstant input fields where
  constant folding could invalidate a measurement.
- [x] Add representative small, maximum-supported, irregular, fragmented, and
  high-degree workloads. Do not rely solely on regular grids.
- [x] Establish a Release baseline with recorded SDK/runtime, machine, inputs, and
  configuration for matched before/after comparisons. Inspect generated code for
  suspicious zero-work timings when necessary.
- [x] Replace affected published results with measured results and their environment.
  Do not invent throughput targets from the old near-zero timings.

This package does not require deliberately failing timing tests. Correct fixture
data should have independently verifiable expected results. Establish valid
baselines before accepting performance changes.

P1 implementation evidence:

- Benchmark results are now consumed, including polygon and factory results beyond
  the original 15 methods. Tree insertion resets outside each measured iteration;
  the harness confirmed one invocation/unroll, with no accumulating mutation.
- The isolated-process Release run attempted 70 cases. 69 produced measurements;
  HighDegree Voronoi failed setup with the expected `IndexOutOfRangeException`
  in cell traversal. Its timing is explicitly deferred to C1, not silently skipped
  or treated as successful. The runner returned exit code 1.
- Narrow disassembly of both Edge constructors, the Point constructor, and
  integer-coordinate Rect containment confirmed actual input loads/output work.
  Near-zero durations remain below useful timing resolution and are qualified
  accordingly in [BENCHMARK.md](BENCHMARK.md).
- All 258 unit tests and 52 integration tests pass in Release. The benchmark
  project builds with zero warnings/errors. No production correctness changes
  are included in P1; the red-green requirement starts with the C packages.

### P2. Reduce allocation and copying at representation boundaries

Review finding 9. Primary areas:
[Triangle.cs](src/Kiyote/Geometry/Kiyote.Geometry/Triangle.cs),
[Polygon.cs](src/Kiyote/Geometry/Kiyote.Geometry/Polygon.cs),
[MapboxDelaunatorFactory.cs](src/Kiyote/Geometry/Kiyote.Geometry/DelaunayVoronoi/MapboxDelaunatorFactory.cs),
and [Trees](src/Kiyote/Geometry/Kiyote.Geometry/Trees).

- [ ] Benchmark a readonly value-type triangle against the current per-triangle
  object representation, including construction, traversal, and copying costs.
- [ ] Build owned polygon edge storage once, without an intermediate full copy.
- [ ] Evaluate reuse/pooling of the seven triangulation work arrays and avoidance
  of both output-trimming copies. Keep scratch ownership separate from result ownership.
- [ ] Add caller-owned destination or struct-visitor query paths so recursive tree
  traversal does not construct intermediate lists. Retain convenience wrappers.
- [ ] Avoid a second result materialization in searchable-Voronoi queries when the
  chosen query API can emit the required cells directly.

**Tests and acceptance:** Preserve all earlier correctness regressions, equality,
result lifetime, and repeated-call isolation. Verify exceptions return rented
storage and escaped results never alias returned scratch. Where stable allocation
budgets are justified, demonstrate that the old implementation exceeds the budget
and the new implementation passes. Otherwise use BenchmarkDotNet allocation data.
Accept only measured benefits that justify complexity; pooling is not an end in itself.

### P3. Make topology mutation and commit scale with affected data

Review finding 10. Primary implementations:
[GridAssembly.cs](src/Kiyote/Geometry/Kiyote.Geometry/Topology/GridAssembly.cs) and
[CompiledGridAssembly.cs](src/Kiyote/Geometry/Kiyote.Geometry/Topology/CompiledGridAssembly.cs).

- [ ] Measure one-cell add/remove with fixed local geometry while independently
  increasing total chunks, placements, seams, and run fragmentation.
- [ ] Measure commits with no dirty chunks, one dirty chunk, and all chunks dirty.
- [ ] Where measurements justify it, index runs/seam links by chunk or placement
  and track dirty chunks rather than repeatedly scanning all mappings.
- [ ] Include index memory, build cost, mutation maintenance, and retained capacity
  in the comparison. Avoid trading small scan costs for a large object graph.

**Tests and acceptance:** Add edit-sequence tests comparing incremental state with a
fresh compilation or independent reference after additions, removals, run splits,
merges, and seam changes. Test dirty-only commits with a recording binding that
checks exactly which source cells are written. Preserve detach/staleness, disposal,
layer swapping, and source commit contracts. Timing/scaling improvements belong in
benchmarks; avoid adding production instrumentation solely to satisfy a unit test.

### A1. Make topology abstractions honest

Review finding 11. Primary implementations:
[CompiledGridAssembly.cs](src/Kiyote/Geometry/Kiyote.Geometry/Topology/CompiledGridAssembly.cs)
and [ConnectivityBuilder.cs](src/Kiyote/Geometry/Kiyote.Geometry/Topology/ConnectivityBuilder.cs).

**Red tests**

- Use a minimal alternate implementation of the advertised assembly interface.
  Demonstrate that compilation currently lacks the promised live edit propagation.
- Test early rejection at any public boundary still accepting an implementation
  interface. Do not permit partially functional compilation.
- Where the revised API prevents unsupported construction at compile time, record
  the old behavioral failure before the API change, then validate the supported
  construction path through builds and behavior tests. Do not mislabel a compile
  error as a runtime red-green reproduction.
- Verify that custom sources, bindings, and connectivity strategies remain usable
  through the supported built-in machinery, including source ownership enforcement.

**Fix and acceptance**

- Close the assembly/compiler/layer implementation family and narrow construction
  boundaries appropriately. Preserve supported custom data/strategy extension
  points and remove misleading assembly-substitutability claims.
- Keep abstraction dispatch out of per-cell loops; preserve struct strategies.
- Audit the closely related lifecycle and mutable-layout exposure. Do not make an
  unrelated class-splitting or DI-framework refactor part of this package.

## 4. Execution order and dependencies

| Milestone | Work | Prerequisites |
|---|---|---|
| 0 (complete) | Approve domain, shape, ownership, and extensibility contracts | Decisions recorded in section 1 |
| 1 (complete) | P1: repaired baseline; high-degree timing deferred to C1 | None; domain approval for boundary fixtures |
| 2 | C4 and C5: arithmetic and rectangle foundations | Milestone 0; P1 for performance comparisons |
| 3 | C1: Voronoi capacity and factory validation | Approved degree/domain contract; C3 and geometry-storage C7 for final result invariants |
| 3 | C2: quadtree correctness | C5; C4 for supported-coordinate arithmetic |
| 3 | C3: polygon semantics; geometry-storage portion of C7 | Approved polygon contract; C4 |
| 3 | C6: rasterizer contract | Approved shape contract; C4; C3 for validated polygon overloads |
| 4 | Grid-source portion of C7 and A1 | Approved ownership/extensibility contracts; C5 |
| 5 | P2: allocation/layout improvements | P1 and correctness fixes for each affected component |
| 5 | P3: topology locality | P1, grid-source C7, and A1 |
| 6 | Documentation, full validation, updated measurements | All accepted packages |

Independent regression tests can be written earlier. Keep changes grouped by
behavior rather than attempting a library-wide rewrite. P2 and P3 are conditional
optimization work: if measurements do not justify a proposed change, retain the
simpler implementation and record that decision.

## 5. Completion criteria

- [ ] Every supported-domain defect has a permanent test observed failing before
  the fix and passing afterward, with the intended failure reason recorded.
- [ ] Explicitly unsupported cases have clear documentation and the approved
  rejection tests or documented unchecked preconditions.
- [ ] All affected public overloads, factories, adapters, and consumers obey the
  same contracts; no obsolete `Grids` API is reintroduced.
- [ ] Both existing test projects pass, and affected consumers and benchmarks build
  in Release with repository warning policies enabled.
- [ ] Seeded reference/invariant tests cover boundaries and interaction cases, not
  just the original examples. Failure messages include the reproducing input.
- [ ] Public XML docs, both READMEs, and affected benchmark results reflect the
  actual supported behavior.
- [ ] Performance comparisons use valid consumed-output benchmarks. Allocation,
  throughput, and scaling tradeoffs are recorded without unsupported speedup claims.
- [ ] No unnecessary dependency, general-purpose pooling framework, arbitrary-
  precision subsystem, or interface layer has been introduced.
