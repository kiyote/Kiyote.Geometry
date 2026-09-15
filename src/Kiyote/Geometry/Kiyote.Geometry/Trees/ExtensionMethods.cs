using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Geometry.Trees;

public static class ExtensionMethods {

	public static IServiceCollection AddTrees(
		this IServiceCollection services
	) {
		services.AddSingleton<IQuadTreeFactory, SimpleQuadTreeFactory>();

		return services;
	}

}
