using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi.D3;

namespace Kiyote.Geometry.DelaunayVoronoi.UnitTests;

[TestFixture]
[ExcludeFromCodeCoverage]
public sealed class VoronoiStorageTests {
	[Test]
	public void Create_GridWithWarmScratch_StaysWithinFullDiagramAllocationBudget() {
		Point[] sites = Grid();
		Rect bounds = new Rect( 0, 0, 100, 100 );
		IVoronoiFactory factory = new D3VoronoiFactory();
		for( int i = 0; i < 10; i++ ) {
			GC.KeepAlive( factory.Create( bounds, sites ) );
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		IVoronoi result = factory.Create( bounds, sites );
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		GC.KeepAlive( result );
		Assert.That( result.Cells.Length, Is.EqualTo( sites.Length ) );
		Assert.That( allocated, Is.LessThanOrEqualTo( 180 * 1024 ) );
	}

	[Test]
	public void Create_ConcurrentReuseAndFailures_PreserveOriginalDiagramStorage() {
		Point[] sites = Grid();
		Rect bounds = new Rect( 0, 0, 100, 100 );
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi original = factory.Create( bounds, sites );
		Cell[] metadata = original.Cells.ToArray();
		ReadOnlySpan<Cell> retainedMetadata = original.Cells;
		ReadOnlySpan<Point> retainedVertices = original.GetCellVertices( 0 );
		Point[][] polygons = Enumerable.Range( 0, sites.Length )
			.Select( i => original.GetCellVertices( i ).ToArray() ).ToArray();
		int[][] neighbours = Enumerable.Range( 0, sites.Length )
			.Select( i => original.GetNeighbourIndices( i ).ToArray() ).ToArray();
		Edge[] edges = original.Edges.ToArray();
		Point[] duplicates = [.. sites, sites[0]];
		Parallel.For( 0, 16, _ => {
			Assert.That( () => factory.Create( bounds, duplicates ), Throws.ArgumentException );
			IVoronoi next = factory.Create( bounds, sites );
			Assert.That( next.Cells.Length, Is.EqualTo( sites.Length ) );
		} );
		Assert.That( original.Points, Is.EqualTo( sites ) );
		Assert.That( original.Edges, Is.EqualTo( edges ) );
		Assert.That( retainedMetadata.SequenceEqual( metadata ), Is.True );
		Assert.That( retainedVertices.SequenceEqual( polygons[0] ), Is.True );
		for( int i = 0; i < sites.Length; i++ ) {
			Assert.That( original.GetCellVertices( i ).ToArray(), Is.EqualTo( polygons[i] ) );
			Assert.That( original.GetNeighbourIndices( i ).ToArray(), Is.EqualTo( neighbours[i] ) );
		}
	}

	[TestCase( -1 )]
	[TestCase( 400 )]
	[TestCase( int.MaxValue )]
	public void ReadCell_InvalidId_ThrowsArgumentOutOfRangeException( int index ) {
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 100, 100 ), Grid() );
		Assert.That( () => { _ = diagram.GetCellVertices( index ); }, Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( () => diagram.Contains( index, default ), Throws.TypeOf<ArgumentOutOfRangeException>() );
		Assert.That( () => diagram.ExportPolygon( index ), Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[TestCase( -32769, 0 )]
	[TestCase( 0, 32768 )]
	public void Contains_OutOfDomainPosition_ThrowsArgumentOutOfRangeException( int x, int y ) {
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 100, 100 ), Grid() );
		Assert.That( () => diagram.Contains( 0, new Point( x, y ) ), Throws.TypeOf<ArgumentOutOfRangeException>() );
	}

	[Test]
	public void ReadCell_RepeatedMetadataVerticesAndContainment_DoesNotAllocate() {
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 100, 100 ), Grid() );
		long expected = Read();
		for( int i = 0; i < 100; i++ ) {
			_ = Read();
		}
		long before = GC.GetAllocatedBytesForCurrentThread();
		long checksum = 0;
		for( int i = 0; i < 100; i++ ) {
			checksum += Read();
		}
		long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.That( checksum, Is.EqualTo( 100 * expected ) );
		Assert.That( allocated, Is.Zero );

		long Read() {
			long sum = 0;
			ReadOnlySpan<Cell> cells = diagram.Cells;
			for( int i = 0; i < cells.Length; i++ ) {
				ref readonly Cell cell = ref cells[i];
				sum += cell.VertexCount + cell.BoundingBox.Width + cell.BoundingBox.Height;
				foreach( Point point in diagram.GetCellVertices( i ) ) {
					sum += point.X + point.Y;
				}
				if( diagram.Contains( i, diagram.Points[i] ) ) {
					sum++;
				}
			}
			return sum;
		}
	}

	[Test]
	public void ExportPolygon_AllCells_PreservesOrderedGeometryAndInclusiveContainment() {
		Point[] sites = Grid();
		IVoronoiFactory factory = new D3VoronoiFactory();
		IVoronoi diagram = factory.Create( new Rect( 0, 0, 100, 100 ), sites );
		sites[0] = new Point( 99, 99 );
		int offset = 0;
		for( int i = 0; i < diagram.Cells.Length; i++ ) {
			ReadOnlySpan<Point> vertices = diagram.GetCellVertices( i );
			Polygon exported = diagram.ExportPolygon( i );
			Assert.That( exported.Points, Is.EqualTo( vertices.ToArray() ) );
			Assert.That( diagram.Cells[i].VertexCount, Is.EqualTo( vertices.Length ) );
			Assert.That( diagram.Cells[i].VertexOffset, Is.EqualTo( offset ) );
			offset += vertices.Length;
			Assert.That( diagram.Contains( i, diagram.Points[i] ), Is.True );
			for( int y = -1; y <= 100; y++ ) {
				Point point = new Point( i % 101, y );
				Assert.That( diagram.Contains( i, point ), Is.EqualTo( exported.Contains( point ) ) );
			}
			for( int j = 0; j < vertices.Length; j++ ) {
				Assert.That( diagram.Contains( i, vertices[j] ), Is.True );
				Assert.That( exported.Edges[j], Is.EqualTo( new Edge( vertices[j], vertices[(j + 1) % vertices.Length] ) ) );
			}
			Assert.That( diagram.ExportPolygon( i ), Is.Not.SameAs( exported ) );
		}
		Assert.That( diagram.Points[0], Is.EqualTo( new Point( 2, 2 ) ) );
	}

	private static Point[] Grid() {
		Point[] sites = new Point[400];
		for( int i = 0; i < sites.Length; i++ ) {
			sites[i] = new Point( 2 + (i % 20 * 5), 2 + (i / 20 * 5) );
		}
		return sites;
	}
}
