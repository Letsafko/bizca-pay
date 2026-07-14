using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;

public interface IKeycloakTokenClient
{
	Task<TokenResult> ExchangeCodeForTokenAsync(
		string code,
		string redirectUri,
		string? codeVerifier = null,
		CancellationToken cancellationToken = default);

	Task<TokenResult> GetClientCredentialsTokenAsync(
		CancellationToken cancellationToken = default);

	Task<TokenResult> RefreshTokenAsync(
		string refreshToken,
		CancellationToken cancellationToken = default);

	Task<bool> RevokeTokenAsync(
		string token,
		string tokenTypeHint = "refresh_token",
		CancellationToken cancellationToken = default);
}