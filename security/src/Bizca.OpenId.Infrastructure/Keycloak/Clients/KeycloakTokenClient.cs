using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Constants;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakTokenClient(
	IKeycloakHttpClient keycloakHttpClient,
	ITokenRequestBuilder tokenRequestBuilder) : IKeycloakTokenClient
{
	public Task<TokenResult> ExchangeCodeForTokenAsync(
		string code,
		string redirectUri,
		string? codeVerifier = null,
		CancellationToken cancellationToken = default)
	{
		var parameters = tokenRequestBuilder.BuildAuthorizationCodeRequest(code, redirectUri, codeVerifier);
		return keycloakHttpClient.RequestTokenAsync(parameters, cancellationToken);
	}

	public Task<TokenResult> GetClientCredentialsTokenAsync(CancellationToken cancellationToken = default)
	{
		var parameters = tokenRequestBuilder.BuildClientCredentialsRequest();
		return keycloakHttpClient.RequestTokenAsync(parameters, cancellationToken);
	}

	public Task<TokenResult> RefreshTokenAsync(
		string refreshToken,
		CancellationToken cancellationToken = default)
	{
		var parameters = tokenRequestBuilder.BuildRefreshTokenRequest(refreshToken);
		return keycloakHttpClient.RequestTokenAsync(parameters, cancellationToken);
	}

	public Task<bool> RevokeTokenAsync(
		string token,
		string tokenTypeHint = OAuth2Constants.ParameterNames.RefreshToken,
		CancellationToken cancellationToken = default)
	{
		var parameters = tokenRequestBuilder.BuildRevokeTokenRequest(token, tokenTypeHint);
		return keycloakHttpClient.RevokeTokenAsync(parameters, cancellationToken);
	}
}