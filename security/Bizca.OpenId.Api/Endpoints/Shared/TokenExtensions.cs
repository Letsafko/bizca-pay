using Bizca.OpenId.ApiModels.Responses;
using Bizca.OpenId.Application.Models;

namespace Bizca.OpenId.Api.Endpoints.Shared;

internal static class TokenExtensions
{
	internal static TokenViewModel ToViewModel(this TokenResponse tokenResponse)
	{
		return new TokenViewModel
		{
			AccessToken = tokenResponse.AccessToken,
			RefreshToken = tokenResponse.RefreshToken,
			ExpiresIn = tokenResponse.ExpiresIn
		};
	}
}