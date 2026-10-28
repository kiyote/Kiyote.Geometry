# D3 performance and memory refactor - completed

**Closed: 2026-10-28. Implementation and experiment results through `c2e7752`.**
This refactoring effort is complete. There are no scheduled implementation
steps or experiments remaining in this plan. Completion does not mean every
proposed optimization was implemented: measured regressions were rejected, and
optional or research alternatives were closed without implementation.

This document records the retained design, final dispositions, and validation.
The original implementation recipes and prioritization remain in Git history.
[BENCHMARK.md][benchmark-results] preserves the detailed dated measurements,
reference revisions, rejected experiments, runtime versions, and limitations.
The [root][root-readme] and [package][package-readme] documentation describe the
current consumer contracts.

## Final disposition

The original decision policy preferred runtime improvements, then allocation
and retained-memory reductions. Correctness, explicit failure, immutable
ownership, and singleton concurrency remained gates. API compatibility was not
a constraint; repository consumers were migrated rather than given allocating
compatibility adapters. No new production dependency was introduced.

| Option | Final outcome | Retained result or reason for closure |
|---|---|---|
| G0 | Implemented | Seed/degeneracy fixes, growable legalization stack, explicit insertion failures, exact mesh/geometry and ownership gates |
| O1 | Implemented local cleanup; later navigation variants rejected | Local span/cursor and invariant hull-hash improvements remain; two cached-remainder variants failed the broad runtime gate |
| O2 | Implemented | Contiguous cell metadata/vertices, ID/span API, CSR adjacency, searchable-ID migration and consumer updates |
| O3 | Implemented scratch and scalar arithmetic; linear simplifier rejected | Bounded stack scratch and explicit squared-distance multiplication remain; original cyclic simplification retained |
| O4 | Implemented tiny-hull path | Exact strict triangle/quadrilateral normalization; original general fallback retained |
| O5 | Implemented | Shared cell/adjacency walk and readonly non-owning traversal view |
| O6 | Implemented publication subset | Compact pre-sized directed-edge keys; selective-output API not pursued |
| O7 | Implemented preprocessing fusion | Owned validation/snapshot/conversion; sanitized coordinate arrays sized for accepted sites; public span overloads not pursued |
| O8 | Implemented | Infinite-cell rays computed from adjacent hull sites on demand |
| O9 | Experiment rejected | Traversal-index/circumcentre pooling reduced allocation but repeatedly slowed grid workloads; prototype removed |
| O10 | Implemented direct materialization | Exact-sized final Delaunay arrays; indexed public triangle representation and further capacity policies not pursued |
| O11 | Not pursued | Alternative duplicate detection and radial sorting were conditional proposals, not required deliverables |
| O12 | Not pursued | Coordinate-layout and SIMD alternatives were not justified for implementation in this effort |
| O13 | Not pursued | Parallel cell construction remained higher-risk research |
| O14 | Not pursued | Neighbour-half-plane clipping remained an unproven alternative algorithm |

Unimplemented portions are not an active backlog or promises of additional
speedups. Reopening any alternative requires a separate scope decision, a fresh
baseline, and correctness and equivalent-work evidence.

## Retained architecture

### Input preparation and triangulation

- [DiagramSites][sites] validates and owns a stable site snapshot. Without
  sanitization, it also fills double coordinates in the same pass.
  Sanitization preserves accepted first occurrences, then allocates coordinates
  for those sites only. The initial sanitized snapshot is still input-sized
  before compaction; that temporary allocation was not eliminated.
- [MapboxDelaunatorFactory][mapbox-factory] performs seed selection, radial
  sorting, hull insertion and legalization. It rents temporary integer/distance
  scratch separately from owned mesh outputs and returns rentals on failure
  as well as success. The pending-edge stack has a cold growable fallback.
- [MapboxDelaunator][mapbox-result] owns triangle/halfedge capacity arrays and a
  hull array. Only logical populated mesh prefixes are exposed. These internal
  capacity arrays are not trimmed; reduced B/op is not a promise of minimum
  retained capacity.
- [D3DelaunayFactory][delaunay-factory] fills public triangles and hull points
  directly into exact-length final owned arrays, without intermediate lists.
  It also builds the traversal indices used by Voronoi construction.
- [D3Delaunay][delaunay-topology] is a readonly, non-owning ref struct over
  traversal-index spans, passed by readonly reference in hot paths.

The point snapshot is retained in the result and is not disposable conversion
scratch. The separate double-coordinate array is temporary. Snapshot fusion
removes a pass; it does not remove the need for independently owned sites.

