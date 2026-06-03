using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;

public interface IKeycloakUserClient
{
	Task<UserInfoResult?> GetUserInfoAsync(
		string accessToken,
		CancellationToken cancellationToken = default);
}