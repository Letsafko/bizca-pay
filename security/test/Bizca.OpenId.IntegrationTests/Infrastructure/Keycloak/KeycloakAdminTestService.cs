using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;
using Bizca.OpenId.Infrastructure.Keycloak.Models;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;

public sealed class KeycloakAdminTestService(
	IHttpClientFactory httpClientFactory,
	IOptions<KeycloakOptions> keycloakOptionsAccessor)
{
	private readonly KeycloakOptions _keycloakOptions = keycloakOptionsAccessor.Value;
	private const string TestUsername = "testuser";
	private const string TestPassword = "Test@1234";

	public async Task ConfigureRealmAndClientAsync(CancellationToken cancellationToken = default)
	{
		var adminToken = await GetAdminTokenAsync(cancellationToken);
		await CreateRealmAsync(adminToken, cancellationToken);
		await CreateClientAsync(adminToken, cancellationToken);
		await CreateTestUserAsync(adminToken, cancellationToken);
	}
	public async Task<string> GetClientCredentialsTokenAsync(CancellationToken cancellationToken = default)
	{
		var httpClient = httpClientFactory.CreateClient(Constant.KeycloakTestClientNameAdmin);
		var content = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.GrantType, OAuth2KeycloakConstants.GrantTypes.ClientCredentials),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.ClientId, _keycloakOptions.ClientId),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.ClientSecret, _keycloakOptions.ClientSecret)
		]);

		var response = await httpClient.PostAsync(
			$"realms/{_keycloakOptions.Realm}/protocol/openid-connect/token",
			content,
			cancellationToken);

		response.EnsureSuccessStatusCode();
		var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResult>(cancellationToken);
		return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get access token");
	}
	public async Task<(string AccessToken, string RefreshToken)> GetRefreshableTokenAsync(CancellationToken cancellationToken = default)
	{
		var httpClient = httpClientFactory.CreateClient(Constant.KeycloakTestClientNameAdmin);
		var content = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.GrantType, OAuth2KeycloakConstants.GrantTypes.Password),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.ClientId, _keycloakOptions.ClientId),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.ClientSecret, _keycloakOptions.ClientSecret),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.Username, TestUsername),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.Password, TestPassword),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.Scope, "openid offline_access")
		]);

		var response = await httpClient.PostAsync(
			requestUri: $"realms/{_keycloakOptions.Realm}/protocol/openid-connect/token",
			content: content,
			cancellationToken: cancellationToken);

		response.EnsureSuccessStatusCode();
		var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResult>(cancellationToken);

		return string.IsNullOrWhiteSpace(tokenResponse?.RefreshToken)
			? throw new InvalidOperationException("Failed to get refresh token - token is null or empty")
			: (tokenResponse.AccessToken, tokenResponse.RefreshToken);
	}

	private async Task CreateTestUserAsync(string accessToken, CancellationToken cancellationToken)
	{
		using var httpClient = CreateClientWithAuthorizationHeader(accessToken);
		var userRequest = new
		{
			username = TestUsername,
			enabled = true,
			emailVerified = true,
			email = "test@example.com",
			firstName = "Test",
			lastName = "User",
			credentials = new[]
			{
				new
				{
					type = "password",
					value = TestPassword,
					temporary = false
				}
			}
		};

		var response = await httpClient.PostAsJsonAsync(
			requestUri: $"admin/realms/{_keycloakOptions.Realm}/users",
			userRequest,
			cancellationToken);

		response.EnsureSuccessStatusCode();
	}
	private async Task CreateClientAsync(string accessToken, CancellationToken cancellationToken)
	{
		using var httpClient = CreateClientWithAuthorizationHeader(accessToken);
		var clientConfigRequest = new
		{
			secret = _keycloakOptions.ClientSecret,
			clientId = _keycloakOptions.ClientId,
			directAccessGrantsEnabled = true,
			serviceAccountsEnabled = true,
			redirectUris = new[] { "*" },
			implicitFlowEnabled = false,
			protocol = "openid-connect",
			webOrigins = new[] { "*" },
			standardFlowEnabled = true,
			publicClient = false,
			enabled = true
		};

		var response = await httpClient.PostAsJsonAsync(
			requestUri: $"admin/realms/{_keycloakOptions.Realm}/clients",
			clientConfigRequest,
			cancellationToken);

		response.EnsureSuccessStatusCode();
	}
	private async Task CreateRealmAsync(string accessToken, CancellationToken cancellationToken)
	{
		using var httpClient = CreateClientWithAuthorizationHeader(accessToken);
		var realmConfigRequest = new
		{
			realm = _keycloakOptions.Realm,
			accessTokenLifespan = 3600,
			refreshTokenMaxReuse = 0,
			sslRequired = "none",
			enabled = true
		};

		var response = await httpClient.PostAsJsonAsync("admin/realms", realmConfigRequest, cancellationToken);
		response.EnsureSuccessStatusCode();
	}
	private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
	{
		using var httpClient = httpClientFactory.CreateClient(Constant.KeycloakTestClientNameAdmin);
		var content = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.GrantType, OAuth2KeycloakConstants.GrantTypes.Password),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.Password, Constant.OpenIdProvider.Keycloak.AdminPassword),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.Username, Constant.OpenIdProvider.Keycloak.AdminUser),
			new KeyValuePair<string, string>(OAuth2KeycloakConstants.ParameterNames.ClientId, "admin-cli")
		]);

		var response = await httpClient.PostAsync(
			"realms/master/protocol/openid-connect/token",
			content,
			cancellationToken);

		response.EnsureSuccessStatusCode();
		var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResult>(cancellationToken);
		return tokenResponse!.AccessToken;
	}
	private HttpClient CreateClientWithAuthorizationHeader(string accessToken)
	{
		var httpClient = httpClientFactory.CreateClient(Constant.KeycloakTestClientNameAdmin);

		httpClient.DefaultRequestHeaders.Add(
			name: OAuth2KeycloakConstants.AuthorizationScheme,
			$"{OAuth2KeycloakConstants.AuthorizationBearerScheme} {accessToken}");

		return httpClient;
	}
}

