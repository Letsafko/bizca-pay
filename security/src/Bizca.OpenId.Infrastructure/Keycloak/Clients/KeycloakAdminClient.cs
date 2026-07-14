using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;
using Bizca.OpenId.Infrastructure.Keycloak.Exceptions;
using Bizca.OpenId.Infrastructure.Keycloak.Models;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakAdminClient(
	IHttpClientFactory httpClientFactory,
	IOptions<KeycloakOptions> keycloakOptionsAccessor) : IKeycloakAdminClient
{
	private readonly HttpClient _httpClient = httpClientFactory.CreateClient(OAuth2KeycloakConstants.KeycloakClientNameAdmin);
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
					type = OAuth2KeycloakConstants.GrantTypes.Password,
					value = password,
					temporary = false
				}
			}
		};

		var url = string.Format(
			CultureInfo.InvariantCulture,
			OAuth2KeycloakConstants.Endpoints.Admin.CreateUserCompositeFormat,
			_options.Realm);

		using var request = new HttpRequestMessage(HttpMethod.Post, url);
		request.Content = JsonContent.Create(userRequest);

		var response = await _httpClient.SendAsync(request, cancellationToken);

		if (response.IsSuccessStatusCode)
		{
			var locationHeader = response.Headers.Location?.ToString();
			if (string.IsNullOrWhiteSpace(locationHeader))
			{
				throw new KeycloakException(
					"USER_ID_NOT_FOUND",
					"Failed to extract user ID from Keycloak response",
					500);
			}

			var segments = locationHeader.Split('/');
			var userId = segments[^1];
			return userId;
		}

		var errorResult = await KeycloakJsonContext.GetErrorResult(response, cancellationToken);
		throw new KeycloakException(errorResult.Error!, errorResult.ErrorDescription, (int)response.StatusCode);
	}

	public async Task SendVerifyEmailActionAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		var url = string.Format(
			CultureInfo.InvariantCulture,
			OAuth2KeycloakConstants.Endpoints.Admin.SendUserEmailVerificationCompositeFormat,
			_options.Realm, userId);

		using var request = new HttpRequestMessage(HttpMethod.Put, url);
		request.Content = JsonContent.Create(VerifyEmailAction);

		var response = await _httpClient.SendAsync(request, cancellationToken);
		if (response.IsSuccessStatusCode)
		{
			return;
		}

		var errorResult = await KeycloakJsonContext.GetErrorResult(response, cancellationToken);
		throw new KeycloakException(
			errorResult.Error!,
			errorResult.ErrorDescription,
			(int)response.StatusCode);
	}

	public async Task UpdateEmailVerifiedAsync(
		string userId,
		bool emailVerified,
		CancellationToken cancellationToken = default)
	{
		var url = string.Format(
			CultureInfo.InvariantCulture,
			OAuth2KeycloakConstants.Endpoints.Admin.UpdateEmailVerificationCompositeFormat,
			_options.Realm, userId);

		using var request = new HttpRequestMessage(HttpMethod.Put, url);
		request.Content = JsonContent.Create(new { emailVerified });

		var response = await _httpClient.SendAsync(request, cancellationToken);

		if (response.IsSuccessStatusCode)
		{
			return;
		}

		var errorResult = await KeycloakJsonContext.GetErrorResult(response, cancellationToken);
		throw new KeycloakException(errorResult.Error!, errorResult.ErrorDescription, (int)response.StatusCode);
	}

	public async Task UpdateUserEnabledAsync(
		string userId,
		bool enabled,
		CancellationToken cancellationToken = default)
	{
		var url = string.Format(
			CultureInfo.InvariantCulture,
			OAuth2KeycloakConstants.Endpoints.Admin.UpdateUserEnabledCompositeFormat,
			_options.Realm, userId);

		using var request = new HttpRequestMessage(HttpMethod.Put, url);
		request.Content = JsonContent.Create(new { enabled });

		var response = await _httpClient.SendAsync(request, cancellationToken);
		if (response.IsSuccessStatusCode)
		{
			return;
		}

		var errorResult = await KeycloakJsonContext.GetErrorResult(response, cancellationToken);
		throw new KeycloakException(errorResult.Error!, errorResult.ErrorDescription, (int)response.StatusCode);
	}
}