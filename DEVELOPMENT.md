# Notes

## Clean up filemodes
`find . -type f -not -path '*/.git' -exec chmod 644 {} +`

`find . -type d -not -path '*/.git*' -exec chmod 755 {} +`

## Github action failing with permission denied?
```
git update-index --add --chmod=+x ./create-orphan-branch.sh
```

Still failing?  Try running the `./create-orphan-branch.sh` locally once.


## Coverage badge failing to be pushed?

Give the workflow the `contents: write` permission.


## Coverage showing as 0%

Add the `coverlet.collector` package to your test project and run tests with `dotnet test --collect:"XPlat Code Coverage"`.

## Performance baselines

Build benchmarks in Release, then run a focused selection:

```powershell
dotnet build test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj -c Release --no-restore
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj -c Release --no-build --no-restore -- --filter '*GeometryWorkloadBenchmarks*' --job short --warmupCount 3 --iterationCount 5 --iterationTime 100 --launchCount 1
```

The runner uses BenchmarkDotNet's isolated-process toolchain. Use the same job
arguments, SDK/runtime, machine, and fixtures on both sides of a comparison.
Short runs establish an exploratory baseline; use longer runs when uncertainty
is too large to support a performance decision. Results and generated code are
written under the ignored `BenchmarkDotNet.Artifacts` directory.

Return benchmark results so the harness consumes them. For `Try...` methods,
consume both the success flag and the output. Do not benchmark constant-only
expressions or let mutations accumulate across invocations. Batched primitive
workloads report per-operation time via `OperationsPerInvoke`; their results
include loop and input-access overhead.

New workload fixtures validate expected results during setup, outside measured
iterations. A failed setup is not a performance result. The runner exits nonzero
if validation or execution fails; fix the relevant correctness defect before
publishing timings for that case. `--job dry` can check fixtures and runner wiring,
but its timings are not a baseline. Use `--disasm` on a narrow selection when a
near-zero result needs generated-code inspection.

See [BENCHMARK.md](BENCHMARK.md) for current results, grouped by benchmark class.
The report contains one fresh full-suite measurement, not mixed historical runs.

### Published measurement environment

The 2026-10-20 refresh uses production at `e761581`, Windows 11
10.0.26100.9448, an Intel Core i7-9700K (8 physical/logical cores),
.NET SDK 11.0.100-rc.2.26504.105, .NET 10.0.13 X64 RyuJIT, and
BenchmarkDotNet 0.15.8. Reproduce the full run with:

```powershell
dotnet run --project test\Kiyote\Geometry\Kiyote.Geometry.Benchmarks\Kiyote.Geometry.Benchmarks.csproj -c Release --no-build --no-restore -- --filter '*' --job short --iterationCount 10 --warmupCount 5 --iterationTime 250 --launchCount 1 --artifacts .\BenchmarkDotNet.Artifacts\final-current
```

Benchmark methods with iteration setup or explicit invocation counts retain their
fixture-specific execution settings. Error is half the 99.9% confidence interval;
allocated bytes describe the measured operation, not retained process memory.
An allocation ratio of `NA` means the ratio to a zero-allocation baseline is
undefined, not that the measurement failed.
Very small operations can fall below reliable timer/overhead resolution. Do not
interpret zero or highly uncertain timings as exact costs or universal speedups.

## Correctness and validation

The [README](README.md) and public XML documentation define supported domains,
shape rules, ownership, and extension contracts. Keep rejection behavior and
consumer overloads consistent with those contracts.

For behavior fixes, add a permanent regression and observe its intended failure
before changing production code. Compilation failure is not a behavioral
reproduction. Preserve the assertion after the fix and add adjacent boundary
and seeded reference cases. For performance-only changes, preserve behavior with
tests and use matched benchmarks; do not introduce wall-clock unit-test limits.
Allocation budgets must specify a reproducible workload and warmup.

Before completing cross-library changes, build the full solution and run both
test projects in Release and Debug:

```powershell
dotnet build Kiyote.Geometry.slnx -c Release --no-restore
dotnet test Kiyote.Geometry.slnx -c Release --no-build --no-restore
dotnet test Kiyote.Geometry.slnx -c Debug --no-restore
```

### Arithmetic and scratch bounds

These derivations justify the current bounded kernels. Revisit them when
changing coordinate or vertex limits; passing a few larger examples is not
proof that a wider domain is safe.

- Geometry coordinates are in `[-32768, 32767]`. Differences have magnitude at
  most 65,535. Squared distance and conservative cross-product magnitude are
  bounded by `2 * 65535^2 = 8,589,672,450 < 2^33`; promote before multiplying.
  Manhattan distance is at most 131,070.
- Segment parameter numerators and denominators fit `long`. For an accepted
  intersection, each coordinate numerator
  `origin * denominator + parameterNumerator * displacement` has magnitude
  below `(32768 + 65535) * 8,589,672,450 < 2^50`. Both numerator and denominator
  convert exactly to `double`.
- A non-half rational coordinate is more than `2^-34` from a half-integer,
  because its positive denominator is less than `2^33`. Double division in
  the supported coordinate interval has absolute error at most `2^-38`.
  It therefore cannot turn a non-tie into a tie or cross the tie; exact halves
  are representable. This permits nearest-even rounding without arbitrary
  precision or integer quotient/remainder division.
- A convex intersection has at most the sum of its input vertex counts.
  With at most 128 normalized vertices per polygon, intersection candidates
  fit 256 points; final polygons still obey the independent 128-corner limit.
- Voronoi traversal is limited to 64 vertices. Open projection adds at most
  two vertices. Each projected edge contributes at most two clipped endpoints,
  with up to eight corner insertions across clipping passes: scratch holds
  `2 * (64 + 2) + 8 = 140` points. Final convex-hull construction retains the
  polygon limit rather than treating scratch capacity as a public limit.
- For at least three sites, triangulation capacity is `6*n - 15`, computed in
  `long` and checked against `Array.MaxLength` before allocation. For `n >= 8`
  it also bounds the `4*n` vectors and `2*n` coordinates; smaller valid counts
  are trivially representable. Pool slices use logical lengths, not capacity.
- Raster deltas are at most 65,535 and doubled scanline indices at most
  131,071; Bresenham error arithmetic fits `int`. Validation occurs before
  callbacks, outside the pixel kernel.
- Shared rectangles and topology retain signed-32-bit coordinates. Dimension
  products, translated endpoints, neighbour coordinates, and padded lengths
  are widened before checks. Positive source-dimension products are below
  `2^62`; chunk sizes are powers of two from 32 through 4096, with at most
  `2^24` cells per chunk. Allocation lengths are checked before narrowing.

The completed implementation plan and its red-green/before-after evidence remain
available in Git history through `e761581`; the live documentation records the
supported contracts rather than retaining a completed task checklist.
