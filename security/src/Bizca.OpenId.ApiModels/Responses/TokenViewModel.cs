using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Responses;

public sealed record TokenViewModel
{
	[Required, JsonPropertyName("access_token")]
	public string? AccessToken { get; init; }

	[JsonPropertyName("refresh_token")]
	public string? RefreshToken { get; init; }

	[JsonPropertyName("expires_in")]
	public TimeSpan ExpiresIn { get; init; }
}