### Voronoi construction and publication

[D3VoronoiFactory][voronoi-factory] collects a cell's circumcentres and ordered
neighbour IDs in the same topology walk, including the terminal hull neighbour.
Infinite-cell rays are derived on demand rather than stored for every site.
Clipping and normalization reuse call-local bounded stack buffers across cells.

Each result owns:

- The site snapshot in cell-ID order.
- One exact-length [Cell][cell] metadata array.
- One exact-length final vertex array shared by all cell ranges.
- Two CSR integer arrays for neighbour offsets and IDs.
- The final distinct directed polygon edges.

Normalized vertices are appended to a growable per-build buffer initially sized
from mesh adjacency, then copied once into the final owned array. Builder
capacity is not retained and cells are not clipped twice. Bounds preserve the
extrema of all floored clipping candidates, not newly tightened hull bounds.
Tiny and general hull kernels are shared with [Polygon][polygon], while its
standalone publication keeps its own small scratch/direct ownership path.

No per-cell polygon, vertex-array, edge-array, or boxed-interface object graph
is retained. Global edges and adjacency remain eager; no selective or lazy
creation mode was added. Edge deduplication uses pre-sized compact directed
keys and preserves first-seen publication order.

### Consumer API

[IVoronoi][voronoi-api] exposes ID-based geometry:

- `Cells` is a `ReadOnlySpan<Cell>`; use `Cells.Length` and ref-readonly access.
- The center of cell `i` is `Points[i]`, not a property holding another object.
- `GetCellVertices(i)` and `GetNeighbourIndices(i)` return allocation-free
  read-only slices.
- `Contains(i, point)` performs inclusive containment without constructing a
  polygon.
- `ExportPolygon(i)` explicitly allocates an independently owned standalone
  polygon and its edges. Ordinary reads do not create or cache polygons.

Search results and visitors return cell IDs. Index entries retain an ID and a
bounds snapshot; quadtree enumeration reads entries by reference to avoid
copying those values in hot visits. Equal cell bounds do not merge identities.
All repository visualizers, fixtures, benchmarks and tests use the flat API.

## Preserved geometry and lifetime contracts

- Accepted positions lie in `[-32768, 32767]`, inclusive. Shared `Point` storage
  remains signed 32-bit because displacements and other consumers have broader
  requirements.
- Factories require at least three distinct, noncollinear sites. Sanitization
  does not excuse out-of-domain coordinates or post-filter degeneracy.
- Rectangle endpoints are inclusive; clipping uses `X1/X2/Y1/Y2`.
- Circumcentre and clipping intermediates remain double precision. Only final
  vertices are floored, including negative values. Generated rounded cells use
  a convex hull to repair quantization concavity; collapsed cells fail explicitly.
  Public polygon construction still rejects arbitrary invalid input.
- Traversal, projected, clipped and final normalized capacities remain distinct:

  | Stage | Vertex capacity |
  |---|---:|
  | Cell traversal | 64 |
  | Projected open cell | 66 |
  | Clipped candidates | 140 |
  | Final normalized polygon | 128 |

  An open cell at the traversal limit may have more than 64 final corners.
  Overflow and collapse are explicit failures, not truncated output.
- Neighbours preserve the original one-ring order and reciprocal completeness,
  including cocircular topology whose dual edge can have zero length.
- Global edges are directed final quantized polygon sides. Reversing endpoints
  produces a distinct edge; they are not interchangeable with primal mesh edges.
- Results do not borrow mutable caller arrays or returned pool storage. They
  remain stable after input mutation, later calls, failures and concurrent
  creation. Factories remain stateless singleton services; results need no
  disposal.

## Measurement conclusions

The largest accepted representation change was O2 (`f880ab7`), compared with its
matched `e08d491` reference:

- The 40,000-site Voronoi factory observed mean fell from 50.73 to 36.45 ms
  (28.2%), with 29.4% fewer allocated bytes.
- Standalone retained diagrams were approximately 54-57% smaller; combined
  retained diagrams and search indexes were approximately 33-37% smaller.
- Index-only construction allocation increased by 20.4%/35.1% for the two
  measured sizes because entries now store ID plus bounds. That cost is not
  hidden by the diagram's reduction.
- Retained traversal and containment improved without allocation. Small-input
  creation timings were mixed; there is no blanket speedup claim.

