using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Requests;

public sealed record ExchangeTokenRequest
{
	[Required, JsonPropertyName("grant_type")]
	public string? GrantType { get; init; }

	[JsonPropertyName("code")]
	public string? Code { get; init; }

	[JsonPropertyName("redirect_uri")]
	public string? RedirectUri { get; init; }

	[JsonPropertyName("code_verifier")]
	public string? CodeVerifier { get; init; }
}