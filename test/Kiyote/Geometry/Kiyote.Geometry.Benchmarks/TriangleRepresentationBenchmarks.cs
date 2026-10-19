namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class TriangleRepresentationBenchmarks {
	private ReferenceTriangle[] _references = null!;
	private Triangle[] _values = null!;
	private ReferenceTriangle[] _referenceCopy = null!;
	private Triangle[] _valueCopy = null!;

	[Params( 16, 2048, 4096, 16384 )]
	public int Count { get; set; }

	[GlobalSetup]
	public void Setup() {
		_references = ConstructReference();
		_values = ConstructValue();
		_referenceCopy = new ReferenceTriangle[Count];
		_valueCopy = new Triangle[Count];
		if( TraverseReference() != TraverseValue() || CopyReference() != CopyValue() ) {
			throw new InvalidOperationException( "Triangle representations disagree." );
		}
	}

	[Benchmark]
	public ReferenceTriangle[] ConstructReference() {
		ReferenceTriangle[] result = new ReferenceTriangle[Count];
		for( int i = 0; i < Count; i++ ) {
			result[i] = new ReferenceTriangle( new Point( i, 0 ), new Point( i + 1, 0 ), new Point( i, 1 ) );
		}
		return result;
	}

	[Benchmark]
	public Triangle[] ConstructValue() {
		Triangle[] result = new Triangle[Count];
		for( int i = 0; i < Count; i++ ) {
			result[i] = new Triangle( new Point( i, 0 ), new Point( i + 1, 0 ), new Point( i, 1 ) );
		}
		return result;
	}

	[Benchmark]
	public long TraverseReference() {
		long sum = 0;
		foreach( ReferenceTriangle triangle in _references ) {
			sum += triangle.P1.X + triangle.P2.X + triangle.P3.Y;
		}
		return sum;
	}

	[Benchmark]
	public long TraverseValue() {
		long sum = 0;
		foreach( Triangle triangle in _values ) {
			sum += triangle.P1.X + triangle.P2.X + triangle.P3.Y;
		}
		return sum;
	}

	[Benchmark]
	public int CopyReference() {
		_references.CopyTo( _referenceCopy, 0 );
		return _referenceCopy[^1].P2.X;
	}

	[Benchmark]
	public int CopyValue() {
		_values.CopyTo( _valueCopy, 0 );
		return _valueCopy[^1].P2.X;
	}

}
