using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Requests;

public sealed record RefreshTokenRequest
{
	[Required, JsonPropertyName("refresh_token")]
	public string? RefreshToken { get; init; }
}