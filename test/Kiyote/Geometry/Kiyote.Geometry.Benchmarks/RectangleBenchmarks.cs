namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
[MarkdownExporterAttribute.GitHub]
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
		_p3 = new Point( 50, 50 );
	}

	[Benchmark, BenchmarkCategory( "Contains" )]
	public void Contains_Rect() {
		_ = _rect1.Contains( _rect3 );
	}

	[Benchmark, BenchmarkCategory("Contains")]
	public void Contains_IRect() {
		_ = _rect1.Contains( _rect2 );
	}

	[Benchmark, BenchmarkCategory( "Contains" )]
	public void Contains_Point() {
		_ = _rect1.Contains( _p1 );
	}

	[Benchmark, BenchmarkCategory( "Contains" )]
	public void Contains_IntInt() {
		_ = _rect1.Contains( 40, 40 );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public void IsEquivalentTo_Rect() {
		_rect1.IsEquivalentTo( _rect3 );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public void IsEquivalentTo_IRect() {
		_rect1.IsEquivalentTo( _rect2 );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public void IsEquivalentTo_PointPoint() {
		_rect1.IsEquivalentTo( _p2, _p3 );
	}

	[Benchmark, BenchmarkCategory( "IsEquivalentTo" )]
	public void IsEquivalentTo_IntIntIntInt() {
		_rect1.IsEquivalentTo( _p2, _p3 );
	}
}
