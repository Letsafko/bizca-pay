using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakUserClient(IKeycloakHttpClient keycloakHttpClient) : IKeycloakUserClient
{
	public async Task<UserInfoResult?> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return await keycloakHttpClient.GetUserInfoAsync(accessToken, cancellationToken);
    }
}