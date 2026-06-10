using System.Collections.Generic;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;

internal interface ITokenRequestBuilder
{
	Dictionary<string, string> BuildAuthorizationCodeRequest(
		string code,
		string redirectUri,
		string? codeVerifier = null);

	Dictionary<string, string> BuildClientCredentialsRequest();

	Dictionary<string, string> BuildRefreshTokenRequest(string refreshToken);

	Dictionary<string, string> BuildRevokeTokenRequest(string token, string tokenTypeHint);
}