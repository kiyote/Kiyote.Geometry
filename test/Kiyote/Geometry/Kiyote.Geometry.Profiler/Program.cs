using Kiyote.Geometry.Profiler.DelaunayVoronoi.D3;
using Kiyote.Geometry.Profiler;
using Kiyote.Geometry.Trees.Profiler;

if( args.Length != 0 ) {
	if( args.Length != 3 || args[0] != "--d3" || !int.TryParse( args[2], out int iterations ) || iterations <= 0 ) {
		Console.Error.WriteLine( "Usage: --d3 <Irregular|Thin|MostlyHull|Sanitized> <positive iteration count>" );
		return 1;
	}
	D3WorkloadProfiler.Profile( args[1], iterations );
	return 0;
}

var profiler = new D3DelaunayFactoryProfiler();
Console.WriteLine( "Press a key to execute..." );
Console.ReadKey();
profiler.Profile();
Console.WriteLine( "Press a key to exit..." );
Console.ReadKey();
return 0;
