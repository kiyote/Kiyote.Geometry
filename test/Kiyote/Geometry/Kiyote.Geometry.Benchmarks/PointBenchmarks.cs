namespace Kiyote.Geometry.Benchmarks;

[MemoryDiagnoser( false )]
public class PointBenchmarks {

	private const int LookupCount = 1024;

	private readonly Point _p1;
	private readonly Point _p2;
	private readonly object _boxed;
	private readonly Point[] _points;
	private readonly HashSet<Point> _set;
	private readonly Dictionary<Point, int> _dictionary;

	public PointBenchmarks() {
		_p1 = new Point( 200, 300 );
		_p2 = new Point( 201, 299 );
		_boxed = _p2;

		_points = new Point[LookupCount];
		for( int i = 0; i < LookupCount; i++ ) {
			_points[i] = new Point( i % 32, i / 32 );
		}
		_set = [.. _points];
		_dictionary = [];
		for( int i = 0; i < LookupCount; i++ ) {
			_dictionary[_points[i]] = i;
		}
	}

	[Benchmark, BenchmarkCategory( "Point_Ctor" )]
	public Point Ctor() {
		return new Point( 200, 300 );
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Add_Point() {
		return _p1.Add( _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Add_IntInt() {
		return _p1.Add( 1, 1 );
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Subtract_Point() {
		return _p1.Subtract( _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Subtract_IntInt() {
		return _p1.Subtract( 1, 1 );
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Negate() {
		return _p1.Negate();
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Operator_Add() {
		return _p1 + _p2;
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Operator_Subtract() {
		return _p1 - _p2;
	}

	[Benchmark, BenchmarkCategory( "Point_Arithmetic" )]
	public Point Operator_Negate() {
		return -_p1;
	}

	[Benchmark]
	public int Deconstruct() {
		(int x, int y) = _p1;
		return x + y;
	}

	[Benchmark, BenchmarkCategory( "Point_MinMax" )]
	public Point Min() {
		return Point.Min( _p1, _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_MinMax" )]
	public Point Max() {
		return Point.Max( _p1, _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_Distance" )]
	public int ManhattanDistance() {
		return _p1.ManhattanDistance( _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_Distance" )]
	public int ChebyshevDistance() {
		return _p1.ChebyshevDistance( _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_Distance" )]
	public long DistanceSquared() {
		return _p1.DistanceSquared( _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_Adjacency" )]
	public bool IsAdjacentTo_Orthogonal() {
		return _p1.IsAdjacentTo( _p2, false );
	}

	[Benchmark, BenchmarkCategory( "Point_Adjacency" )]
	public bool IsAdjacentTo_Diagonal() {
		return _p1.IsAdjacentTo( _p2, true );
	}

	[Benchmark, BenchmarkCategory( "Point_Chunk" )]
	public Point ShiftRight() {
		return _p1.ShiftRight( 5 );
	}

	[Benchmark, BenchmarkCategory( "Point_Chunk" )]
	public Point And() {
		return _p1.And( 31 );
	}

	[Benchmark, BenchmarkCategory( "Point_Equality" )]
	public bool Operator_Equals() {
		return _p1 == _p2;
	}

	[Benchmark, BenchmarkCategory( "Point_Equality" )]
	public bool Operator_NotEquals() {
		return _p1 != _p2;
	}

	[Benchmark, BenchmarkCategory( "Point_Equality" )]
	public bool Equals_Object() {
		return _p1.Equals( _boxed );
	}

	[Benchmark, BenchmarkCategory( "Point_Equality" )]
	public bool Equals_IEquatable() {
		return ( (IEquatable<Point>)_p1 ).Equals( _p2 );
	}

	[Benchmark, BenchmarkCategory( "Point_Hash" )]
	public int GetHashCode_Single() {
		return _p1.GetHashCode();
	}

	[Benchmark( OperationsPerInvoke = LookupCount ), BenchmarkCategory( "Point_Hash" )]
	public int HashSet_Contains() {
		int found = 0;
		Point[] points = _points;
		for( int i = 0; i < points.Length; i++ ) {
			if( _set.Contains( points[i] ) ) {
				found++;
			}
		}
		return found;
	}

	[Benchmark( OperationsPerInvoke = LookupCount ), BenchmarkCategory( "Point_Hash" )]
	public int Dictionary_TryGetValue() {
		int sum = 0;
		Point[] points = _points;
		for( int i = 0; i < points.Length; i++ ) {
			if( _dictionary.TryGetValue( points[i], out int value ) ) {
				sum += value;
			}
		}
		return sum;
	}

	[Benchmark( OperationsPerInvoke = LookupCount ), BenchmarkCategory( "Point_Hash" )]
	public int HashSet_Build() {
		HashSet<Point> set = new HashSet<Point>( LookupCount );
		for( int i = 0; i < LookupCount; i++ ) {
			set.Add( new Point( i % 32, i / 32 ) );
		}
		return set.Count;
	}

	[Benchmark]
	public string ToStringBenchmark() {
		return _p1.ToString();
	}
}
