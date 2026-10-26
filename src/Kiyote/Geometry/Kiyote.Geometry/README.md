# Kiyote.Geometry

Provides geometric algorithms for dependency-free operations.

## Coordinate and segment contracts

Screen-space geometry uses coordinates in `[-32768, 32767]`, inclusive, exposed by
`GeometryLimits`. Point distance/adjacency, segment intersection, length and
interpolation, and polygon construction/point containment reject out-of-range
positions with `ArgumentOutOfRangeException` in both Debug and Release.

`Point` and `Edge` still store full signed 32-bit coordinates. Shared point
translations, edge bounds, and absolute displacements are not screen-space
restricted; unrepresentable results throw `OverflowException`. A displacement
may legitimately exceed 32,767. Topology uses its separate signed 32-bit domain.

Segments include their endpoints, and zero-length segments behave as points.
`HasIntersection` includes collinear shared lengths; `TryFindIntersection`
requires a unique point. Integer intersection coordinates round to nearest,
with exact halves to even, after determining intersection existence.
`Edge.Interpolate(amount)` replaces `GetMidpoint`: it follows A to B for finite
amounts in `[0, 1]`, using the same rounding rule, without extrapolation.

## Rectangle contracts

Rectangle bounds are inclusive occupied cells: `Rect(0,0,10,10)` covers `0..9`.
`default(Rect)` and either zero dimension are canonical empty; negative dimensions
throw even if the other dimension is zero. Empty rectangles contain nothing,
never overlap or touch, and are the identity for union. They are not equivalent
to the occupied origin cell `Rect(0,0,1,1)`.

`Rect.Overlaps` replaces `HasOverlap` and requires a shared cell. `Touches`
includes overlap and immediate eight-way adjacency, including corners.
Concrete and `IRect` overloads use the same rules; `IRect` now exposes `IsEmpty`.
Dimensions and endpoints must fit signed 32-bit integers; invalid construction
or an unrepresentable union throws `ArgumentOutOfRangeException`.

## Polygon contracts

`Polygon` owns an immutable snapshot of its vertices and edges. Nonempty polygons
must be convex, have positive area, and have at most `Polygon.MaximumVertices`
(128) normalized corners. Construction removes consecutive/closing duplicates
and intermediate collinear vertices while preserving winding and boundary order.
Concave, self-intersecting, backtracking, and degenerate nonempty input throws
`ArgumentException`; arbitrary input is never sorted into a different shape.
An empty vertex collection and `Polygon.None` represent absence.

Containment includes edges and corners and requires the entire nonempty target
to be contained. Empty targets are not contained. `IsEquivalentTo` compares
geometric boundaries independently of starting vertex and winding.
`HasIntersection` (also exposed as `HasOverlap`) includes contact and containment.
`TryFindBoundaryIntersections` replaces `TryFindIntersections`: it returns
deduplicated, nearest-even point intersections of boundary edges, not containment
or an arbitrary point from a shared length. Its returned collection is read-only.

`TryIntersect` returns a polygon only when the snapped result has positive area.
It classifies contained vertices and crossings of the original edges before
rounding, then takes the convex hull of the nearest-even snapped vertices.
This convex integer approximation can move the mathematical intersection's
boundary slightly. Contact or quantization collapse returns false and
`Polygon.None`; a result exceeding 128 corners throws `InvalidOperationException`.
Scratch storage is bounded, and contained immutable operands can be reused.

Generated Voronoi cells retain floor-to-integer quantization of final vertices;
circumcentres and clipping calculations are no longer rounded prematurely.
Their convex hull removes quantization-induced concavity; a collapsed cell
throws rather than publishing a degenerate polygon. This trusted producer path
does not relax public polygon construction.

## Diagram factory contracts

Delaunay and Voronoi factories require at least three distinct, noncollinear
sites within `GeometryLimits`. Duplicates are rejected by default. Explicit
`sanitizePoints` deduplicates in first-occurrence order; for Voronoi it also
discards sites outside the clipping rectangle. Remaining sites must still be
nondegenerate. Out-of-domain coordinates are always invalid, even with
sanitization enabled.

Voronoi sites must lie inside the requested rectangle unless sanitization is
enabled. Both rectangle endpoints must be within the geometry domain, and its
inclusive width and height must each be at least two to enclose geometric area.
Invalid site sets throw `ArgumentException`; invalid coordinates, bounds, and
unrepresentable array capacities throw `ArgumentOutOfRangeException`.

