using System;
using Asp.Versioning;
using Bizca.Sdk.Api.OpenApi.Transformers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Bizca.Sdk.Api.OpenApi;

/// <summary>
/// Extension methods for registering and mapping the Bizca OpenAPI layer.
/// </summary>
public static class OpenApiServiceExtensions
{
    /// <summary>
    /// Registers versioned OpenAPI document generation, API versioning, and optional
    /// Bearer security — ready to consume in any Bizca microservice API.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">Optional delegate to override <see cref="BizcaOpenApiOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddBizcaOpenApi(
        this IServiceCollection services,
        Action<BizcaOpenApiOptions>? configure = null)
    {
        var options = new BizcaOpenApiOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        services.AddApiVersioning(o =>
        {
            o.DefaultApiVersion = ApiVersion.Default;
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.ReportApiVersions = true;
        });

        // Single instance reused across all version documents — stateless transformer
        var securityTransformer = options.EnableBearerSecurity
            ? new BearerSecuritySchemeTransformer(options)
            : null;

        foreach (var version in options.Versions)
        {
            var infoTransformer = new DocumentInfoTransformer(options, version);

            services.AddOpenApi(version, openApiOptions =>
            {
                openApiOptions.AddDocumentTransformer(infoTransformer);

                if (securityTransformer is not null)
				{
					openApiOptions.AddDocumentTransformer(securityTransformer);
				}
			});
        }

        return services;
    }

    /// <summary>
    /// Maps OpenAPI spec endpoints (<c>/openapi/{version}.json</c>) and the Scalar interactive UI.
    /// Both are exposed only in <c>Development</c> and <c>Local</c> environments.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication UseBizcaOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Local"))
		{
			return app;
		}

		app.MapOpenApi();

        var options = app.Services.GetRequiredService<BizcaOpenApiOptions>();

        app.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions.WithTitle(options.Title);

            foreach (var version in options.Versions)
			{
				scalarOptions.AddServer(new ScalarServer($"/openapi/{version}.json", $"{options.Title} {version}"));
			}
		});

        return app;
    }
}
