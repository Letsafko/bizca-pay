using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Bizca.OpenId.Infrastructure.Keycloak.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UserInfoResult))]
[JsonSerializable(typeof(TokenResult))]
[JsonSerializable(typeof(ErrorResult))]
internal sealed partial class KeycloakJsonContext : JsonSerializerContext
{
	internal static async Task<ErrorResult> GetErrorResult(
		HttpResponseMessage response,
		CancellationToken ct = default)
	{
		return await JsonSerializer.DeserializeAsync(
					await response.Content.ReadAsStreamAsync(ct),
					Default.ErrorResult, ct)
				?? throw new InvalidOperationException("Keycloak returned an empty token response.");
	}

	internal static async Task<TokenResult> GetTokenResult(
		HttpResponseMessage response,
		CancellationToken ct = default)
	{
		return await JsonSerializer.DeserializeAsync(
					await response.Content.ReadAsStreamAsync(ct),
					Default.TokenResult, ct)
				?? throw new InvalidOperationException("Keycloak returned an empty token response.");
	}

	internal static async Task<UserInfoResult> GetUserInfoResult(
		HttpResponseMessage response,
		CancellationToken ct = default)
	{
		return await JsonSerializer.DeserializeAsync(
					await response.Content.ReadAsStreamAsync(ct),
					Default.UserInfoResult, ct)
				?? throw new InvalidOperationException("Keycloak returned an empty token response.");
	}
}