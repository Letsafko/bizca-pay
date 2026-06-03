using System.Text.Json.Serialization;

namespace Bizca.OpenId.Infrastructure.Keycloak.Models;

internal sealed record TokenErrorResult(
	[property: JsonPropertyName("error")] string? Error,
	[property: JsonPropertyName("error_description")] string? ErrorDescription);