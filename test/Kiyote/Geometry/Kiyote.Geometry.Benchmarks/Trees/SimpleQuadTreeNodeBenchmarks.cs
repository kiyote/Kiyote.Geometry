using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Trees.Benchmarks;

[MemoryDiagnoser( true )]
[MarkdownExporterAttribute.GitHub]
public class SimpleQuadTreeNodeBenchmarks {

	private readonly IRect _bounds;
	private readonly IRect _query;
	private readonly SimpleQuadTreeNode<Rect> _node;

	public SimpleQuadTreeNodeBenchmarks() {
		_bounds = new Rect( 0, 0, 1920, 1080 );
		_node = new SimpleQuadTreeNode<Rect>( _bounds );

		IRandom random = new FastRandom( 0xBADF00D );
		for (int i = 0; i < 10000; i++) {
			int x = random.NextInt( 0, 980 );
			int y = random.NextInt( 0, 980 );
			int w = random.NextInt( 5, 20 );
			int h = random.NextInt( 5, 20 );
			_node.Insert( new Rect( x, y, w, h ) );
		}

		_query = new Rect( 250, 250, 20, 20 );
	}

	[Benchmark]
	public void Query() {
		_ = _node.Query( _query );
	}
}
