using Kiyote.Geometry.DelaunayVoronoi.Profiler;
using Kiyote.Geometry.Profiler;
using Kiyote.Geometry.Randomization.Profiler;
using Kiyote.Geometry.Trees.Profiler;

var profiler = new RectProfiler();
Console.WriteLine( "Press a key to execute..." );
Console.ReadKey();
profiler.Profile();
Console.WriteLine( "Press a key to exit..." );
Console.ReadKey();
