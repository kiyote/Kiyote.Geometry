using System.Diagnostics.CodeAnalysis;
using Kiyote.Geometry.DelaunayVoronoi;
using Kiyote.Geometry.Rasterizers;
using Kiyote.Geometry.Trees;
using Microsoft.Extensions.DependencyInjection;

namespace Kiyote.Geometry;

[ExcludeFromCodeCoverage]
public static class ExtensionMethods {

	public static IServiceCollection AddGeometry(
		this IServiceCollection services
	) {
		return services
			.AddDelaunayVoronoi()
			.AddRasterizer()
			.AddTrees();
	}
}