`IVoronoiFactory.MaximumCellVertices` is a compile-time limit of 64 vertices
emitted by cell traversal, before projection, clipping, and simplification.
Open cells have separately bounded projection/clipping scratch; their final
polygons can have more than 64 corners, subject to the polygon limit of 128.
Exceeding the traversal or polygon limit, or collapsing a cell after final
quantization, throws `InvalidOperationException`. Increasing the constants
requires rebuilding and revisiting their derived scratch bounds.

There is no fixed total site-count cap. Required array lengths must be
representable, and allocations remain subject to available memory. Results own
stable site snapshots and read-only triangles, hulls, cells, edges, and neighbour
index spans; reusing or mutating an input array/list cannot change an existing
diagram. Quantized Voronoi polygons approximate the continuous cells, not an
exact partition of continuous space.

Triangulation seed selection uses the bounding-box center on each axis.
Triangle and hull starting indices are implementation details, not a canonical
ordering. Edge legalization grows its temporary stack when necessary rather
than discarding pending work.

Without sanitization, factories validate and snapshot sites while preparing
double-coordinate scratch in one pass. The owned point snapshot is retained by
the result; coordinate scratch is temporary. Sanitization filters sites first,
then allocates coordinates for the accepted sites only, avoiding input-sized
coordinate scratch when most sites are discarded.

Infinite-cell rays are derived from adjacent hull sites on demand, without a
four-double vector allocation for every site.

Voronoi clipping uses bounded, call-local stack scratch reused across cells.
Coordinate-pair insertion shifts the tail once; scratch never escapes into
published results. The 64/66/140 traversal/projected/clipped vertex capacities
remain distinct, with explicit overflow failures.

Voronoi adjacency is stored in two contiguous owned integer arrays rather than
a cell-keyed dictionary and per-cell collections. `GetNeighbourIndices(i)`
returns an allocation-free `ReadOnlySpan<int>` of indices into `Cells`, in
one-ring traversal order. Cell indices match site order. Resolve a neighbour as
`diagram.Cells[neighbourIndex]`. This replaces the former `Neighbours` property.
Adjacency remains eager, complete, and stable for the diagram's lifetime.

Global Voronoi edges are distinct directed polygon sides: reversing endpoints
produces a different edge. Construction uses pre-sized compact bounded-coordinate
keys internally; published edges retain full `Point` coordinates.

`Triangle` is an immutable value type with ordered structural equality, not a
nullable reference object. Its default value is degenerate and is rejected by
filled rasterization. Triangles are stored inline in collections; copying large
collections copies all vertex data, not just references.

Triangulation scratch is pooled separately from owned result storage and returned
on success or failure. Results need no disposal and never alias returned scratch.
Internal meshes expose only populated entries while retaining capacity arrays:
this avoids trimming copies but can retain nearly twice the logical mesh storage
for all-hull inputs. Pool caches also retain scratch; lower allocated bytes do
not imply an equal reduction in process working set.

## Quadtree contracts

Quadtrees use inclusive occupied-cell bounds in signed 32-bit coordinate space,
without the screen-space coordinate restriction. Construction snapshots the bounds;
empty bounds are rejected, while `1 x 1` and one-cell-wide/high trees are valid.
Children partition their parent without shared cells or gaps, including odd sizes.
Regions with area at most ten cells, or an axis that cannot split, remain leaves.
Thin terminal regions may therefore require a linear scan of their entries.

Insertions must be nonempty and fully contained by the tree; invalid insertions
throw `ArgumentException` without changing its entries or count. Each insertion
counts separately, including repeated values. Indexed item bounds must remain
unchanged; arbitrary mutable items are not cloned. Queries return entries sharing
at least one occupied cell, and empty queries return no entries.

`Count` and `IsEmpty` describe the entire subtree, not whether it has children,
and are maintained in constant time. A populated leaf is not empty.
`SimpleQuadTreeNode.Bounds` now exposes an immutable `Rect` snapshot; `Contents`
is a read-only live view of direct entries. `GetSubTreeContents` returns an
independent collection of all entries.

`Query(Rect, ref visitor)` and searchable Voronoi's `Search(Rect, ref visitor)`
accept struct or ref-struct implementations of `IQueryVisitor<T>`. Implement
`Visit(scoped in T item)` to consume each match without intermediate collections.
Traversal allocates nothing; caller-owned visitor state survives both success and
callback exceptions. Do not mutate the index or indexed bounds during a visit.
Existing collection-returning overloads remain, using a single result list.
Use the concrete `Rect` query overload to avoid boxing the query rectangle.

