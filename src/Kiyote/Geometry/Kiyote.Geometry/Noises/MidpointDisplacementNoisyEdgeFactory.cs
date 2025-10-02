using Kiyote.Geometry.Randomization;

namespace Kiyote.Geometry.Noises;

internal sealed class MidpointDisplacementNoisyEdgeFactory : INoisyEdgeFactory {

	private readonly IRandom _random;

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
		int segments = (int)Math.Pow( 2, levels );
		if( magnitude / segments < 2.0f ) {
			throw new ArgumentException( "Too many divisions." );
		}

		Edge[] toSplitQueue = new Edge[segments];
		Edge[] controlQueue = new Edge[segments];
		int queueEnd = 0;
		toSplitQueue[queueEnd] = toSplit;
		controlQueue[queueEnd] = control;

		Edge[] newToSplitQueue = new Edge[segments];
		Edge[] newControlQueue = new Edge[segments];

		for( int i = 0; i < levels; i++ ) {
			int newQueueEnd = -1;
			for( int j = 0; j <= queueEnd; j++ ) {
				Edge psplit = toSplitQueue[j];
				Edge pcontrol = controlQueue[j];
				Process( psplit, pcontrol, amplitude, out Edge e1, out Edge e2 );

				Edge saca = new Edge( psplit.A, pcontrol.A );
				Edge sacb = new Edge( psplit.A, pcontrol.B );
				Edge sbca = new Edge( psplit.B, pcontrol.A );
				Edge sbcb = new Edge( psplit.B, pcontrol.B );

				Edge e1Control = new Edge( saca.GetMidpoint( 0.5f ), sacb.GetMidpoint( 0.5f ) );
				Edge e2Control = new Edge( sbca.GetMidpoint( 0.5f ), sbcb.GetMidpoint( 0.5f ) );

				newQueueEnd++;
				newToSplitQueue[newQueueEnd] = e1;
				newControlQueue[newQueueEnd] = e1Control;
				newQueueEnd++;
				newToSplitQueue[newQueueEnd] = e2;
				newControlQueue[newQueueEnd] = e2Control;
			}

			(newToSplitQueue, toSplitQueue) = (toSplitQueue, newToSplitQueue);
			(newControlQueue, controlQueue) = (controlQueue, newControlQueue);

			queueEnd = newQueueEnd;
		}

		return new NoisyEdge(
			toSplit,
			toSplitQueue
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
		if( newMagnitude < 2.0f ) {
			throw new InvalidOperationException( "Displacement produced too-small split." );
		}

		return p;
	}
}
