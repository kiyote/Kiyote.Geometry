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

See [BENCHMARK.md](BENCHMARK.md) for measured results and known blocked workloads,
and [CORRECTNESS_PLAN.md](CORRECTNESS_PLAN.md) for the red-green implementation order.
