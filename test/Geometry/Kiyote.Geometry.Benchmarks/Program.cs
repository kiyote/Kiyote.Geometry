using BenchmarkDotNet.Running;
using Kiyote.Geometry;
using Kiyote.Geometry.Benchmarks.DelaunayVoronoi;
using Kiyote.Geometry.Benchmarks.Randomization;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;


//BenchmarkRunner.Run<DelaunatorFactoryBenchmarks>();

IDelaunatorFactory delaunatorFactory = new DelaunatorFactory();
IRandom random = new FastRandom();
IPointFactory pointFactory = new FastPoissonDiscPointFactory( random );
var points = pointFactory.Fill( new Bounds( 1000, 1000 ), 5 );
for (int i = 0; i < 100; i++) {
	delaunatorFactory.Create( points );
}


