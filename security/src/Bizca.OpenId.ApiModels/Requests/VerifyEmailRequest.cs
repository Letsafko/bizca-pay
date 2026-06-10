using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Requests;

public sealed record VerifyEmailRequest
{
	[Required, JsonPropertyName("token")]
	public string? Token { get; init; }
}

