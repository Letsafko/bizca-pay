using System.Text.Json.Serialization;

namespace Bizca.OpenId.Infrastructure.Keycloak.Models;

public sealed record TokenResult(
	[property: JsonPropertyName("access_token")] string AccessToken,
	[property: JsonPropertyName("token_type")] string TokenType,
	[property: JsonPropertyName("expires_in")] int ExpiresIn,
	[property: JsonPropertyName("refresh_token")] string? RefreshToken,
	[property: JsonPropertyName("refresh_expires_in")] int? RefreshExpiresIn,
	[property: JsonPropertyName("scope")] string? Scope,
	[property: JsonPropertyName("id_token")] string? IdToken);