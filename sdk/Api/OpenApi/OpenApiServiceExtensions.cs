using Asp.Versioning;
using Bizca.Sdk.Api.OpenApi.Transformers;
using Bizca.Sdk.Api.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
	/// <param name="configuration">The application configuration used to bind <see cref="OpenApiOptions"/> from <c>appsettings.json</c>.</param>
	/// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
	public static void AddBizcaOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new OpenApiOptions();
        configuration.GetSection(nameof(OpenApiOptions)).Bind(options);
		services.AddOptionsWithValidation<OpenApiOptions>(OpenApiOptions.SectionName);
        services.AddApiVersioning(o =>
        {
            o.DefaultApiVersion = ApiVersion.Default;
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.ReportApiVersions = true;
        });

        var securityTransformer = options.EnableBearerSecurity
            ? new BearerSecuritySchemeTransformer(options)
            : null;

        foreach (var version in options.Versions)
        {
            services.AddOpenApi(version, openApiOptions =>
            {
                openApiOptions.AddDocumentTransformer(new DocumentInfoTransformer(options, version));
                if (securityTransformer is not null)
                {
                    openApiOptions.AddDocumentTransformer(securityTransformer);
                }
            });
        }
	}

	/// <summary>
	/// Maps OpenAPI spec endpoints (<c>/openapi/{version}.json</c>) and the Scalar interactive UI.
	/// Both are exposed only in <c>Development</c> and <c>Local</c> environments.
	/// </summary>
	/// <param name="app">The <see cref="WebApplication"/> to configure.</param>
	/// <returns>The same <see cref="WebApplication"/> for chaining.</returns>
	public static void UseBizcaOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Local"))
		{
			return;
		}

        app.MapOpenApi();
        var options = app.Services.GetRequiredService<IOptions<OpenApiOptions>>().Value;
        app.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions
                .WithTitle(options.Title ?? string.Empty)
                .AddDocuments(options.Versions);
        });
	}
}
