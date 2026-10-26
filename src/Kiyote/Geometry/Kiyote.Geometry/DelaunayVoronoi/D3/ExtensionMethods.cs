using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Geometry.DelaunayVoronoi.D3;

public static class ExtensionMethods {

	public static IServiceCollection AddD3Voronoi(
		this IServiceCollection services
	) {
		services.AddSingleton<IVoronoiFactory, D3VoronoiFactory>();
		services.AddSingleton<IDelaunayFactory, D3DelaunayFactory>();
		return services;
	}
}
