using System;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.OpenId.Application.Models;

namespace Bizca.OpenId.Auth.Endpoints.Tokens;

internal static class TokenExtensions
{
	internal static TokenViewModel ToViewModel(this TokenResponse tokenResponse)
	{
		return new TokenViewModel
		{
			AccessToken = tokenResponse.AccessToken,
			RefreshToken = tokenResponse.RefreshToken,
			ExpiresIn = TimeSpan.FromSeconds(tokenResponse.ExpiresIn)
		};
	}
}