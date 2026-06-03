namespace Bizca.OpenId.Application.Models;

public sealed record TokenResponse(
	string? IdToken,
	string AccessToken,
	string? RefreshToken,
	string? Scope,
	string TokenType,
	int? RefreshExpiresIn,
	int ExpiresIn);