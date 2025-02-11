Size: 1920x1080, Count: 10000, Cells: 5->20, Query: 20

```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5247/22H2/2022Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.102
  [Host] : .NET 8.0.12 (8.0.1224.60305), X64 RyuJIT AVX2
```

| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Query  | 6.551 us | 0.2946 us | 0.2756 us | 3.8834 | 0.0076 |  23.84 KB |
