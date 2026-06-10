using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Models;

namespace Bizca.OpenId.Application.Abstractions;

public interface ITokenProvider
{
	Task<TokenResponse> ExchangeCodeForTokenAsync(
		string code,
		string redirectUri,
		string? codeVerifier = null,
		CancellationToken cancellationToken = default);

	Task<TokenResponse> GetClientCredentialsTokenAsync(
		CancellationToken cancellationToken = default);

	Task<TokenResponse> RefreshTokenAsync(
		string refreshToken,
		CancellationToken cancellationToken = default);

	Task<bool> RevokeTokenAsync(
		string token,
		string tokenTypeHint = "refresh_token",
		CancellationToken cancellationToken = default);

	Task<UserInfoResult?> GetUserInfoAsync(
		string accessToken,
		CancellationToken cancellationToken = default);
}

public sealed record UserInfoResult(
	string Sub,
	string? Email,
	bool? EmailVerified,
	string? PreferredUsername,
	string? Name,
	string? GivenName,
	string? FamilyName);