using System.Collections.Generic;
using Bizca.OpenId.Infrastructure.Constants;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
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
            [OAuth2Constants.ParameterNames.GrantType] = OAuth2GrantTypes.AuthorizationCode,
            [OAuth2Constants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2Constants.ParameterNames.ClientId] = _options.ClientId,
            [OAuth2Constants.ParameterNames.RedirectUri] = redirectUri,
            [OAuth2Constants.ParameterNames.Code] = code
        };

        if (!string.IsNullOrWhiteSpace(codeVerifier))
        {
            parameters[OAuth2Constants.ParameterNames.CodeVerifier] = codeVerifier;
        }

        return parameters;
    }

    public Dictionary<string, string> BuildClientCredentialsRequest()
    {
        return new Dictionary<string, string>
        {
            [OAuth2Constants.ParameterNames.GrantType] = OAuth2GrantTypes.ClientCredentials,
            [OAuth2Constants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2Constants.ParameterNames.ClientId] = _options.ClientId,
            [OAuth2Constants.ParameterNames.Scope] = _options.Scopes
        };
    }

    public Dictionary<string, string> BuildRefreshTokenRequest(string refreshToken)
    {
        return new Dictionary<string, string>
        {
            [OAuth2Constants.ParameterNames.GrantType] = OAuth2GrantTypes.RefreshToken,
            [OAuth2Constants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2Constants.ParameterNames.ClientId] = _options.ClientId,
            [OAuth2Constants.ParameterNames.RefreshToken] = refreshToken
        };
    }

    public Dictionary<string, string> BuildRevokeTokenRequest(string token, string tokenTypeHint)
    {
        return new Dictionary<string, string>
        {
            [OAuth2Constants.ParameterNames.ClientSecret] = _options.ClientSecret,
            [OAuth2Constants.ParameterNames.TokenTypeHint] = tokenTypeHint,
            [OAuth2Constants.ParameterNames.ClientId] = _options.ClientId,
            [OAuth2Constants.ParameterNames.Token] = token
        };
    }
}


