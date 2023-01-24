﻿namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class EdgeBenchmarks {

	private readonly Edge _edge1;
	private readonly Edge _edge2;

	public EdgeBenchmarks() {
		Bounds bounds = new Bounds( 1000, 1000 );
		_edge1 = new Edge( new Point( 0, 0 ), new Point( bounds.Width, bounds.Height ) );
		_edge2 = new Edge( new Point( 0, bounds.Height ), new Point( bounds.Width, 0 ) );
	}

	[Benchmark]
	public void TryFindIntersection() {
		_ = _edge1.TryFindIntersection( _edge2, out _ );
	}
}

