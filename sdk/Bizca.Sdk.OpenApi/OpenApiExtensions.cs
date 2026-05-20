using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Bizca.Sdk.OpenApi;

/// <summary>
/// Extension methods for configuring OpenAPI in a Bizca service.
/// </summary>
public static class OpenApiExtensions
{
	/// <summary>
	/// Adds OpenAPI document generation services to the service collection.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="configure">An optional action to configure <see cref="OpenApiOptions"/>.</param>
	/// <returns>The <see cref="IServiceCollection"/> so calls can be chained.</returns>
	public static IServiceCollection AddBizcaOpenApi(
		this IServiceCollection services,
		Action<OpenApiOptions>? configure = null)
	{
		services.AddOpenApi(options =>
		{
			configure?.Invoke(options);
		});

		return services;
	}

	/// <summary>
	/// Maps the OpenAPI document endpoint and exposes the Scalar interactive UI.
	/// </summary>
	/// <param name="app">The <see cref="WebApplication"/> instance.</param>
	/// <returns>The <see cref="WebApplication"/> so calls can be chained.</returns>
	public static WebApplication UseBizcaOpenApi(this WebApplication app)
	{
		app.MapOpenApi();
		app.MapScalarApiReference();
		return app;
	}
}
