using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Geometry.Grids;

public static class ExtensionMethods {

	public static IServiceCollection AddGridAnalyzer(
		this IServiceCollection services
	) {
		return services.AddSingleton<IGridAnalyzer, GridAnalyzer>();
	}
}
