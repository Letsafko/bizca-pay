using System;
using Bizca.Sdk.Api.OpenId.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Bizca.Sdk.Api.OpenId.Extensions;

/// <summary>
/// Extension methods for configuring Bizca OpenID Connect middleware.
/// </summary>
public static class WebApplicationExtensions
{
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

