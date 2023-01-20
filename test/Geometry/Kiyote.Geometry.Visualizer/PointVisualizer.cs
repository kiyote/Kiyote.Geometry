using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Visualizer;

public sealed class PointVisualizer {

	private readonly Bounds _bounds;
	private readonly string _outputFolder;
	private readonly IRandom _random;

	public PointVisualizer(
		string outputFolder,
		Bounds bounds
	) {
		_outputFolder = outputFolder;
		_bounds = bounds;
		_random = new FastRandom();
	}

	public void Visualize() {
		VisualizeInside();
	}

	private void VisualizeInside() {
		Console.WriteLine( "Point.Inside" );
		using Image<Rgb24> image = new Image<Rgb24>( _bounds.Width, _bounds.Height );

		IReadOnlyList<IPoint> polygon = new List<IPoint>() {
			new Point( 200, 200 ),
			new Point( _bounds.Width - 200, 200 ),
			new Point( _bounds.Width - 200, _bounds.Height - 200 ),
			new Point( 200, _bounds.Height - 200 )
		};

		image.Mutate( ( context ) => {
			PointF[] lines = new PointF[5];
			lines[0].X = 200;
			lines[0].Y = 200;
			lines[1].X = _bounds.Width - 200;
			lines[1].Y = 200;
			lines[2].X = _bounds.Width - 200;
			lines[2].Y = _bounds.Height - 200;
			lines[3].X = 200;
			lines[3].Y = _bounds.Height - 200;
			lines[4].X = 200;
			lines[4].Y = 200;

			context.DrawLines( Color.Yellow, 1.0f, lines );
		} );

		for (int i = 0; i < 5000; i++) {
			int x = _random.NextInt( _bounds.Width );
			int y = _random.NextInt( _bounds.Height );
			IPoint p = new Point( x, y );

			image[x, y] = p.Inside( polygon ) ? Color.Green : Color.Red;
		}



		image.SaveAsPng( Path.Combine( _outputFolder, "PointInside.png" ) );
	}
}
