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

Implementation is staged: topology validation, convex polygon
invariants, and factory/rasterizer boundary checks have separate remaining
milestones in [CORRECTNESS_PLAN.md](CORRECTNESS_PLAN.md).

## Getting Started

The code in Kiyote.Geometry.Visualizer provides a starting point for utilizing each of the operations available.
