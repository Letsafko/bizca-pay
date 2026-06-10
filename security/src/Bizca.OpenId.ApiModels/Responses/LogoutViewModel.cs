using System.Text.Json.Serialization;

namespace Bizca.OpenId.ApiModels.Responses;

public sealed record LogoutViewModel
{
	[JsonPropertyName("revoked")]
	public bool Revoked { get; init; }
}