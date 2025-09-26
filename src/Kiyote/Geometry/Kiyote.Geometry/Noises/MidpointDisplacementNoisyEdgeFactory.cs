using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Noises;

internal sealed class MidpointDisplacementNoisyEdgeFactory : INoisyEdgeFactory {

	private readonly IRandom _random;

	private readonly struct ToProcess {

		public ToProcess(
			Edge toSplit,
			Edge control
		) {
			ToSplit = toSplit;
			Control = control;
		}

		public readonly Edge ToSplit;
		public readonly Edge Control;
	};

	public MidpointDisplacementNoisyEdgeFactory(
		IRandom random
	) {
		_random = random;
	}

	NoisyEdge INoisyEdgeFactory.Create(
		Edge toSplit,
		Edge control,
		float amplitude,
		int levels
	) {
		float magnitude = toSplit.Magnitude();
		if( magnitude / Math.Pow( 2, levels ) < 2.0f ) {
			throw new ArgumentException( "Too many divisions." );
		}

		Queue<ToProcess> queue = [];
		Queue<ToProcess> newQueue;

		queue.Enqueue( new ToProcess( toSplit, control ) );
		for( int i = 0; i < levels; i++ ) {
			newQueue = [];
			while( queue.Count != 0 ) {
				ToProcess tp = queue.Dequeue();
				Process( tp.ToSplit, tp.Control, amplitude, out Edge e1, out Edge e2 );

				Edge saca = new Edge( tp.ToSplit.A, tp.Control.A );
				Edge sacb = new Edge( tp.ToSplit.A, tp.Control.B );
				Edge sbca = new Edge( tp.ToSplit.B, tp.Control.A );
				Edge sbcb = new Edge( tp.ToSplit.B, tp.Control.B );

				Edge e1Control = new Edge( saca.GetMidpoint( 0.5f ), sacb.GetMidpoint( 0.5f ) );
				Edge e2Control = new Edge( sbca.GetMidpoint( 0.5f ), sbcb.GetMidpoint( 0.5f ) );

				newQueue.Enqueue( new ToProcess( e1, e1Control ) );
				newQueue.Enqueue( new ToProcess( e2, e2Control ) );
			}
			queue = newQueue;
		}

		return new NoisyEdge(
			toSplit,
			[.. queue.Select( q => q.ToSplit )]
		);
	}

	private void Process(
		Edge toSplit,
		Edge control,
		float amplitude,
		out Edge e1,
		out Edge e2
	) {
		Point split = Split( control.A, control.B, amplitude );
		e1 = new Edge( toSplit.A, split );
		e2 = new Edge( split, toSplit.B );
	}

	private Point Split(
		Point p1,
		Point p2,
		float amplitude
	) {
		float distance = _random.NextFloat( amplitude / 2.0f, 1.0f - ( amplitude / 2.0f ) );
		Point p = Edge.GetMidpoint( p1, p2, distance );
		float newMagnitude = Math.Min( Edge.Magnitude( p1, p ), Edge.Magnitude( p2, p ) );
		if (newMagnitude < 2.0f) {
			throw new InvalidOperationException( "Displacement produced too-small split." );
		}

		return p;
	}
}
