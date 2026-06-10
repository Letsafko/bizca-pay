using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Responses;

public sealed record VerifyEmailViewModel
{
	[Required, JsonPropertyName("success")]
	public bool Success { get; init; }

	[JsonPropertyName("message")]
	public string? Message { get; init; }
}

