using Kiyote.Geometry.Topology;

namespace Kiyote.Geometry.Benchmarks.Topology;

[MemoryDiagnoser( false )]
public class TopologyLocalityBenchmarks {
	private GridAssembly<BenchCell> _assembly = null!;
	private GridAssembly<BenchCell> _unobserved = null!;
	private ICompiledGridAssembly<BenchCell> _compiled = null!;
	private IGridLayer<float> _layer = null!;
	private PlacementId _placement;
	private PlacementId _unobservedPlacement;
	private int _chunk;
	private int _expectedCells;
	private readonly BenchCell _cell = new BenchCell { Pressure = 1 };

	[Params( 16, 256 )]
	public int Scale { get; set; }

	[Params( "Chunks", "Placements", "Seams", "Fragmentation" )]
	public string Workload { get; set; } = "Chunks";

	[GlobalSetup]
	public void Setup() {
		_assembly = CreateAssembly( out _placement );
		_unobserved = CreateAssembly( out _unobservedPlacement );
		_compiled = new GridCompiler().Compile( _assembly, 32 );
		_layer = _compiled.Bind<float, PressureBinding>( default );
		_ = _compiled.ChunkLayout.TryLocate( 31, 16, out _chunk, out _ );
		_expectedCells = 2048 + ( Workload switch {
			"Chunks" => Scale * 1024,
			"Placements" => 256,
			"Seams" => 256 * 64,
			"Fragmentation" => 4096,
			_ => throw new InvalidOperationException( "Unknown workload." )
		} );
		if( !EditUnobserved() || !EditCompiled() || CountCells( _compiled ) != _expectedCells ) {
			throw new InvalidOperationException( "Locality fixture differs from its occupancy reference." );
		}
		_layer[31, 16] = 3;
		CommitOneDirty();
		if( _assembly.Placements[0].Source.GetCell( 31, 16 ).Pressure != 3 || _layer.IsDirty( _chunk ) ) {
			throw new InvalidOperationException( "Commit failed to store and clear the selected chunk." );
		}
		CommitAllDirty();
		Console.WriteLine( $"Locality metadata: {Workload}/{Scale}, chunks={_compiled.ChunkLayout.ChunkCount}, capacity={_compiled.ChunkLayout.ChunkCapacity}, runs={_compiled.SourceRuns.Length}, links={_compiled.SeamLinks.Length}" );
	}

	[GlobalCleanup]
	public void Cleanup() => _compiled.Dispose();

	[Benchmark]
	public bool EditUnobserved() =>
		_unobserved.TryRemoveCell( _unobservedPlacement, 31, 16 )
		&& _unobserved.TryAddCell( _unobservedPlacement, 31, 16, _cell );

	[Benchmark]
	public bool EditCompiled() =>
		_assembly.TryRemoveCell( _placement, 31, 16 )
		&& _assembly.TryAddCell( _placement, 31, 16, _cell );

	[Benchmark]
	public void CommitClean() => _compiled.Commit();

	[Benchmark]
	public void CommitOneDirty() {
		_layer.MarkDirty( _chunk );
		_compiled.Commit();
	}

	[Benchmark]
	public void CommitAllDirty() {
		foreach( int chunk in _compiled.ChunkLayout.EnumerateChunks() ) {
			_layer.MarkDirty( chunk );
		}
		_compiled.Commit();
	}

	[Benchmark]
	public int CompileAndBind() {
		using ICompiledGridAssembly<BenchCell> compiled = new GridCompiler().Compile( _unobserved, 32 );
		_ = compiled.Bind<float, PressureBinding>( default );
		return compiled.SourceRuns.Length;
	}

	private GridAssembly<BenchCell> CreateAssembly( out PlacementId placement ) {
		GridAssembly<BenchCell> assembly = new GridAssembly<BenchCell>();
		placement = Attach( assembly, CreateSource( 32, 32 ), 0, 0 );
		_ = Attach( assembly, CreateSource( 32, 32 ), 32, 0 );
		switch( Workload ) {
			case "Chunks":
				_ = Attach( assembly, CreateSource( Scale * 32, 32 ), 1024, 1024 );
				break;
			case "Placements":
				int width = 8192 / Scale;
				for( int i = 0; i < Scale; i++ ) {
					DenseGridSource<BenchCell> source = new DenseGridSource<BenchCell>( width, 1 );
					for( int x = 0; x < width; x += 32 ) {
						_ = source.TrySetCell( x, 0, _cell );
					}
					_ = Attach( assembly, source, 1024 + (i * width), 1024 );
				}
				break;
			case "Seams":
				for( int i = 0; i < 256; i++ ) {
					_ = Attach( assembly, CreateSource( 2, 16 ), 1024 + (i * 32), 1024 );
					_ = Attach( assembly, CreateSource( 2, 16 ), 1024 + (i * 32) + ( i < Scale ? 2 : 4 ), 1024 );
				}
				break;
			case "Fragmentation":
				DenseGridSource<BenchCell> fragmented = new DenseGridSource<BenchCell>( 512, 16 );
				int stride = 512 / Scale;
				for( int y = 0; y < 16; y++ ) {
					for( int x = 0; x < 512; x++ ) {
						if( x % stride < stride / 2 ) {
							_ = fragmented.TrySetCell( x, y, _cell );
						}
					}
				}
				_ = Attach( assembly, fragmented, 1024, 1024 );
				break;
			default:
				throw new InvalidOperationException( "Unknown workload." );
		}
		return assembly;
	}

	private DenseGridSource<BenchCell> CreateSource( int width, int height ) {
		DenseGridSource<BenchCell> source = new DenseGridSource<BenchCell>( width, height );
		for( int y = 0; y < height; y++ ) {
			for( int x = 0; x < width; x++ ) {
				_ = source.TrySetCell( x, y, _cell );
			}
		}
		return source;
	}

	private static PlacementId Attach( GridAssembly<BenchCell> assembly, IGridSource<BenchCell> source, int x, int y ) {
		AttachResult result = assembly.TryAttach( source, x, y );
		if( result.Placement.IsNone ) {
			throw new InvalidOperationException( "Unexpected fixture overlap." );
		}
		return result.Placement;
	}

	private static int CountCells( ICompiledGridAssembly<BenchCell> compiled ) {
		int count = 0;
		foreach( SourceRun run in compiled.SourceRuns ) {
			count += run.Length;
		}
		return count;
	}
}
