using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Clients;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.OpenId.Infrastructure.Keycloak.Extensions;

internal static class KeycloakClientExtensions
{
    public static void AddKeycloakClients(this IServiceCollection services)
    {
		services.AddKeycloakAdminHttpClient();
		services.AddKeycloakHttpClient(OAuth2KeycloakConstants.KeycloakClientName);

        services.AddSingleton<ITokenRequestBuilder, TokenRequestBuilder>();
        services.AddSingleton<IKeycloakHttpClient, KeycloakHttpClient>();
        services.AddSingleton<IKeycloakTokenClient, KeycloakTokenClient>();
        services.AddSingleton<IKeycloakUserClient, KeycloakUserClient>();
        services.AddSingleton<IKeycloakAdminClient, KeycloakAdminClient>();
        services.AddSingleton<IIdentityProvider, KeycloakIdentityProvider>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
	}
}