These are workload-specific matched results, not cumulative percentages across
all commits. Earlier incremental core/large-Delaunay speed claims were not
reproduced by controlled remeasurement and are not claimed as final gains.
Allocated bytes, retained managed heap, stack capacity, peak live memory and
working set are different measurements; peak-memory reduction was not directly
established. Managed-thread trace samples include waits/runtime work and are not
CPU-cycle or disjoint phase measurements.

### Accepted arithmetic exception

O3 explicit squares (`043eb56`) removed six demonstrated general-power calls
and reduced `Step` Tier1 code from 998 to 704 bytes. Supported bounded
squared-distance arithmetic and tie behavior are unchanged. Longer matched
timing intervals overlapped on every workload; no general runtime or memory
improvement is claimed. This simple exact cleanup was an explicitly documented
exception to the suggested speed-improvement threshold.

### Rejected experiments

| Experiment | Outcome |
|---|---|
| O9 scoped pooling (`7ca93bf`) | Lower allocation did not offset repeated grid runtime regressions; prototype and candidate-only tests removed |
| O3 linked linear simplification (`47e9766`) | Exact cyclic variants slowed the 400-site full consumer by 6.3%/8.0%, saved no managed allocation and added 560 stack bytes; original scan restored |
| O1 cached-remainder navigation (`c2e7752`) | Two variants reduced remainder instruction sequences but introduced branches and failed the broad performance gate in four serial 22-case suites; original kernel restored |

O1 branch counts and disassembly remain useful evidence, not proof that all core
opportunities are exhausted. No diagnostic counters or rejected production
paths remain. Smaller assembly or better asymptotic complexity alone did not
justify retaining a slower implementation.

## Final verification and retained evidence

The final production/test revision, `c2e7752`, passed:

- A Release solution build with zero warnings and errors.
- All 486 unit tests and 185 integration tests.
- All eight original exact full-output Voronoi fingerprints, unchanged.
- Five pre-edit mesh-ordering fingerprints: four profile fixtures and a
  128-seed full-domain corpus containing 61,972 sites.

The retained gates cover:

- [Exact mesh invariants][mesh-tests] with independent `Int128` in-circle
  checks, site participation, winding, reciprocal halfedges and hull membership.
- [Frozen mesh ordering][ordering-tests] and [full diagram output][clipping-tests].
- [Storage ownership][storage-tests], dirty scratch reuse, concurrency and
  failure recovery, plus the growable legalization stack.
- [Flat Voronoi storage][voronoi-storage-tests], contiguous ranges, export,
  inclusive containment, allocation budgets and concurrent result stability.
- [Rounded hulls][rounded-tests] against an independent exact oracle, and
  [diagram regressions][diagram-tests] for limits and nearest-site geometry.
- [Search regressions][search-tests] for visitor behavior, ID multiplicity and
  bounding-box equivalence.

Benchmark infrastructure retains checked/unchecked grid core cases, both public
factories, four broader deterministic fixtures, full Voronoi consumers,
retained traversal, containment and searchable-index operations. Diagnostic
counters stayed outside timed production measurements. Temporary reference
worktrees were removed.

The measurements and rejected outcomes in [BENCHMARK.md][benchmark-results]
are the final evidence for this effort, not instructions to resume experiments.

[benchmark-results]: ../../../../../../BENCHMARK.md
[root-readme]: ../../../../../../README.md
[package-readme]: ../../README.md
[mapbox-factory]: MapboxDelaunatorFactory.cs
[mapbox-result]: MapboxDelaunator.cs
[delaunay-factory]: D3DelaunayFactory.cs
[delaunay-topology]: D3Delaunay.cs
[voronoi-factory]: D3VoronoiFactory.cs
[sites]: ../DiagramSites.cs
[voronoi-api]: ../IVoronoi.cs
[cell]: ../Cell.cs
[polygon]: ../../Polygon.cs
[mesh-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/D3/DelaunatorInvariantTests.cs
[ordering-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/D3/DelaunatorOrderingRegressionTests.cs
[clipping-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/D3/VoronoiClippingRegressionTests.cs
[storage-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.UnitTests/TriangulationStorageTests.cs
[voronoi-storage-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.UnitTests/DelaunayVoronoi/VoronoiStorageTests.cs
[rounded-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.UnitTests/RoundedPolygonTests.cs
[diagram-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/DiagramRegressionTests.cs
[search-tests]: ../../../../../../test/Kiyote/Geometry/Kiyote.Geometry.IntegrationTests/DelaunayVoronoi/SearchableVoronoiRegressionTests.cs
