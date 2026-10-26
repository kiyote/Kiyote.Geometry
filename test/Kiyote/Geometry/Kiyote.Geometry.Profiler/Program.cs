using Kiyote.Geometry.Profiler.DelaunayVoronoi.D3;
using Kiyote.Geometry.Profiler;
using Kiyote.Geometry.Trees.Profiler;

var profiler = new D3DelaunayFactoryProfiler();
Console.WriteLine( "Press a key to execute..." );
Console.ReadKey();
profiler.Profile();
Console.WriteLine( "Press a key to exit..." );
Console.ReadKey();
