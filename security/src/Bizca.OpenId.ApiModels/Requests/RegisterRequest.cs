using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Requests;

public sealed record RegisterRequest
{
	[Required, JsonPropertyName("username")]
	public string? Username { get; init; }

	[Required, JsonPropertyName("email")]
	public string? Email { get; init; }

	[Required, JsonPropertyName("password")]
	public string? Password { get; init; }

	[JsonPropertyName("first_name")]
	public string? FirstName { get; init; }

	[JsonPropertyName("last_name")]
	public string? LastName { get; init; }
}

