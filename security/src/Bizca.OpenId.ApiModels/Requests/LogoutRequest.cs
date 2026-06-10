using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Requests;

public sealed record LogoutRequest
{
	[Required, JsonPropertyName("token")]
	public string? Token { get; init; }

	[JsonPropertyName("token_type_hint")]
	public string? TokenTypeHint { get; init; }
}