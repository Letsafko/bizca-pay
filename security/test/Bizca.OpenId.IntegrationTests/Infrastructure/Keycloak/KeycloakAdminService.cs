using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak;
using Bizca.OpenId.Infrastructure.Keycloak.Models;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;

public sealed class KeycloakAdminService(
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
		var httpClient = httpClientFactory.CreateClient(Constant.HttpClientName);
		var content = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("grant_type", "client_credentials"),
			new KeyValuePair<string, string>("client_id", _keycloakOptions.ClientId),
			new KeyValuePair<string, string>("client_secret", _keycloakOptions.ClientSecret)
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
		var httpClient = httpClientFactory.CreateClient(Constant.HttpClientName);
		var content = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("grant_type", "password"),
			new KeyValuePair<string, string>("client_id", _keycloakOptions.ClientId),
			new KeyValuePair<string, string>("client_secret", _keycloakOptions.ClientSecret),
			new KeyValuePair<string, string>("username", TestUsername),
			new KeyValuePair<string, string>("password", TestPassword),
			new KeyValuePair<string, string>("scope", "openid offline_access")
		]);

		var response = await httpClient.PostAsync(
			$"realms/{_keycloakOptions.Realm}/protocol/openid-connect/token",
			content,
			cancellationToken);

		response.EnsureSuccessStatusCode();
		var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResult>(cancellationToken);

		return string.IsNullOrWhiteSpace(tokenResponse?.RefreshToken)
			? throw new InvalidOperationException("Failed to get refresh token - token is null or empty")
			: (tokenResponse.AccessToken, tokenResponse.RefreshToken);
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
		using var httpClient = httpClientFactory.CreateClient(Constant.HttpClientName);
		var content = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>("grant_type", "password"),
			new KeyValuePair<string, string>("client_id", "admin-cli"),
			new KeyValuePair<string, string>("username", Constant.OpenIdProvider.Keycloak.AdminUser),
			new KeyValuePair<string, string>("password", Constant.OpenIdProvider.Keycloak.AdminPassword)
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
		var httpClient = httpClientFactory.CreateClient(Constant.HttpClientName);
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
		return httpClient;
	}
}

