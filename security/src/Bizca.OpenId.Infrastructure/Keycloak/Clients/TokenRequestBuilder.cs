using System.Collections.Generic;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class TokenRequestBuilder(IOptions<KeycloakOptions> options) : ITokenRequestBuilder
{
    private readonly KeycloakOptions _options = options.Value;

    public Dictionary<string, string> BuildAuthorizationCodeRequest(
        string code,
        string redirectUri,
        string? codeVerifier = null)
    {
        var parameters = new Dictionary<string, string>
        {
            [OAuth2KeycloakConstants.ParameterNames.GrantType] = OAuth2KeycloakConstants.GrantTypes.AuthorizationCode,
            [OAuth2KeycloakConstants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2KeycloakConstants.ParameterNames.ClientId] = _options.ClientId,
            [OAuth2KeycloakConstants.ParameterNames.RedirectUri] = redirectUri,
            [OAuth2KeycloakConstants.ParameterNames.Code] = code
        };

        if (!string.IsNullOrWhiteSpace(codeVerifier))
        {
            parameters[OAuth2KeycloakConstants.ParameterNames.CodeVerifier] = codeVerifier;
        }

        return parameters;
    }

    public Dictionary<string, string> BuildClientCredentialsRequest(bool withScopes = false)
    {
        var request = new Dictionary<string, string>
        {
            [OAuth2KeycloakConstants.ParameterNames.GrantType] = OAuth2KeycloakConstants.GrantTypes.ClientCredentials,
            [OAuth2KeycloakConstants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2KeycloakConstants.ParameterNames.ClientId] = _options.ClientId
        };

		if(withScopes)
		{
			request[OAuth2KeycloakConstants.ParameterNames.Scope] = _options.Scopes;
		}

		return request;
	}
	

    public Dictionary<string, string> BuildRefreshTokenRequest(string refreshToken)
    {
        return new Dictionary<string, string>
        {
            [OAuth2KeycloakConstants.ParameterNames.GrantType] = OAuth2KeycloakConstants.GrantTypes.RefreshToken,
            [OAuth2KeycloakConstants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2KeycloakConstants.ParameterNames.ClientId] = _options.ClientId,
            [OAuth2KeycloakConstants.ParameterNames.RefreshToken] = refreshToken
        };
    }

    public Dictionary<string, string> BuildRevokeTokenRequest(string token, string tokenTypeHint)
    {
        return new Dictionary<string, string>
        {
            [OAuth2KeycloakConstants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2KeycloakConstants.ParameterNames.TokenTypeHint] = tokenTypeHint,
            [OAuth2KeycloakConstants.ParameterNames.ClientId] = _options.ClientId,
            [OAuth2KeycloakConstants.ParameterNames.Token] = token
        };
    }
}


