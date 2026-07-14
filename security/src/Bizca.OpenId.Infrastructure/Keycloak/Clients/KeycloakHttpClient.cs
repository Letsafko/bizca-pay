using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;
using Bizca.OpenId.Infrastructure.Keycloak.Exceptions;
using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakHttpClient(IHttpClientFactory httpClientFactory) : IKeycloakHttpClient
{
	private readonly HttpClient _httpClient = httpClientFactory.CreateClient(OAuth2KeycloakConstants.KeycloakClientName);
	public async Task<TokenResult> RequestTokenAsync(
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(OAuth2KeycloakConstants.Endpoints.Token, content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
			return await KeycloakJsonContext.GetTokenResult(response, cancellationToken);
		}

		var errorResult = await KeycloakJsonContext.GetErrorResult(response, cancellationToken);
		throw new KeycloakException(errorResult.Error!, errorResult.ErrorDescription, (int)response.StatusCode);
    }

    public async Task<bool> RevokeTokenAsync(
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(parameters);
        var url = new Uri(OAuth2KeycloakConstants.Endpoints.Revoke, UriKind.Relative);

        var response = await _httpClient.PostAsync(url, content, cancellationToken);

		if(response.IsSuccessStatusCode)
		{
			return true;
		}

		var errorResult = await KeycloakJsonContext.GetErrorResult(response, cancellationToken);
		throw new KeycloakException(errorResult.Error!, errorResult.ErrorDescription, (int)response.StatusCode);
	}

    public async Task<UserInfoResult?> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri(OAuth2KeycloakConstants.Endpoints.UserInfo, UriKind.Relative);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(OAuth2KeycloakConstants.AuthorizationBearerScheme, accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

		if(response.IsSuccessStatusCode)
		{
			return await KeycloakJsonContext.GetUserInfoResult(response, cancellationToken);
		}

		var errorResult = await KeycloakJsonContext.GetErrorResult(response, cancellationToken);
		throw new KeycloakException(errorResult.Error!, errorResult.ErrorDescription, (int)response.StatusCode);
	}
}