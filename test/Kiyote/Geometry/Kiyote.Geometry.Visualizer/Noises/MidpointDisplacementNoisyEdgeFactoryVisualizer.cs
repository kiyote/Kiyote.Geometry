using Kiyote.Geometry.Randomization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kiyote.Geometry.Noises.Visualizer;

public sealed class MidpointDisplacementNoisyEdgeFactoryVisualizer {

	private readonly string _outputFolder;
	private readonly ISize _bounds;
	private readonly INoisyEdgeFactory _edgeFactory;

	public MidpointDisplacementNoisyEdgeFactoryVisualizer(
		string outputFolder,
		ISize size
	) {
		_outputFolder = outputFolder;
		_bounds = size;

		IRandom random = new FastRandom();
		_edgeFactory = new MidpointDisplacementNoisyEdgeFactory( random );
	}

	public void Visualize() {
		VisualizeCreate();
	}

	private void VisualizeCreate() {
		Console.WriteLine( "MidpointDisplacementNoisyEdgeFactoryVisualizer.Create" );
		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		int midX = (int)( _bounds.Width * 0.5f );
		int xOffset = (int)( _bounds.Width * 0.1f );
		int midY = (int)( _bounds.Height * 0.5f );
		int yOffset = (int)( _bounds.Height * 0.25f );
		Edge toSplit = new Edge( xOffset, midY, _bounds.Width - xOffset, midY );
		Edge control = new Edge( midX, yOffset, midX, _bounds.Height - yOffset );

		NoisyEdge noisyEdge = _edgeFactory.Create( toSplit, control, 0.5f, 6 );

		image.Mutate( ( context ) => {
			PointF[] edge = new PointF[2];
			edge[0].X = noisyEdge.Source.A.X;
			edge[0].Y = noisyEdge.Source.A.Y;
			edge[1].X = noisyEdge.Source.B.X;
			edge[1].Y = noisyEdge.Source.B.Y;
			context.DrawLine( Color.LightGray, 1.0f, edge );

			edge[0].X = control.A.X;
			edge[0].Y = control.A.Y;
			edge[1].X = control.B.X;
			edge[1].Y = control.B.Y;
			context.DrawLine( Color.DarkGray, 1.0f, edge );

			PointF[] lines = new PointF[noisyEdge.Noise.Count + 1];
			lines[0].X = noisyEdge.Noise[0].A.X;
			lines[0].Y = noisyEdge.Noise[0].A.Y;
			for( int i = 0; i < noisyEdge.Noise.Count; i++ ) {
				lines[i + 1].X = noisyEdge.Noise[i].B.X;
				lines[i + 1].Y = noisyEdge.Noise[i].B.Y;
			}

			context.DrawLine( Color.Yellow, 1.0f, lines );

			foreach( Edge e in noisyEdge.Noise ) {
				image[e.A.X, e.A.Y] = Color.Magenta;
				image[e.B.X, e.B.Y] = Color.Magenta;
			}

			image[control.A.X, control.A.Y] = Color.Red;
			image[control.B.X, control.B.Y] = Color.Red;
			image[toSplit.A.X, toSplit.A.Y] = Color.Blue;
			image[toSplit.B.X, toSplit.B.Y] = Color.Blue;
		} );

		image.SaveAsPng( System.IO.Path.Combine( _outputFolder, "MidpointDisplacementNoisyEdgeFactoryVisualizerCreate.png" ) );
	}
}
