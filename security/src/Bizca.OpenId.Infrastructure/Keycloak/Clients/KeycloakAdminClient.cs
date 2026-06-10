using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Constants;
using Bizca.OpenId.Infrastructure.Keycloak;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Exceptions;
using Bizca.OpenId.Infrastructure.Keycloak.Models;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakAdminClient(
	IHttpClientFactory httpClientFactory,
	IOptions<KeycloakOptions> keycloakOptionsAccessor) : IKeycloakAdminClient
{
	private readonly HttpClient _httpClient = httpClientFactory.CreateClient(OAuth2Constants.Keycloak);
	private readonly KeycloakOptions _options = keycloakOptionsAccessor.Value;

	private static readonly string[] VerifyEmailAction = ["VERIFY_EMAIL"];

	public async Task<string> CreateUserAsync(
		string username,
		string email,
		string password,
		string? firstName,
		string? lastName,
		bool emailVerified,
		bool enabled,
		CancellationToken cancellationToken = default)
	{
		var adminToken = await GetAdminAccessTokenAsync(cancellationToken);

		var userRequest = new
		{
			username,
			email,
			emailVerified,
			enabled,
			firstName = firstName ?? string.Empty,
			lastName = lastName ?? string.Empty,
			credentials = new[]
			{
				new
				{
					type = "password",
					value = password,
					temporary = false
				}
			}
		};

		using var request = new HttpRequestMessage(HttpMethod.Post, $"admin/realms/{_options.Realm}/users");
		request.Headers.Add("Authorization", $"Bearer {adminToken}");
		request.Content = JsonContent.Create(userRequest);

		var response = await _httpClient.SendAsync(request, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
			throw new KeycloakException(
				"USER_CREATION_FAILED",
				$"Failed to create user in Keycloak: {errorContent}",
				(int)response.StatusCode);
		}

		// Extract user ID from Location header (Keycloak returns it on successful creation)
		var locationHeader = response.Headers.Location?.ToString();
		if (string.IsNullOrWhiteSpace(locationHeader))
		{
			throw new KeycloakException(
				"USER_ID_NOT_FOUND",
				"Failed to extract user ID from Keycloak response",
				500);
		}

		var segments = locationHeader.Split('/');
		var userId = segments[segments.Length - 1];
		return userId;
	}

	public async Task SendVerifyEmailActionAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		var adminToken = await GetAdminAccessTokenAsync(cancellationToken);

		using var request = new HttpRequestMessage(
			HttpMethod.Put,
			$"admin/realms/{_options.Realm}/users/{userId}/execute-actions-email");
		request.Headers.Add("Authorization", $"Bearer {adminToken}");
		request.Content = JsonContent.Create(VerifyEmailAction);

		var response = await _httpClient.SendAsync(request, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
			throw new KeycloakException(
				"EMAIL_VERIFICATION_SEND_FAILED",
				$"Failed to send verification email: {errorContent}",
				(int)response.StatusCode);
		}
	}

	public async Task UpdateEmailVerifiedAsync(
		string userId,
		bool emailVerified,
		CancellationToken cancellationToken = default)
	{
		var adminToken = await GetAdminAccessTokenAsync(cancellationToken);

		using var request = new HttpRequestMessage(
			HttpMethod.Put,
			$"admin/realms/{_options.Realm}/users/{userId}");
		request.Headers.Add("Authorization", $"Bearer {adminToken}");
		request.Content = JsonContent.Create(new { emailVerified });

		var response = await _httpClient.SendAsync(request, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
			throw new KeycloakException(
				"EMAIL_VERIFICATION_UPDATE_FAILED",
				$"Failed to update email verification status: {errorContent}",
				(int)response.StatusCode);
		}
	}

	public async Task UpdateUserEnabledAsync(
		string userId,
		bool enabled,
		CancellationToken cancellationToken = default)
	{
		var adminToken = await GetAdminAccessTokenAsync(cancellationToken);

		using var request = new HttpRequestMessage(
			HttpMethod.Put,
			$"admin/realms/{_options.Realm}/users/{userId}");
		request.Headers.Add("Authorization", $"Bearer {adminToken}");
		request.Content = JsonContent.Create(new { enabled });

		var response = await _httpClient.SendAsync(request, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
			throw new KeycloakException(
				"USER_ENABLED_UPDATE_FAILED",
				$"Failed to update user enabled status: {errorContent}",
				(int)response.StatusCode);
		}
	}

	public async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken = default)
	{
		var content = new FormUrlEncodedContent(
		[
			new KeyValuePair<string, string>(OAuth2Constants.ParameterNames.GrantType, "client_credentials"),
			new KeyValuePair<string, string>(OAuth2Constants.ParameterNames.ClientId, _options.ClientId),
			new KeyValuePair<string, string>(OAuth2Constants.ParameterNames.ClientSecret, _options.ClientSecret)
		]);

		var response = await _httpClient.PostAsync(
			OAuth2Constants.Endpoints.Token,
			content,
			cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
			throw new KeycloakException(
				"ADMIN_TOKEN_FAILED",
				$"Failed to get admin access token: {errorContent}",
				(int)response.StatusCode);
		}

		var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResult>(cancellationToken);
		return tokenResponse?.AccessToken ?? throw new KeycloakException(
			"ADMIN_TOKEN_NULL",
			"Admin access token is null",
			500);
	}
}





