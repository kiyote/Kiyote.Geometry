using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Trees;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Geometry;

public static class ExtensionMethods {

	public static IServiceCollection AddRasterizer(
		this IServiceCollection services
	) {
		_ = services.AddScoped<IRasterizer, Rasterizer>();

		return services;
	}

	public static IServiceCollection AddRandomization(
		this IServiceCollection services
	) {
		_ = services.AddScoped<IPointFactory, FastPoissonDiscPointFactory>();
		_ = services.AddScoped<IRandom, FastRandom>();

		return services;
	}

	public static IServiceCollection AddDelaunayVoronoi(
		this IServiceCollection services
	) {
		_ = services.AddSingleton<IVoronoiFactory, D3VoronoiFactory>();
		_ = services.AddSingleton<IDelaunayFactory, D3DelaunayFactory>();
		services.TryAddSingleton<IQuadTreeFactory, SimpleQuadTreeFactory>();
		_ = services.AddSingleton<ISearchableVoronoiFactory, QuadTreeSearchableVoronoiFactory>();

		return services;
	}

	public static IServiceCollection AddTrees(
		this IServiceCollection services
	) {
		_ = services.AddSingleton<IQuadTreeFactory, SimpleQuadTreeFactory>();

		return services;
	}
}
