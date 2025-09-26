using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Geometry.Noises;

public static class ExtensionMethods {

	public static IServiceCollection AddNoise(
		this IServiceCollection services
	) {
		services.TryAddSingleton<INoisyEdgeFactory, MidpointDisplacementNoisyEdgeFactory>();

		return services;
	}
}
