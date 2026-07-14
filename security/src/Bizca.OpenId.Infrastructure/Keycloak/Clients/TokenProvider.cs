using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Application.Models;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class TokenProvider(
	IKeycloakTokenClient tokenClient,
	IKeycloakUserClient userClient) : ITokenProvider
{
	public async Task<TokenResponse> ExchangeCodeForTokenAsync(
		string code,
		string redirectUri,
		string? codeVerifier = null,
		CancellationToken cancellationToken = default)
	{
		var tokenResponse = await tokenClient.ExchangeCodeForTokenAsync(code, redirectUri, codeVerifier, cancellationToken);
		return GetIdentityProviderResult(tokenResponse);
	}

	public async Task<TokenResponse> GetClientCredentialsTokenAsync(CancellationToken cancellationToken = default)
	{
		var tokenResponse = await tokenClient.GetClientCredentialsTokenAsync(cancellationToken);
		return GetIdentityProviderResult(tokenResponse);
	}

	public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		var tokenResponse = await tokenClient.RefreshTokenAsync(refreshToken, cancellationToken);
		return GetIdentityProviderResult(tokenResponse);
	}

	public Task<bool> RevokeTokenAsync(
		string token,
		string tokenTypeHint = OAuth2KeycloakConstants.ParameterNames.RefreshToken,
		CancellationToken cancellationToken = default)
	{
		return tokenClient.RevokeTokenAsync(token, tokenTypeHint, cancellationToken);
	}

	public async Task<UserInfoResult?> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default)
	{
		var userInfoResponse = await userClient.GetUserInfoAsync(accessToken, cancellationToken);
		return userInfoResponse is null
			? null
			: new UserInfoResult(
				userInfoResponse.Sub,
				userInfoResponse.Email,
				userInfoResponse.EmailVerified,
				userInfoResponse.PreferredUsername,
				userInfoResponse.Name,
				userInfoResponse.GivenName,
				userInfoResponse.FamilyName);
	}

	private static TokenResponse GetIdentityProviderResult(Models.TokenResult tokenResult)
	{
		return new TokenResponse(
			tokenResult.IdToken,
			tokenResult.AccessToken,
			tokenResult.RefreshToken,
			tokenResult.Scope,
			tokenResult.TokenType,
			tokenResult.RefreshExpiresIn,
			tokenResult.ExpiresIn);
	}
}