using Bizca.Sdk.Api.OpenId.Middleware;
using Bizca.Sdk.Api.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.Sdk.Api.OpenId;

/// <summary>
/// Extension methods for registering Bizca OpenID Connect services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Bizca OpenID Connect JWT validation services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBizcaOpenId(this IServiceCollection services)
    {
        services.AddOptionsWithValidation<OpenIdOptions>(OpenIdOptions.SectionName);
        return services;
    }

	/// <summary>
	/// Adds Bizca OpenID Connect JWT validation and claims enrichment middleware to the pipeline.
	/// Must be called BEFORE authorization middleware and AFTER routing.
	/// </summary>
	/// <param name="app">The web application.</param>
	/// <returns>The web application for chaining.</returns>
	public static WebApplication UseBizcaOpenId(this WebApplication app)
	{
		app.UseMiddleware<TokenValidationMiddleware>();
		app.UseMiddleware<ClaimsEnrichmentMiddleware>();

		return app;
	}
}
