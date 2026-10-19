namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class RectangleBenchmarks {

	private readonly Rect _rect1;
	private readonly IRect _rect2;
	private readonly Rect _rect3;
	private readonly Point _p1;
	private readonly Point _p2;
	private readonly Point _p3;

	public RectangleBenchmarks() {
		_rect1 = new Rect( 0, 0, 50, 50 );
		_rect2 = new Rect( 0, 0, 50, 50 );
		_rect3 = new Rect( 0, 0, 50, 50 );

		_p1 = new Point( 30, 30 );
		_p2 = new Point( 0, 0 );
		_p3 = new Point( 49, 49 );
	}

	[Benchmark, BenchmarkCategory( "Contains" )]
	public bool Contains_Rect() {
		return _rect1.Contains( _rect3 );
	}

	[Benchmark, BenchmarkCategory("Contains")]
	public bool Contains_IRect() {
		return _rect1.Contains( _rect2 );
	}

	[Benchmark, BenchmarkCategory( "Contains" )]
	public bool Contains_Point() {
		return _rect1.Contains( _p1 );
	}

	[Benchmark, BenchmarkCategory( "Contains" )]
	public bool Contains_IntInt() {
		return _rect1.Contains( _p1.X, _p1.Y );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public bool IsEquivalentTo_Rect() {
		return _rect1.IsEquivalentTo( _rect3 );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public bool IsEquivalentTo_IRect() {
		return _rect1.IsEquivalentTo( _rect2 );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public bool IsEquivalentTo_PointPoint() {
		return _rect1.IsEquivalentTo( _p2, _p3 );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public bool IsEquivalentTo_IntIntIntInt() {
		return _rect1.IsEquivalentTo( _p2.X, _p2.Y, _p3.X, _p3.Y );
	}

	[GlobalSetup]
	public void Validate() {
		if( !Contains_Rect()
			|| !Contains_IRect()
			|| !Contains_Point()
			|| !Contains_IntInt()
			|| !IsEquivalentTo_Rect()
			|| !IsEquivalentTo_IRect()
			|| !IsEquivalentTo_PointPoint()
			|| !IsEquivalentTo_IntIntIntInt()
		) {
			throw new InvalidOperationException( "Rectangle benchmark fixture produced an unexpected result." );
		}
	}
}
