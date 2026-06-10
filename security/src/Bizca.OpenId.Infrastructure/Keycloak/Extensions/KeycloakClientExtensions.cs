using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Clients;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.OpenId.Infrastructure.Keycloak.Extensions;

internal static class KeycloakClientExtensions
{
    public static void AddKeycloakClients(this IServiceCollection services)
    {
        services.AddScoped<ITokenRequestBuilder, TokenRequestBuilder>();
        services.AddScoped<IKeycloakHttpClient, KeycloakHttpClient>();
        services.AddScoped<IKeycloakTokenClient, KeycloakTokenClient>();
        services.AddScoped<IKeycloakUserClient, KeycloakUserClient>();
        services.AddScoped<ITokenProvider, TokenProvider>();
	}
}

