using System;
using Bizca.Sdk.Api.OpenId.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.Sdk.Api.OpenId.Extensions;

/// <summary>
/// Extension methods for registering Bizca OpenID Connect services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Bizca OpenID Connect JWT validation services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBizcaOpenId(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureOptions<OpenIdOptionsSetup>();
        return services;
    }

    /// <summary>
    /// Registers Bizca OpenID Connect JWT validation services with explicit configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBizcaOpenId(
        this IServiceCollection services,
        Action<OpenIdOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        return services;
    }
}

