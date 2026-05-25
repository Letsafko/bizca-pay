using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Bizca.OpenId.Api.Options;

namespace Bizca.OpenId.Api.Keycloak;

/// <summary>
/// HTTP client for Keycloak token and user management operations.
/// </summary>
public sealed class KeycloakClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;

    public KeycloakClient(HttpClient httpClient, IOptions<KeycloakOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.Authority);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.HttpTimeoutSeconds);
    }

    /// <summary>
    /// Exchanges authorization code for access token (Authorization Code flow).
    /// </summary>
    public async Task<TokenResponse> ExchangeCodeForTokenAsync(
        string code,
        string redirectUri,
        string? codeVerifier = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = redirectUri
        };

        if (!string.IsNullOrEmpty(codeVerifier))
        {
            parameters["code_verifier"] = codeVerifier;
        }

        return await RequestTokenAsync(parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a token using Client Credentials flow.
    /// </summary>
    public async Task<TokenResponse> GetClientCredentialsTokenAsync(
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["scope"] = _options.Scopes
        };

        return await RequestTokenAsync(parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    public async Task<TokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = refreshToken
        };

        return await RequestTokenAsync(parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Revokes a token (logout).
    /// </summary>
    public async Task<bool> RevokeTokenAsync(
        string token,
        string tokenTypeHint = "refresh_token",
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["token"] = token,
            ["token_type_hint"] = tokenTypeHint
        };

        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(
            $"/protocol/openid-connect/revoke",
            content,
            cancellationToken).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Retrieves user info from the userinfo endpoint.
    /// </summary>
    public async Task<UserInfoResponse?> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/protocol/openid-connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<UserInfoResponse>(json);
    }

    private async Task<TokenResponse> RequestTokenAsync(
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpClient.PostAsync(
            "/protocol/openid-connect/token",
            content,
            cancellationToken).ConfigureAwait(false);

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var error = JsonSerializer.Deserialize<TokenErrorResponse>(json);
            throw new KeycloakException(
                error?.Error ?? "unknown_error",
                error?.ErrorDescription ?? "An unknown error occurred");
        }

        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);
        return tokenResponse ?? throw new KeycloakException("invalid_response", "Failed to deserialize token response");
    }
}

/// <summary>
/// Token response from Keycloak.
/// </summary>
public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string? RefreshToken,
    int? RefreshExpiresIn,
    string? Scope,
    string? IdToken
)
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = AccessToken;

    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string TokenType { get; init; } = TokenType;

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; } = ExpiresIn;

    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; } = RefreshToken;

    [System.Text.Json.Serialization.JsonPropertyName("refresh_expires_in")]
    public int? RefreshExpiresIn { get; init; } = RefreshExpiresIn;

    [System.Text.Json.Serialization.JsonPropertyName("scope")]
    public string? Scope { get; init; } = Scope;

    [System.Text.Json.Serialization.JsonPropertyName("id_token")]
    public string? IdToken { get; init; } = IdToken;
}

/// <summary>
/// User info response from Keycloak.
/// </summary>
public sealed record UserInfoResponse(
    string Sub,
    string? Email,
    bool? EmailVerified,
    string? PreferredUsername,
    string? Name,
    string? GivenName,
    string? FamilyName
)
{
    [System.Text.Json.Serialization.JsonPropertyName("sub")]
    public string Sub { get; init; } = Sub;

    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string? Email { get; init; } = Email;

    [System.Text.Json.Serialization.JsonPropertyName("email_verified")]
    public bool? EmailVerified { get; init; } = EmailVerified;

    [System.Text.Json.Serialization.JsonPropertyName("preferred_username")]
    public string? PreferredUsername { get; init; } = PreferredUsername;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; init; } = Name;

    [System.Text.Json.Serialization.JsonPropertyName("given_name")]
    public string? GivenName { get; init; } = GivenName;

    [System.Text.Json.Serialization.JsonPropertyName("family_name")]
    public string? FamilyName { get; init; } = FamilyName;
}

/// <summary>
/// Token error response from Keycloak.
/// </summary>
internal sealed record TokenErrorResponse(
    string? Error,
    string? ErrorDescription
)
{
    [System.Text.Json.Serialization.JsonPropertyName("error")]
    public string? Error { get; init; } = Error;

    [System.Text.Json.Serialization.JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; } = ErrorDescription;
}

/// <summary>
/// Exception thrown when a Keycloak operation fails.
/// </summary>
public sealed class KeycloakException : Exception
{
    public string ErrorCode { get; }

    public KeycloakException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}