Searchable Voronoi queries use cell bounding boxes, not exact polygon intersections.
The searchable factory's size defines an origin-anchored index rectangle; it must
contain every cell's bounding box. Undersized or incorrectly positioned indexes
now throw rather than silently omitting cells.
Distinct cells with identical bounding boxes remain distinct search results.

### Rasterization

Rasterization accepts coordinates in `[-32768, 32767]` and validates raw input
before invoking the pixel sink. Empty boundaries emit nothing. Filled raw
boundaries and triangles must follow the convex, positive-area polygon contract,
including the 128 normalized-corner limit. Harmless repeated/collinear vertices
are normalized without constructing a temporary polygon; concave, self-crossing,
backtracking, and nonempty degenerate fills throw.

Use the line overload or `filled: false` for points and line segments. Raw
outlines may remain concave. Already constructed `Polygon` inputs reuse their
validated immutable boundary without another convexity scan. Struct/ref-struct
sinks, pooled scanline scratch, and inclusive whole-span emission are preserved.
Sink exceptions propagate to the caller and pooled buffers are returned.

`IPixelOperation.PixelSpan` is now required: inheriting its former default
implementation boxed pixel-only struct sinks once per scanline. A scalar sink
can implement it as `IPixelOperation.EmitSpan(this, xMin, xMax, y)`; whole-span
sinks should retain their specialized implementation. The helper operates on a
value copy, so observable state should be referenced/span-backed; write the loop
directly if mutable value state must persist inside the sink. Rasterizer APIs
continue to receive sinks by value. Delegate adapters also use the unboxed helper.

### Topology ownership and lifetime

Topology retains signed 32-bit coordinates, independent of screen-space geometry.
Placements must have positive, representable dimensions and endpoints. Neighbours
beyond the coordinate domain are absent, not wrapped. Dense storage and padded
connectivity scratch are capacity-checked before allocation; available memory
still limits allocations. Widely separated placements remain supported even when
their combined `Bounds` cannot fit one `Rect`; that property then throws.

A source can belong to only one placement across all assemblies. Detach releases
it for reuse. Direct occupancy changes while attached throw; use the assembly's
`TryAddCell`/`TryRemoveCell` so seams and compiled layers stay synchronized.
Existing-cell value edits and explicit dirty-layer commits remain supported.
Custom sources expose one stable `GridSourceOwnership` and call
`EnsureShapeChangeAllowed` before changing occupancy. Source queries must remain
consistent during an operation, failed mutations must leave the source unchanged,
and callbacks must not reenter the assembly. These are trusted extension contracts,
not a transactional sandbox for arbitrary custom implementations.

Assembly/compiler/layer machinery is a closed implementation family: compile a
`GridAssembly<TCell>` with `GridCompiler`. Consumption interfaces do not promise
support for replacement implementations. Custom sources, struct bindings, and
struct connectivity strategies remain extension points.

**Commit before attaching or detaching.** Every structural change makes earlier
compilations stale and stops their live edit propagation. Stale or disposed
compilations reject commits, layer management, and connectivity operations;
dispose and recompile rather than writing old data into a reused source.

Placement and layer lists are read-only live views; seams are read-only snapshots.
`GridLayout` exposes chunk topology and read-only presence spans, not mutation.
The former mutable layer `Values` property is replaced by `GetValues`,
`GetRowValues`, `CopyPadded`, and `CopyPaddedRow`. These preserve writable value
spans and caller-owned stencil scratch without exposing occupancy or raw storage
swapping. Mark bound chunks dirty after writing; use compiled `Swap` to exchange
values and dirty flags together. Retained spans cannot be revoked and must not
be used across storage swaps. None of these APIs is thread-safe.

Compiled mappings and seam links are indexed by chunk. Local edits inspect the
affected chunk's mappings instead of every compiled mapping. Dirty chunks are
tracked once per layer: clean commits do not scan source runs, partial commits
visit selected chunks, and fully dirty commits retain sequential source access.
Only occupied cells are committed; callback and mapping order are unspecified.
If a binding throws, that layer remains dirty for retry without rolling back
earlier writes. Dirty state travels with storage when layers are swapped.

These indexes retain extra flat integer arrays, not an object per chunk or
contact. Compilation cost and memory increase in exchange for faster local
operations. The uncompiled assembly's seam-pair scan is deliberately retained:
the measured replacement slowed common small edits. The plan records this
tradeoff rather than promising constant time for the entire mutation pipeline.

## Getting Started

The code in Kiyote.Geometry.Visualizer provides a starting point for utilizing each of the operations available.
