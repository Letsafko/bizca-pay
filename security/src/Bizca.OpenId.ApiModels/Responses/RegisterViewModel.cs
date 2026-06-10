using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Responses;

public sealed record RegisterViewModel
{
	[Required, JsonPropertyName("user_id")]
	public string? UserId { get; init; }

	[JsonPropertyName("message")]
	public string? Message { get; init; }
}

