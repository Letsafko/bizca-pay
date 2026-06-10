using System.Text.Json.Serialization;

namespace Bizca.OpenId.Infrastructure.Keycloak.Models;
public sealed record UserInfoResult(
	[property: JsonPropertyName("sub")] string Sub,
	[property: JsonPropertyName("email")] string? Email,
	[property: JsonPropertyName("email_verified")] bool? EmailVerified,
	[property: JsonPropertyName("preferred_username")] string? PreferredUsername,
	[property: JsonPropertyName("name")] string? Name,
	[property: JsonPropertyName("given_name")] string? GivenName,
	[property: JsonPropertyName("family_name")] string? FamilyName);