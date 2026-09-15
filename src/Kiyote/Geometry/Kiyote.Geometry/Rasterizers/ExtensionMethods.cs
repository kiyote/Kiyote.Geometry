using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Geometry.Rasterizers;

public static class ExtensionMethods {

	public static IServiceCollection AddRasterizer(
		this IServiceCollection services
	) {
		services.AddScoped<IRasterizer, IntegerRasterizer>();
		return services;
	}
}
