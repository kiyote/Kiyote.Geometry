namespace Kiyote.Geometry.Noises;

public interface INoisyEdgeFactory {

	NoisyEdge Create(
		Edge toSplit,
		Edge control,
		float amplitude,
		int levels
	);

}
