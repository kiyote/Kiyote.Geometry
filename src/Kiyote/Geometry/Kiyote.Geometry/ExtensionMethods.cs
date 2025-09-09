using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Randomization;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Geometry.Trees;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kiyote.Geometry;

[ExcludeFromCodeCoverage]
public static class ExtensionMethods {

	public static IServiceCollection AddRasterizer(
		this IServiceCollection services
	) {
		 services.AddScoped<IRasterizer, IntegerRasterizer>();

		return services;
	}

	public static IServiceCollection AddRandomization(
		this IServiceCollection services
	) {
		 services.AddScoped<IPointFactory, FastPoissonDiscPointFactory>();
		 services.AddScoped<IRandom, FastRandom>();

		return services;
	}

	public static IServiceCollection AddDelaunayVoronoi(
		this IServiceCollection services
	) {
		 services.AddSingleton<IVoronoiFactory, D3VoronoiFactory>();
		 services.AddSingleton<IDelaunayFactory, D3DelaunayFactory>();
		services.TryAddSingleton<IQuadTreeFactory, SimpleQuadTreeFactory>();
		 services.AddSingleton<ISearchableVoronoiFactory, QuadTreeSearchableVoronoiFactory>();

		return services;
	}

	public static IServiceCollection AddTrees(
		this IServiceCollection services
	) {
		 services.AddSingleton<IQuadTreeFactory, SimpleQuadTreeFactory>();

		return services;
	}
}
