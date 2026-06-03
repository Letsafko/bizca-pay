using System.Net.Http.Headers;
using System.Text.Json;
using Bizca.OpenId.Infrastructure.Constants;
using Bizca.OpenId.Infrastructure.Exceptions;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakHttpClient(IHttpClientFactory httpClientFactory) : IKeycloakHttpClient
{
	private readonly HttpClient _httpClient = httpClientFactory.CreateClient(OAuth2Constants.Keycloak);
	public async Task<TokenResult> RequestTokenAsync(
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(
            OAuth2Constants.Endpoints.Token,
            content,
            cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = JsonSerializer.Deserialize<TokenErrorResult>(json);
            throw new TechnicalException(
                error?.Error ?? OAuth2Constants.UnknownError,
                error?.ErrorDescription ?? OAuth2Constants.DefaultErrorMessage);
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResult>(json);
        return tokenResponse ?? throw new TechnicalException(OAuth2Constants.InvalidResponse, OAuth2Constants.InvalidResponseMessage);
    }

    public async Task<bool> RevokeTokenAsync(
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(parameters);
        var url = new Uri(OAuth2Constants.Endpoints.Revoke, UriKind.Relative);
        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfoResult?> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri(OAuth2Constants.Endpoints.UserInfo, UriKind.Relative);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(OAuth2Constants.AuthenticationScheme, accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<UserInfoResult>(json);
    }
}