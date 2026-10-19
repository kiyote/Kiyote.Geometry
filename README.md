![CI](https://github.com/kiyote/Kiyote.Geometry/actions/workflows/ci.yml/badge.svg?branch=main)
![coverage](https://raw.githubusercontent.com/kiyote/Kiyote.Geometry/badges/.badges/main/coverage.svg)

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
collections; reusing or mutating an input array/list cannot change an existing
diagram. Quantized Voronoi polygons approximate the continuous cells, not an
exact partition of continuous space.

Implementation is staged: topology validation and rasterizer boundary
checks have separate remaining
milestones in [CORRECTNESS_PLAN.md](CORRECTNESS_PLAN.md).

## Getting Started

The code in Kiyote.Geometry.Visualizer provides a starting point for utilizing each of the operations available.
