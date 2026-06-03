using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;

internal interface IKeycloakHttpClient
{
	Task<TokenResult> RequestTokenAsync(
		Dictionary<string, string> parameters,
		CancellationToken cancellationToken = default);

	Task<bool> RevokeTokenAsync(
		Dictionary<string, string> parameters,
		CancellationToken cancellationToken = default);

	Task<UserInfoResult?> GetUserInfoAsync(
		string accessToken,
		CancellationToken cancellationToken = default);
}