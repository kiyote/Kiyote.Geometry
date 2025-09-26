namespace Kiyote.Geometry.Noises;

public sealed class NoisyEdge {

	public NoisyEdge(
		Edge source,
		IReadOnlyList<Edge> noise
	) {
		Source = source;
		Noise = noise;
	}

	public Edge Source { get; }

	public IReadOnlyList<Edge> Noise { get; }
}
